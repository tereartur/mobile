using System;
using SQLite.Net.Attributes;

namespace Toggl.Phoebe.Data.Models
{
    public interface IProjectData : ICommonData
    {
        string Name { get; }
        int Color { get; }
        bool IsActive { get; }
        bool IsBillable { get; }
        bool IsPrivate { get; }
        bool IsTemplate { get; }
        bool UseTasksEstimate { get; }
        long WorkspaceRemoteId { get; }
        long? ClientRemoteId { get; }
        string HexColor { get; }
        Guid WorkspaceId { get; }
        Guid ClientId { get; }
        IProjectData With(Action<ProjectData> transform);
    }

    [Table("ProjectModel")]
    public class ProjectData : CommonData, IProjectData
    {
        public static readonly string[] HexColors =
        {
            "#4dc3ff", "#bc85e6", "#df7baa", "#f68d38", "#b27636",
            "#8ab734", "#14a88e", "#268bb5", "#6668b4", "#a4506c",
            "#67412c", "#3c6526", "#094558", "#bc2d07", "#999999"
        };

        public static readonly string DefaultColor = "#999999";

        public static readonly int GroupedProjectColorIndex = -1;

        public static readonly string GroupedProjectColor = "#dddddd";

        public static IProjectData Create(Action<ProjectData> transform = null)
        {
            return CommonData.Create(transform);
        }

        /// <summary>
        /// ATTENTION: This constructor should only be used by SQL and JSON serializers
        /// To create new objects, use the static Create method instead
        /// </summary>
        public ProjectData()
        {
        }

        protected ProjectData(ProjectData other) : base(other)
        {
            Name = other.Name;
            Color = other.Color;
            HexColor = other.HexColor;
            IsActive = other.IsActive;
            IsBillable = other.IsBillable;
            IsPrivate = other.IsPrivate;
            IsTemplate = other.IsTemplate;
            UseTasksEstimate = other.UseTasksEstimate;
            WorkspaceId = other.WorkspaceId;
            ClientId = other.ClientId;
            ClientRemoteId = other.ClientRemoteId;
            WorkspaceRemoteId = other.WorkspaceRemoteId;
        }

        public override object Clone()
        {
            return new ProjectData(this);
        }

        public IProjectData With(Action<ProjectData> transform)
        {
            return base.With(transform);
        }

        public string Name { get; set; }

        public int Color { get; set; }

        public bool IsActive { get; set; }

        public bool IsBillable { get; set; }

        public bool IsPrivate { get; set; }

        public bool IsTemplate { get; set; }

        public bool UseTasksEstimate { get; set; }

        public long WorkspaceRemoteId { get; set; }

        public long? ClientRemoteId { get; set; }

        public Guid WorkspaceId { get; set; }

        public Guid ClientId { get; set; }

        public string HexColor { get; set; }
    }
}
