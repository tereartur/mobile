using System;
using System.Collections.Generic;
using AutoMapper;
using Toggl.Phoebe.Data.Models;

namespace Toggl.Phoebe.Data.Json
{
    public class JsonMapper
    {
        readonly IMapper mapper;

        public JsonMapper()
        {
            var mapConfig = new MapperConfiguration(config =>
            {
                // TODO: Review how map reverse works with include.
                config.CreateMap<CommonJson, CommonData> ()
                .ForMember(dest => dest.SyncState, opt => opt.UseValue(SyncState.Synced))
                .ForMember(dest => dest.ModifiedAt, opt => opt.ResolveUsing<ModifiedAtResolver> ())
                .ForMember(dest => dest.DeletedAt, opt => opt.ResolveUsing<DeletedAtResolver>())
                .Include<ProjectJson, ProjectData> ()
                .Include<ClientJson, ClientData> ()
                .Include<TagJson, TagData>()
                .Include<TaskJson, TaskData>()
                .Include<WorkspaceJson, WorkspaceData>()
                .Include<WorkspaceUserJson, WorkspaceUserData>()
                .Include<UserJson, UserData>()
                .Include<TimeEntryJson, TimeEntryData>().ReverseMap();

                config.CreateMap<CommonData, CommonJson>()
                .ForMember(dest => dest.DeletedAt, opt => opt.ResolveUsing<InverseDeletedAtResolver>())
                .ForMember(dest => dest.ModifiedAt, opt => opt.MapFrom(src => src.ModifiedAt.ToUtc()));

                config.CreateMap<ProjectJson, ProjectData> ().ReverseMap();
                config.CreateMap<ProjectUserJson, ProjectUserData>().ReverseMap();
                config.CreateMap<ClientJson, ClientData>();
                config.CreateMap<ClientData, ClientJson>();
                config.CreateMap<TagJson, TagData>().ReverseMap();
                config.CreateMap<TaskJson, TaskData>().ReverseMap();
                config.CreateMap<WorkspaceUserJson, WorkspaceUserData>().ReverseMap();
                config.CreateMap<WorkspaceJson, WorkspaceData>().ReverseMap();

                // User mapping
                config.CreateMap<UserJson, UserData>()
                .ForMember(dest => dest.TrackingMode, opt => opt.MapFrom(src => src.StoreStartAndStopTime ? TrackingMode.StartNew : TrackingMode.Continue));

                config.CreateMap<UserData, UserJson>()
                .ForMember(dest => dest.CreatedWith, opt => opt.UseValue(Platform.DefaultCreatedWith));

                // TimeEntry mapping
                config.CreateMap<TimeEntryJson, TimeEntryData>()
                .ForMember(dest => dest.StartTime, opt => opt.ResolveUsing<StartTimeResolver>())
                .ForMember(dest => dest.StopTime, opt => opt.ResolveUsing<StopTimeResolver>())
                .ForMember(dest => dest.State, opt => opt.ResolveUsing<StateResolver>());

                config.CreateMap<TimeEntryData, TimeEntryJson>()
                .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartTime.ToUtc()))
                .ForMember(dest => dest.StopTime, opt => opt.MapFrom(src => src.StopTime.ToUtc()))
                .ForMember(dest => dest.CreatedWith, opt => opt.UseValue(Platform.DefaultCreatedWith))
                .ForMember(dest => dest.Duration, opt => opt.ResolveUsing<DurationResolver>());

                // Extra mappings
                config.CreateMap<ReportJson, ReportData>()
                .ForMember(dest => dest.Activity, opt => opt.ResolveUsing<ReportActivityResolver>())
                .ForMember(dest => dest.Projects, opt => opt.ResolveUsing<ReportProjectsResolver>())
                .ForMember(dest => dest.TotalCost, opt => opt.ResolveUsing<ReportTotalCostResolver>());
            });

