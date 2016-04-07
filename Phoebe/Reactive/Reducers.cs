﻿using System;
using System.Collections.Generic;
using System.Linq;
using Toggl.Phoebe.Data;
using Toggl.Phoebe.Data.Models;
using Toggl.Phoebe.Helpers;
using Toggl.Phoebe.Logging;
using Toggl.Phoebe.Net;
using XPlatUtils;

namespace Toggl.Phoebe.Reactive
{
    public interface IReducer
    {
        DataSyncMsg<object> Reduce(object state, DataMsg msg);
    }

    public class Reducer<T> : IReducer
    {
        readonly Func<T, DataMsg, DataSyncMsg<T>> reducer;

        public virtual DataSyncMsg<T> Reduce(T state, DataMsg msg)
        {
            return reducer(state, msg);
        }

        DataSyncMsg<object> IReducer.Reduce(object state, DataMsg msg)
        {
            return Reduce((T)state, msg).Cast<object> ();
        }

        protected Reducer() { }

        public Reducer(Func<T, DataMsg, DataSyncMsg<T>> reducer)
        {
            this.reducer = reducer;
        }
    }

    public class TagCompositeReducer<T> : Reducer<T>, IReducer
    {
        readonly Dictionary<Type, Reducer<T>> reducers = new Dictionary<Type, Reducer<T>> ();

        public TagCompositeReducer<T> Add(Type msgType, Func<T, DataMsg, DataSyncMsg<T>> reducer)
        {
            return Add(msgType, new Reducer<T> (reducer));
        }

        public TagCompositeReducer<T> Add(Type msgType, Reducer<T> reducer)
        {
            reducers.Add(msgType, reducer);
            return this;
        }

        public override DataSyncMsg<T> Reduce(T state, DataMsg msg)
        {
            Reducer<T> reducer;
            if (reducers.TryGetValue(msg.GetType(), out reducer))
            {
                return reducer.Reduce(state, msg);
            }
            else
            {
                return DataSyncMsg.Create(state);
            }
        }

        DataSyncMsg<object> IReducer.Reduce(object state, DataMsg msg)
        {
            return Reduce((T)state, msg).Cast<object> ();
        }
    }

    public static class Reducers
    {
        public static Reducer<AppState> Init()
        {
            return new TagCompositeReducer<AppState> ()
                   .Add(typeof(DataMsg.ServerRequest), ServerRequest)
                   .Add(typeof(DataMsg.ServerResponse), ServerResponse)
                   .Add(typeof(DataMsg.TimeEntriesLoad), TimeEntriesLoad)
                   .Add(typeof(DataMsg.TimeEntryPut), TimeEntryPut)
                   .Add(typeof(DataMsg.TimeEntriesRemove), TimeEntryRemove)
                   .Add(typeof(DataMsg.TimeEntryContinue), TimeEntryContinue)
                   .Add(typeof(DataMsg.TimeEntryStop), TimeEntryStop)
                   .Add(typeof(DataMsg.TagsPut), TagsPut)
                   .Add(typeof(DataMsg.ClientDataPut), ClientDataPut)
                   .Add(typeof(DataMsg.ProjectDataPut), ProjectDataPut)
                   .Add(typeof(DataMsg.UserDataPut), UserDataPut)
                   .Add(typeof(DataMsg.ResetState), Reset)
                   .Add(typeof(DataMsg.UpdateSetting), UpdateSettings);
        }

        static DataSyncMsg<AppState> ServerRequest(AppState state, DataMsg msg)
        {
            var req = (msg as DataMsg.ServerRequest).Data;

            var reqInfo = state.RequestInfo.With(running: state.RequestInfo.Running.Append(req).ToList());
            if (req is ServerRequest.GetChanges)
                reqInfo = reqInfo.With(getChangesLastRun: state.Settings.GetChangesLastRun);

            return DataSyncMsg.Create(req, state.With(requestInfo: reqInfo));
        }

