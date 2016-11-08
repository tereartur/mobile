using System.Collections.Generic;

namespace Toggl.Phoebe.Data.Models
{
    public class TimeEntryInfo
    {
        public IWorkspaceData WorkspaceData { get; private set; }
        public IProjectData ProjectData { get; private set; }
        public IClientData ClientData { get; private set; }
        public ITaskData TaskData { get; private set; }
        public IReadOnlyList<ITagData> Tags { get; private set; }
        public string HexColor { get; private set; }

        public TimeEntryInfo(
            IWorkspaceData wsData,
            IProjectData projectData,
            IClientData clientData,
            ITaskData taskData,
            IReadOnlyList<ITagData> tags,
            string hexColor)
        {
            WorkspaceData = wsData;
            ProjectData = projectData;
            ClientData = clientData;
            TaskData = taskData;
            Tags = tags;
            HexColor = hexColor;
        }

        public TimeEntryInfo With(
            IWorkspaceData wsData = null,
            IProjectData projectData = null,
            IClientData clientData = null,
            ITaskData taskData = null,
            IReadOnlyList<ITagData> tags = null,
            string hexColor = null)
        {
            return new TimeEntryInfo(
                       wsData ?? this.WorkspaceData,
                       projectData ?? this.ProjectData,
                       clientData ?? this.ClientData,
                       taskData ?? this.TaskData,
                       tags ?? this.Tags,
                       hexColor ?? this.HexColor);
        }
    }
}

