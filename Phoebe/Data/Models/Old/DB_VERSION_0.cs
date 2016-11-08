using System;
using System.Collections.Generic;
using System.Linq;
using SQLite.Net.Attributes;
using Toggl.Phoebe.Misc;
using XPlatUtils;

namespace Toggl.Phoebe.Data.Models.Old.DB_VERSION_0
{
    class Migrator : DatabaseMigrator
    {
        public Migrator()
        : base(0, 1)
        {
        }

        protected override IEnumerable<Action<UpgradeContext>> upgraders =>
        new Action<UpgradeContext>[]
        {
            c => c.Upgrade<ClientData, Models.ClientData>(),
            c => c.Upgrade<ProjectData, Models.ProjectData>(),
            c => c.Upgrade<ProjectUserData, Models.ProjectUserData>(),
            c => c.Upgrade<TagData, Models.TagData>(),
            c => c.Upgrade<TaskData, Models.TaskData>(),
            c => c.Upgrade<TimeEntryData, Models.TimeEntryData>(),
            c => c.Upgrade<UserData, Models.UserData>(),
            c => c.Upgrade<WorkspaceData, Models.WorkspaceData>(),
            c => c.Upgrade<WorkspaceUserData, Models.WorkspaceUserData>(),
        };
    }

    public abstract class CommonData : IIdentificable
    {
        [PrimaryKey, AutoIncrement]
        public Guid Id { get; set; }
        public DateTime ModifiedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool IsDirty { get; set; }
        [Unique]
        public long? RemoteId { get; set; }
        public bool RemoteRejected { get; set; }

        protected void Upgrade(Models.CommonData data)
        {
            data.Id = Id;
            data.RemoteId = RemoteId;
            data.ModifiedAt = ModifiedAt;
            data.DeletedAt = DeletedAt;
            data.SyncState = IsDirty
                             ? (RemoteId.HasValue ? SyncState.UpdatePending : SyncState.CreatePending)
                             : SyncState.Synced;
        }
    }

    [Table("ClientModel")]
    public class ClientData : CommonData, IUpgradesTo<Models.ClientData>
    {
        public string Name { get; set; }
        public Guid WorkspaceId { get; set; }

        public Models.ClientData Upgrade(ISyncDataStoreContext ctx)
        {
            var data = new Models.ClientData();
            Upgrade(data);
            data.Name = Name;

            data.WorkspaceId = WorkspaceId;
            var ws = ctx.Connection.Table<WorkspaceData> ().Where(x => x.Id == WorkspaceId).FirstOrDefault();
            data.WorkspaceRemoteId = ws?.RemoteId ?? 0;

            return data;
        }
    }

    [Table("ProjectModel")]
    public class ProjectData : CommonData, IUpgradesTo<Models.ProjectData>
    {
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsBillable { get; set; }
        public bool IsPrivate { get; set; }
        public bool IsTemplate { get; set; }
        public bool UseTasksEstimate { get; set; }
        public Guid WorkspaceId { get; set; }
        public Guid? ClientId { get; set; }

        public Models.ProjectData Upgrade(ISyncDataStoreContext ctx)
        {
            var data = new Models.ProjectData();
            Upgrade(data);
            data.Name = Name;
            data.IsActive = IsActive;
            data.IsBillable = IsBillable;
            data.IsPrivate = IsPrivate;
            data.IsTemplate = IsTemplate;
            data.UseTasksEstimate = UseTasksEstimate;

            data.WorkspaceId = WorkspaceId;
            var ws = ctx.Connection.Table<WorkspaceData> ().Where(x => x.Id == WorkspaceId).FirstOrDefault();
            data.WorkspaceRemoteId = ws?.RemoteId ?? 0;

            data.ClientId = ClientId ?? Guid.Empty;
            if (ClientId.HasValue)
            {
                var cl = ctx.Connection.Table<ClientData> ().Where(x => x.Id == ClientId).FirstOrDefault();
                data.ClientRemoteId = cl?.RemoteId;
            }

            return data;
        }
    }

    [Table("ProjectUserModel")]
    public class ProjectUserData : CommonData, IUpgradesTo<Models.ProjectUserData>
    {
        public bool IsManager { get; set; }
        public int HourlyRate { get; set; }
        public Guid ProjectId { get; set; }
        public Guid UserId { get; set; }