        static DataSyncMsg<AppState> TimeEntriesLoad(AppState state, DataMsg msg)
        {
            var dataStore = ServiceContainer.Resolve<ISyncDataStore> ();
            var endDate = state.RequestInfo.NextDownloadFrom;

            var startDate = GetDatesByDays(dataStore, endDate, Literals.TimeEntryLoadDays);
            var dbEntries = dataStore
                            .Table<TimeEntryData> ()
                            .Where(r =>
                                   r.State != TimeEntryState.New &&
                                   r.StartTime >= startDate && r.StartTime < endDate &&
                                   r.DeletedAt == null)
                            // TODO TODO TODO: Rx why the entries are saved without local user ID.
                            //r.UserId == state.User.Id)
                            .Take(Literals.TimeEntryLoadMaxInit)
                            .OrderByDescending(r => r.StartTime)
                            .ToList();

            var req = new ServerRequest.DownloadEntries();
            var reqInfo = state.RequestInfo.With(
                              running: state.RequestInfo.Running.Append(req).ToList(),
                              downloadFrom: endDate,
                              nextDownloadFrom: dbEntries.Any() ? dbEntries.Min(x => x.StartTime) : endDate);

            return DataSyncMsg.Create(req, state.With(
                                          requestInfo: reqInfo,
                                          timeEntries: state.UpdateTimeEntries(dbEntries)));
        }

        static DataSyncMsg<AppState> ServerResponse(AppState state, DataMsg msg)
        {
            var serverMsg = msg as DataMsg.ServerResponse;
            return serverMsg.Data.Match(
                       receivedData => serverMsg.Request.MatchType(
                           (ServerRequest.CRUD _) =>
            {
                state = UpdateStateWithNewData(state, receivedData);
                return DataSyncMsg.Create(state);
            },
            (ServerRequest.DownloadEntries req) =>
            {
                state = UpdateStateWithNewData(state, receivedData);
                var reqInfo = state.RequestInfo.With(
                                  running: state.RequestInfo.Running.Where(x => x != req).ToList(),
                                  hasMore: receivedData.OfType<TimeEntryData> ().Any(),
                                  hadErrors: false);
                return DataSyncMsg.Create(state.With(requestInfo: reqInfo));
            },
            (ServerRequest.GetChanges req) =>
            {
                state = UpdateStateWithNewData(state, receivedData);

                // Update user
                var dataStore = ServiceContainer.Resolve<ISyncDataStore> ();
                UserData user = serverMsg.User;
                user.Id = state.User.Id;
                user.DefaultWorkspaceId = state.Workspaces.Values.Single(x => x.RemoteId == user.DefaultWorkspaceRemoteId).Id;
                var userUpdated = (UserData)dataStore.Update(ctx => ctx.Put(user)).Single();

                var reqInfo = state.RequestInfo.With(
                                  hadErrors: false,
                                  running: state.RequestInfo.Running.Where(x => x != req).ToList(),
                                  getChangesLastRun: serverMsg.Timestamp);

                return DataSyncMsg.Create(state.With(
                                              user: userUpdated,
                                              requestInfo: reqInfo,
                                              settings: state.Settings.With(getChangesLastRun: serverMsg.Timestamp)));
            },
            (ServerRequest.GetCurrentState _) =>
            {
                throw new NotImplementedException();
            },
            (ServerRequest.Authenticate _) =>
            {
                // TODO RX: Right now, Authenticate responses send UserDataPut messages
                throw new NotImplementedException();
            }),
            ex =>
            {
                var reqInfo = state.RequestInfo.With(
                                  running: state.RequestInfo.Running.Where(x => x != serverMsg.Request).ToList(),
                                  hadErrors: true);
                return DataSyncMsg.Create(state.With(requestInfo: reqInfo));
            });
        }

        static DataSyncMsg<AppState> TimeEntryPut(AppState state, DataMsg msg)
        {
            var entryData = (msg as DataMsg.TimeEntryPut).Data.ForceLeft();
            var dataStore = ServiceContainer.Resolve<ISyncDataStore>();
            var tagList = (msg as DataMsg.TimeEntryPut).TagNames;

            var updated = dataStore.Update(ctx =>
            {
                // Update time entry tags
                if (tagList.Any())
                {
                    var existingTags = state.Tags.Values.Where(x => x.WorkspaceId == entryData.WorkspaceId);
                    List<Guid> tagIds = new List<Guid> ();
                    foreach (var item in tagList)
                    {
                        if (!existingTags.Any(x => x.Name == item))
                        {
                            var newTag = TagData.Create(x =>
                            {
                                x.Name = item;
                                x.WorkspaceId = entryData.WorkspaceId;
                                x.WorkspaceRemoteId = entryData.WorkspaceRemoteId;
                            });
                            ctx.Put(newTag);
                            // Add the last added id
                            tagIds.Add(ctx.UpdatedItems.Last().Id);
                        }
                        else
                        {
                            tagIds.Add(existingTags.First(x => x.Name == item).Id);
                        }
                    }
                    entryData.With(x => x.TagIds = tagIds);
                }
                // TODO: Entry sanity check
                ctx.Put(entryData);
            });
            return DataSyncMsg.Create(updated, state.With(timeEntries: state.UpdateTimeEntries(updated),
                                      tags: state.Update(state.Tags, updated)));
        }