            mapper = mapConfig.CreateMapper();
        }

        public T Map<T> (object source)
        {
            return mapper.Map<T> (source);
        }

        public CommonData Map(CommonJson source)
        {
            Type sourceType = source.GetType();

            if (sourceType == typeof(ClientJson))
            {
                return mapper.Map<ClientData> (source as ClientJson);
            }
            else if (sourceType == typeof(ProjectJson))
            {
                return mapper.Map<ProjectData> (source as ProjectJson);
            }
            else if (sourceType == typeof(TaskJson))
            {
                return mapper.Map<TaskData> (source as TaskJson);
            }
            else if (sourceType == typeof(TimeEntryJson))
            {
                return mapper.Map<TimeEntryData> (source as TimeEntryJson);
            }
            else if (sourceType == typeof(WorkspaceJson))
            {
                return mapper.Map<WorkspaceData> (source as WorkspaceJson);
            }
            else if (sourceType == typeof(UserJson))
            {
                return mapper.Map<UserData> (source as UserJson);
            }
            else if (sourceType == typeof(TagJson))
            {
                return mapper.Map<TagData> (source as TagJson);
            }
            else if (sourceType == typeof(WorkspaceUserJson))
            {
                return mapper.Map<WorkspaceUserData> (source as WorkspaceUserJson);
            }
            else if (sourceType == typeof(ProjectUserJson))
            {
                return mapper.Map<ProjectUserData> (source as ProjectUserJson);
            }
            else
            {
                throw new NotSupportedException(string.Format("Cannot map {0} from JSON", sourceType.FullName));
            }
        }

        public CommonJson MapToJson(ICommonData source)
        {
            Type destinationType = null;
            Type sourceType = source.GetType();

            if (sourceType == typeof(ClientData))
            {
                destinationType = typeof(ClientJson);
            }
            else if (sourceType == typeof(ProjectData))
            {
                destinationType = typeof(ProjectJson);
            }
            else if (sourceType == typeof(TaskData))
            {
                destinationType = typeof(TaskJson);
            }
            else if (sourceType == typeof(TimeEntryData))
            {
                destinationType = typeof(TimeEntryJson);
            }
            else if (sourceType == typeof(WorkspaceData))
            {
                destinationType = typeof(WorkspaceJson);
            }
            else if (sourceType == typeof(UserData))
            {
                destinationType = typeof(UserJson);
            }
            else if (sourceType == typeof(TagData))
            {
                destinationType = typeof(TagJson);
            }
            else if (sourceType == typeof(WorkspaceUserData))
            {
                destinationType = typeof(WorkspaceUserJson);
            }
            else if (sourceType == typeof(ProjectUserData))
            {
                destinationType = typeof(ProjectUserJson);
            }
            else
            {
                throw new NotSupportedException(string.Format("Cannot map {0} to JSON", sourceType.FullName));
            }

            return (CommonJson)mapper.Map(source, sourceType, destinationType);
        }

        #region TimeEntry resolvers
        public class ModifiedAtResolver : ValueResolver<CommonJson, DateTime>
        {
            protected override DateTime ResolveCore(CommonJson source)
            {
                if (source.ModifiedAt == DateTime.MinValue)
                {
                    return DateTime.UtcNow;
                }
                return source.ModifiedAt.ToUtc();
            }
        }

        public class DeletedAtResolver : ValueResolver < CommonJson, DateTime?>
        {
            protected override DateTime? ResolveCore(CommonJson source)
            {
                if (source.DeletedAt.HasValue)
                {
                    return source.DeletedAt.Value.ToUtc();
                }
                return null;
            }
        }

        public class InverseDeletedAtResolver : ValueResolver < CommonData, DateTime?>
        {
            protected override DateTime? ResolveCore(CommonData source)
            {
                if (source.DeletedAt.HasValue)
                {
                    return source.DeletedAt.Value.ToUtc();
                }
                return null;
            }
        }

        public class StartTimeResolver : ValueResolver<TimeEntryJson, DateTime>
        {
            protected override DateTime ResolveCore(TimeEntryJson source)
            {
                DateTime startTime;
                if (source.Duration >= 0)
                {
                    startTime = source.StartTime.ToUtc();
                }
                else
                {
                    var now = Time.UtcNow.Truncate(TimeSpan.TicksPerSecond);
                    var duration = now.ToUnix() + TimeSpan.FromSeconds(source.Duration);
                    startTime = now - duration;
                }
                return startTime;
            }
        }

        public class StopTimeResolver : ValueResolver < TimeEntryJson, DateTime?>
        {
            protected override DateTime? ResolveCore(TimeEntryJson source)
            {
                DateTime? stopTime = null;
                TimeSpan duration;
                if (source.Duration >= 0)
                {
                    duration = TimeSpan.FromSeconds(source.Duration);
                    stopTime = source.StartTime.ToUtc() + duration;
                }

                return stopTime;
            }
        }

        public class StateResolver : ValueResolver<TimeEntryJson, TimeEntryState>
        {
            protected override TimeEntryState ResolveCore(TimeEntryJson source)
            {
                return source.Duration >= 0 ? TimeEntryState.Finished : TimeEntryState.Running;
            }
        }
        #endregion

        #region Report resolvers
        public class DurationResolver : ValueResolver<TimeEntryData, long>
        {
            protected override long ResolveCore(TimeEntryData source)
            {
                var now = Time.UtcNow;

                // Calculate time entry duration
                TimeSpan duration;
                if (source.StartTime.IsMinValue())
                {
                    duration = TimeSpan.Zero;
                }
                else
                {
                    duration = (source.StopTime ?? now) - source.StartTime;
                    if (duration < TimeSpan.Zero)
                    {
                        duration = TimeSpan.Zero;
                    }
                }

                // Encode the duration
                var encoded = (long)duration.TotalSeconds;
                if (source.State == TimeEntryState.Running)
                {
                    encoded = (long)(encoded - now.ToUnix().TotalSeconds);
                }
                return encoded;
            }
        }

        public class ReportActivityResolver : ValueResolver<ReportJson, List<ReportActivity>>
        {
            readonly DateTime UnixStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

            protected override List<ReportActivity> ResolveCore(ReportJson source)
            {
                var jsonList = source.ActivityContainer;
                var activityList = new List<ReportActivity>();
                foreach (var item in jsonList.Rows)
                {
                    activityList.Add(new ReportActivity()
                    {
                        StartTime = UnixStart.AddTicks(ToLong(item[0]) * TimeSpan.TicksPerMillisecond),
                        TotalTime = ToLong(item[1]),
                        BillableTime = ToLong(item[2])
                    });
                }
                return activityList;
            }

            long ToLong(string s)
            {
                long l;

                // round decimal values
                if (!string.IsNullOrEmpty(s) && s.Contains("."))
                {
                    double d;
                    double.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out d);
                    l = (long)Math.Round(d);
                }
                else
                {
                    long.TryParse(s, out l);
                }
                return l;
            }
        }

        public class ReportProjectsResolver : ValueResolver<ReportJson, List<ReportProject>>
        {
            protected override List<ReportProject> ResolveCore(ReportJson source)
            {
                var jsonList = source.Projects;
                var projectList = new List<ReportProject>();

                foreach (var item in jsonList)
                {
                    int colorIndex = ProjectData.HexColors.Length - 1;
                    if (item.Description?.Project != null)
                    {
                        if (!int.TryParse(item.Description.Color, out colorIndex))
                        {
                            colorIndex = ProjectData.HexColors.Length - 1;
                        }
                    }

                    var p = new ReportProject()
                    {
                        Project = item.Description.Project,
                        TotalTime = item.TotalTime,
                        Color = item.Description.HexColor ?? ProjectData.HexColors[colorIndex]
                    };
                    p.Items = new List<ReportTimeEntry>();
                    if (item.Items != null)
                    {
                        foreach (var i in item.Items)
                        {
                            p.Items.Add(new ReportTimeEntry()
                            {
                                Rate = i.Rate,
                                Title = i.Description.Title,
                                Time = i.Time,
                                Sum = i.Sum
                            });
                        }
                    }
                    p.Currencies = new List<ReportCurrency>();
                    if (item.Currencies != null)
                    {
                        foreach (var i in item.Currencies)
                        {
                            p.Currencies.Add(new ReportCurrency()
                            {
                                Amount = i.Amount,
                                Currency = i.Currency
                            });
                        }
                    }
                    projectList.Add(p);
                }
                return projectList;
            }
        }

        public class ReportTotalCostResolver : ValueResolver<ReportJson, List<string>>
        {
            protected override List<string> ResolveCore(ReportJson source)
            {
                var totalCost = new List<string>();
                if (source.TotalCurrencies != null)
                {
                    source.TotalCurrencies.Sort((x, y) => y.Amount.CompareTo(x.Amount));
                    foreach (var row in source.TotalCurrencies)
                    {
                        totalCost.Add($"{row.Amount} {row.Currency}");
                    }
                }
                return totalCost;
            }
        }
        #endregion
    }
}