        public Models.ProjectUserData Upgrade(ISyncDataStoreContext ctx)
        {
            var data = new Models.ProjectUserData();
            Upgrade(data);
            data.IsManager = IsManager;
            data.HourlyRate = HourlyRate;

            data.ProjectId = ProjectId;
            var pr = ctx.Connection.Table<ProjectData> ().Where(x => x.Id == ProjectId).FirstOrDefault();
            data.ProjectRemoteId = pr?.RemoteId ?? 0;

            data.UserId = UserId;
            var usr = ctx.Connection.Table<UserData> ().Where(x => x.Id == UserId).FirstOrDefault();
            data.UserRemoteId = usr?.RemoteId ?? 0;

            return data;
        }
    }

    [Table("TagModel")]
    public class TagData : CommonData, IUpgradesTo<Models.TagData>
    {
        public string Name { get; set; }
        public Guid WorkspaceId { get; set; }

        public Models.TagData Upgrade(ISyncDataStoreContext ctx)
        {
            var data = new Models.TagData();
            Upgrade(data);
            data.Name = Name;

            data.WorkspaceId = WorkspaceId;
            var ws = ctx.Connection.Table<WorkspaceData> ().Where(x => x.Id == WorkspaceId).FirstOrDefault();
            data.WorkspaceRemoteId = ws?.RemoteId ?? 0;

            return data;
        }
    }

    [Table("TaskModel")]
    public class TaskData : CommonData, IUpgradesTo<Models.TaskData>
    {
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public long Estimate { get; set; }
        public Guid WorkspaceId { get; set; }
        public Guid ProjectId { get; set; }

        public Models.TaskData Upgrade(ISyncDataStoreContext ctx)
        {
            var data = new Models.TaskData();
            Upgrade(data);
            data.Name = Name;
            data.IsActive = IsActive;
            data.Estimate = Estimate;

            data.WorkspaceId = WorkspaceId;
            var ws = ctx.Connection.Table<WorkspaceData> ().Where(x => x.Id == WorkspaceId).FirstOrDefault();
            data.WorkspaceRemoteId = ws?.RemoteId ?? 0;

            data.ProjectId = ProjectId;
            var pr = ctx.Connection.Table<ProjectData> ().Where(x => x.Id == ProjectId).FirstOrDefault();
            data.ProjectRemoteId = pr?.RemoteId ?? 0;

            return data;
        }
    }

    [Table("TimeEntryModel")]
    public class TimeEntryData : CommonData, IUpgradesTo<Models.TimeEntryData>
    {
        public TimeEntryState State { get; set; }
        public string Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? StopTime { get; set; }
        public bool DurationOnly { get; set; }
        public bool IsBillable { get; set; }
        public Guid UserId { get; set; }
        public Guid WorkspaceId { get; set; }
        public Guid? ProjectId { get; set; }
        public Guid? TaskId { get; set; }

        public Models.TimeEntryData Upgrade(ISyncDataStoreContext ctx)
        {
            var data = new Models.TimeEntryData();
            Upgrade(data);
            data.State = State;
            data.Description = Description;
            data.StartTime = StartTime;
            data.StopTime = StopTime;
            data.DurationOnly = DurationOnly;
            data.IsBillable = IsBillable;

            data.Tags = ctx.Connection.Query<TagData>(
                            string.Format(
                                "SELECT * FROM {0} INNER JOIN (SELECT {1}.TagId AS tagId FROM {1} WHERE {1}.TimeEntryId = '{2}') ON {0}.Id = tagId",
                                "TagModel",
                                "TimeEntryTagModel",
                                this.Id
                            )
                        ).Select(t => t.Name).ToList();

            data.UserId = UserId;
            var usr = ctx.Connection.Table<UserData> ().Where(x => x.Id == UserId).FirstOrDefault();
            data.UserRemoteId = usr?.RemoteId ?? 0;

            data.WorkspaceId = WorkspaceId;
            var ws = ctx.Connection.Table<WorkspaceData> ().Where(x => x.Id == WorkspaceId).FirstOrDefault();
            data.WorkspaceRemoteId = ws?.RemoteId ?? 0;

            data.ProjectId = ProjectId ?? Guid.Empty;
            if (ProjectId.HasValue)
            {
                var pr = ctx.Connection.Table<ProjectData> ().Where(x => x.Id == ProjectId).FirstOrDefault();
                data.ProjectRemoteId = pr?.RemoteId;
            }

            data.TaskId = TaskId ?? Guid.Empty;
            if (TaskId.HasValue)
            {
                var task = ctx.Connection.Table<TaskData> ().Where(x => x.Id == TaskId).FirstOrDefault();
                data.TaskRemoteId = task?.RemoteId;
            }

            return data;
        }
    }