        static DataSyncMsg<AppState> TimeEntryRemove(AppState state, DataMsg msg)
        {
            // The TEs should have been already removed from AppState but try to remove them again just in case
            var entriesData = (msg as DataMsg.TimeEntriesRemove).Data.ForceLeft();
            var dataStore = ServiceContainer.Resolve<ISyncDataStore>();

            var updated = dataStore.Update(ctx =>
            {
                foreach (var entryData in entriesData)
                {
                    ctx.Delete(entryData.With(x => x.DeletedAt = Time.UtcNow));
                }
            });

            return DataSyncMsg.Create(updated, state.With(timeEntries: state.UpdateTimeEntries(updated)));
        }

        static DataSyncMsg<AppState> TagsPut(AppState state, DataMsg msg)
        {
            var tags = (msg as DataMsg.TagsPut).Data.ForceLeft();
            var dataStore = ServiceContainer.Resolve<ISyncDataStore>();

            var updated = dataStore.Update(ctx =>
            {
                foreach (var tag in tags)
                {
                    ctx.Put(tag);
                }
            });

            return DataSyncMsg.Create(updated, state.With(tags: state.Update(state.Tags, updated)));
        }

        static DataSyncMsg<AppState> ClientDataPut(AppState state, DataMsg msg)
        {
            var data = (msg as DataMsg.ClientDataPut).Data.ForceLeft();
            var dataStore = ServiceContainer.Resolve<ISyncDataStore>();

            var updated = dataStore.Update(ctx => ctx.Put(data));

            return DataSyncMsg.Create(updated, state.With(clients: state.Update(state.Clients, updated)));
        }

        static DataSyncMsg<AppState> ProjectDataPut(AppState state, DataMsg msg)
        {
            var data = (msg as DataMsg.ProjectDataPut).Data.ForceLeft();
            var dataStore = ServiceContainer.Resolve<ISyncDataStore>();

            var updated = dataStore.Update(ctx => ctx.Put(data));

            return DataSyncMsg.Create(updated, state.With(projects: state.Update(state.Projects, updated)));
        }

        static DataSyncMsg<AppState> UserDataPut(AppState state, DataMsg msg)
        {
            return (msg as DataMsg.UserDataPut).Data.Match(
                       userData =>
            {

                // Create user and workspace at the same time,
                // workspace created with default data and will be
                // updated in the next sync.
                var dataStore = ServiceContainer.Resolve<ISyncDataStore> ();
                var updated = dataStore.Update(ctx => { ctx.Put(userData); });

                // This will throw an exception if user hasn't been correctly updated
                var userDataInDb = updated.OfType<UserData> ().Single();

                return DataSyncMsg.Create(state.With(
                                              user: userDataInDb,
                                              requestInfo: state.RequestInfo.With(authResult: AuthResult.Success),
                                              workspaces: state.Update(state.Workspaces, updated),
                                              settings: state.Settings.With(userId: userDataInDb.Id)));
            },
            ex =>
            {
                return DataSyncMsg.Create(state.With(
                                              user: new UserData(),
                                              requestInfo: state.RequestInfo.With(authResult: ex.AuthResult)));
            });
        }

        static void CheckTimeEntryState(ITimeEntryData entryData, TimeEntryState expected, string action)
        {
            if (entryData.State != expected)
            {
                throw new InvalidOperationException(
                    string.Format("Cannot {0} a time entry ({1}) in {2} state.",
                                  action, entryData.Id, entryData.State));
            }
        }

        static DataSyncMsg<AppState> TimeEntryContinue(AppState state, DataMsg msg)
        {
            var entryData = (msg as DataMsg.TimeEntryContinue).Data.ForceLeft();
            var dataStore = ServiceContainer.Resolve <ISyncDataStore> ();
            var isStartedNew = (msg as DataMsg.TimeEntryContinue).StartedByFAB;

            var updated = dataStore.Update(ctx =>
            {
                // Stop ActiveEntry if necessary
                var prev = state.ActiveEntry.Data;
                if (prev.Id != Guid.Empty && prev.State == TimeEntryState.Running)
                {
                    ctx.Put(prev.With(x =>
                    {
                        x.State = TimeEntryState.Finished;
                        x.StopTime = Time.UtcNow;
                    }));
                }

                ITimeEntryData draft = null;
                if (entryData.Id == Guid.Empty)
                {
                    draft = state.GetTimeEntryDraft();
                }
                else
                {
                    CheckTimeEntryState(entryData, TimeEntryState.Finished, "continue");
                    draft = entryData;
                }

                ctx.Put(TimeEntryData.Create(draft: draft, transform: x =>
                {
                    x.RemoteId = null;
                    x.State = TimeEntryState.Running;
                    x.StartTime = Time.UtcNow.Truncate(TimeSpan.TicksPerSecond);
                    x.StopTime = null;
                }));
            });

            return DataSyncMsg.Create(updated, state.With(timeEntries: state.UpdateTimeEntries(updated)));
        }

        static DataSyncMsg<AppState> TimeEntryStop(AppState state, DataMsg msg)
        {
            var entryData = (msg as DataMsg.TimeEntryStop).Data.ForceLeft();
            var dataStore = ServiceContainer.Resolve <ISyncDataStore> ();

            CheckTimeEntryState(entryData, TimeEntryState.Running, "stop");

            var updated = dataStore.Update(ctx => ctx.Put(entryData.With(x =>
            {
                x.State = TimeEntryState.Finished;
                x.StopTime = Time.UtcNow;
            })));
            // TODO: Check updated.Count == 1?
            return DataSyncMsg.Create(updated, state.With(timeEntries: state.UpdateTimeEntries(updated)));
        }

        static DataSyncMsg<AppState> Reset(AppState state, DataMsg msg)
        {
            var dataStore = ServiceContainer.Resolve <ISyncDataStore> ();
            dataStore.WipeTables();

            // Clear platform settings.
            Settings.SerializedSettings = string.Empty;

            // Reset state
            var appState = AppState.Init();

            // TODO: Ping analytics?
            // TODO: Call Log service?

            return DataSyncMsg.Create(appState);
        }

        static DataSyncMsg<AppState> UpdateSettings(AppState state, DataMsg msg)
        {
            var info = (msg as DataMsg.UpdateSetting).Data.ForceLeft();
            SettingsState newSettings = state.Settings;

            switch (info.Item1)
            {
                case nameof(SettingsState.ShowWelcome):
                    newSettings = newSettings.With(showWelcome: (bool)info.Item2);
                    break;
                case nameof(SettingsState.ProjectSort):
                    newSettings = newSettings.With(projectSort: (string)info.Item2);
                    break;
                case nameof(SettingsState.ShowNotification):
                    newSettings = newSettings.With(showNotification: (bool)info.Item2);
                    break;
                case nameof(SettingsState.IdleNotification):
                    newSettings = newSettings.With(idleNotification: (bool)info.Item2);
                    break;
                case nameof(SettingsState.ChooseProjectForNew):
                    newSettings = newSettings.With(chooseProjectForNew: (bool)info.Item2);
                    break;
                case nameof(SettingsState.UseDefaultTag):
                    newSettings = newSettings.With(useTag: (bool)info.Item2);
                    break;
                case nameof(SettingsState.GroupedEntries):
                    newSettings = newSettings.With(groupedEntries: (bool)info.Item2);
                    break;

                    // TODO: log invalid/unknowns?
            }

            return DataSyncMsg.Create(state.With(settings: newSettings));
        }

        #region Util
        static AppState UpdateStateWithNewData(AppState state, IEnumerable<CommonData> receivedData)
        {
            var dataStore = ServiceContainer.Resolve <ISyncDataStore> ();
            dataStore.Update(ctx =>
            {
                foreach (var iterator in receivedData)
                {
                    // Check first if the newData has localId assigned
                    // (for example, the ones returned by TogglClient.Create)
                    // If no localId, check if an item with the same RemoteId is in the db
                    var newData = iterator;
                    var oldData = newData.Id != Guid.Empty
                                  ? ctx.GetByColumn(newData.GetType(), nameof(ICommonData.Id), newData.Id)
                                  : ctx.GetByColumn(newData.GetType(), nameof(ICommonData.RemoteId), newData.RemoteId);

                    if (oldData != null)
                    {
                        // TODO RX check this criteria to compare.
                        // and evaluate if local relations are needed.
                        if (newData.CompareTo(oldData) >= 0)
                        {
                            newData.Id = oldData.Id;
                            var data = BuildLocalRelationships(state, newData);  // Set local Id values.
                            PutOrDelete(ctx, data);
                        }
                        else
                        {
                            // No changes, just continue.
                            var logger = ServiceContainer.Resolve<ILogger> ();
                            logger.Info("UpdateStateWithNewData", "Posible sync error. Object without changes " +  newData);
                            continue;
                        }
                    }
                    else
                    {
                        newData.Id = Guid.NewGuid();  // Assign new Id
                        newData = BuildLocalRelationships(state, newData);  // Set local Id values.
                        PutOrDelete(ctx, newData);
                    }

                    // TODO RX Create a single update method
                    var updatedList = new List<ICommonData> {newData};
                    state = state.With(
                                workspaces: state.Update(state.Workspaces, updatedList),
                                projects: state.Update(state.Projects, updatedList),
                                workspaceUsers: state.Update(state.WorkspaceUsers, updatedList),
                                projectUsers: state.Update(state.ProjectUsers, updatedList),
                                clients: state.Update(state.Clients, updatedList),
                                tasks: state.Update(state.Tasks, updatedList),
                                tags: state.Update(state.Tags, updatedList),
                                timeEntries: state.UpdateTimeEntries(updatedList)
                            );
                }
            });
            return state;
        }