    [Table("TimeEntryTagModel")]
    public class TimeEntryTagData : CommonData
    {
        public Guid TimeEntryId { get; set; }
        public Guid TagId { get; set; }
    }

    [Table("UserModel")]
    public class UserData : CommonData, IUpgradesTo<Models.UserData>
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public DayOfWeek StartOfWeek { get; set; }
        public string DateFormat { get; set; }
        public string TimeFormat { get; set; }
        public string ImageUrl { get; set; }
        public string Locale { get; set; }
        public string Timezone { get; set; }
        public bool SendProductEmails { get; set; }
        public bool SendTimerNotifications { get; set; }
        public bool SendWeeklyReport { get; set; }
        public TrackingMode TrackingMode { get; set; }
        public DurationFormat DurationFormat { get; set; }
        public bool ExperimentIncluded { get; set; }
        public int ExperimentNumber { get; set; }
        public Guid DefaultWorkspaceId { get; set; }

        public Models.UserData Upgrade(ISyncDataStoreContext ctx)
        {
            var data = new Models.UserData();
            Upgrade(data);
            data.Name = Name;
            data.Email = Email;
            data.StartOfWeek = StartOfWeek;
            data.DateFormat = DateFormat;
            data.TimeFormat = TimeFormat;
            data.ImageUrl = ImageUrl;
            data.Locale = Locale;
            data.Timezone = Timezone;
            data.SendProductEmails = SendProductEmails;
            data.SendTimerNotifications = SendTimerNotifications;
            data.SendWeeklyReport = SendWeeklyReport;
            data.TrackingMode = TrackingMode;
            data.DurationFormat = DurationFormat;
            data.ExperimentIncluded = ExperimentIncluded;
            data.ExperimentNumber = ExperimentNumber;
            data.DefaultWorkspaceId = DefaultWorkspaceId;
            var ws = ctx.Connection.Table<WorkspaceData> ().Where(x => x.Id == DefaultWorkspaceId).FirstOrDefault();
            data.DefaultWorkspaceRemoteId = ws?.RemoteId ?? 0;

            return data;
        }
    }

    [Table("WorkspaceModel")]
    public class WorkspaceData : CommonData, IUpgradesTo<Models.WorkspaceData>
    {
        public string Name { get; set; }
        public bool IsPremium { get; set; }
        public decimal? DefaultRate { get; set; }
        public string DefaultCurrency { get; set; }
        public AccessLevel ProjectCreationPrivileges { get; set; }
        public AccessLevel BillableRatesVisibility { get; set; }
        public RoundingMode RoundingMode { get; set; }
        public int RoundingPercision { get; set; }
        public string LogoUrl { get; set; }

        public Models.WorkspaceData Upgrade(ISyncDataStoreContext ctx)
        {
            var data = new Models.WorkspaceData();
            Upgrade(data);
            data.Name = Name;
            data.IsPremium = IsPremium;
            data.DefaultRate = DefaultRate;
            data.DefaultCurrency = DefaultCurrency;
            data.ProjectCreationPrivileges = ProjectCreationPrivileges;
            data.BillableRatesVisibility = BillableRatesVisibility;
            data.RoundingMode = RoundingMode;
            data.RoundingPrecision = RoundingPercision;
            data.LogoUrl = LogoUrl;

            // This fields are not present in the current DB
            // They are ignored for the moment.

            //data.OnlyAdminsMayCreateProjects
            //data.OnlyAdminsSeeBillableRates
            //data.IsAdmin

            return data;
        }
    }

    [Table("WorkspaceUserModel")]
    public class WorkspaceUserData : CommonData, IUpgradesTo<Models.WorkspaceUserData>
    {
        public bool IsAdmin { get; set; }
        public bool IsActive { get; set; }
        public Guid WorkspaceId { get; set; }
        public Guid UserId { get; set; }

        public Models.WorkspaceUserData Upgrade(ISyncDataStoreContext ctx)
        {
            var data = new Models.WorkspaceUserData();
            Upgrade(data);
            data.IsAdmin = IsAdmin;
            data.IsActive = IsActive;

            data.UserId = UserId;
            var usr = ctx.Connection.Table<UserData> ().Where(x => x.Id == UserId).FirstOrDefault();
            data.UserRemoteId = usr?.RemoteId ?? 0;

            data.WorkspaceId = WorkspaceId;
            var ws = ctx.Connection.Table<WorkspaceData> ().Where(x => x.Id == WorkspaceId).FirstOrDefault();
            data.WorkspaceRemoteId = ws?.RemoteId ?? 0;

            return data;
        }
    }
}