        static CommonData BuildLocalRelationships(AppState state, CommonData data)
        {
            // Build local relationships.
            // Object that comes from server needs to be
            // filled with local Ids.
            if (data is TimeEntryData)
            {
                var te = (TimeEntryData)data;
                te.UserId = state.User.Id;
                te.WorkspaceId = state.Workspaces.Values.Single(x => x.RemoteId == te.WorkspaceRemoteId).Id;
                if (te.ProjectRemoteId.HasValue &&
                        state.Projects.Any(x => x.Value.RemoteId == te.ProjectRemoteId.Value))
                {
                    te.ProjectId = state.Projects.Single(x => x.Value.RemoteId == te.ProjectRemoteId.Value).Value.Id;
                }

                if (te.TaskRemoteId.HasValue &&
                        state.Tasks.Any(x => x.Value.RemoteId == te.TaskRemoteId.Value))
                {
                    te.TaskId = state.Tasks.Single(x => x.Value.RemoteId == te.TaskRemoteId.Value).Value.Id;
                }
                return te;
            }

            if (data is ProjectData)
            {
                var pr = (ProjectData)data;
                pr.WorkspaceId = state.Workspaces.Values.Single(x => x.RemoteId == pr.WorkspaceRemoteId).Id;
                if (pr.ClientRemoteId.HasValue &&
                        state.Clients.Any(x => x.Value.RemoteId == pr.ClientRemoteId.Value))
                {
                    pr.ClientId = state.Clients.Single(x => x.Value.RemoteId == pr.ClientRemoteId.Value).Value.Id;
                }
                return pr;
            }

            if (data is ClientData)
            {
                var cl = (ClientData)data;
                cl.WorkspaceId = state.Workspaces.Values.Single(x => x.RemoteId == cl.WorkspaceRemoteId).Id;
                return cl;
            }

            if (data is TaskData)
            {
                var ts = (TaskData)data;
                ts.WorkspaceId = state.Workspaces.Values.Single(x => x.RemoteId == ts.WorkspaceRemoteId).Id;
                if (state.Projects.Any(x => x.Value.RemoteId == ts.ProjectRemoteId))
                {
                    ts.ProjectId = state.Projects.Single(x => x.Value.RemoteId == ts.ProjectRemoteId).Value.Id;
                }
                return ts;
            }

            if (data is TagData)
            {
                var t = (TagData)data;
                t.WorkspaceId = state.Workspaces.Values.Single(x => x.RemoteId == t.WorkspaceRemoteId).Id;
                return t;
            }

            if (data is UserData)
            {
                var u = (UserData)data;
                u.DefaultWorkspaceId = state.Workspaces.Values.Single(x => x.RemoteId == u.DefaultWorkspaceRemoteId).Id;
            }

            return data;
        }

        static void PutOrDelete(ISyncDataStoreContext ctx, ICommonData data)
        {
            if (data.DeletedAt == null)
            {
                ctx.Put(data);
            }
            else
            {
                ctx.Delete(data);
            }
        }

        // TODO: replace this method with the SQLite equivalent.
        static DateTime GetDatesByDays(ISyncDataStore dataStore, DateTime startDate, int numDays)
        {
            var baseQuery = dataStore.Table<TimeEntryData> ().Where(
                                r => r.State != TimeEntryState.New &&
                                r.StartTime < startDate &&
                                r.DeletedAt == null);

            var entries = baseQuery.ToList();
            if (entries.Count > 0)
            {
                var group = entries
                            .OrderByDescending(r => r.StartTime)
                            .GroupBy(t => t.StartTime.Date)
                            .Take(numDays)
                            .LastOrDefault();
                return group.Key;
            }
            return DateTime.MinValue;
        }
        #endregion
    }
}

