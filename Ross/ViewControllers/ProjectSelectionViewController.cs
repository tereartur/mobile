using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Cirrious.FluentLayouts.Touch;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using GalaSoft.MvvmLight.Helpers;
using Toggl.Phoebe.Data.Models;
using Toggl.Phoebe.Helpers;
using Toggl.Phoebe.Reactive;
using Toggl.Phoebe.ViewModels;
using Toggl.Ross.DataSources;
using Toggl.Ross.Theme;
using Toggl.Ross.Views;
using UIKit;

namespace Toggl.Ross.ViewControllers
{
    public class ProjectSelectionViewController : UITableViewController
    {
        private const string TopProjectsKey = "ProjectTopProjects";

        private readonly static nfloat CellHeight = 64f;
        private readonly static nfloat HeaderCellHeight = 56f;

        private readonly static NSString ClientHeaderId = new NSString("ClientHeaderId");
        private readonly static NSString ProjectCellId = new NSString("ProjectCellId");
        private readonly static NSString TaskCellId = new NSString("TaskCellId");

        private const float CellSpacing = 4f;
        private Guid workspaceId;
        private ProjectListVM viewModel;
        private readonly IOnProjectSelectedHandler handler;

        public ProjectSelectionViewController(EditTimeEntryViewController editView) : base(UITableViewStyle.Plain)
        {
            Title = "ProjectTitle".Tr();
            this.workspaceId = editView.WorkspaceId;
            this.handler = editView;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            View.Apply(Style.Screen);
            EdgesForExtendedLayout = UIRectEdge.None;

            TableView.RowHeight = 64f;
            TableView.RegisterClassForHeaderFooterViewReuse(typeof(SectionHeaderView), ClientHeaderId);
            TableView.RegisterClassForCellReuse(typeof(ProjectCell), ProjectCellId);
            TableView.RegisterClassForCellReuse(typeof(TaskCell), TaskCellId);
            TableView.SeparatorStyle = UITableViewCellSeparatorStyle.SingleLine;

            var defaultFooterView = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.Gray);
            defaultFooterView.Frame = new CGRect(0, 0, 50, 50);
            defaultFooterView.StartAnimating();
            TableView.TableFooterView = defaultFooterView;

            viewModel = new ProjectListVM(StoreManager.Singleton.AppState, workspaceId);
            TableView.Source = new Source(this, viewModel);

            var addBtn = new UIBarButtonItem(UIBarButtonSystemItem.Add, OnAddNewProject);
            if (viewModel.WorkspaceList.Count > 1)
            {
                var filterBtn = new UIBarButtonItem(UIImage.FromFile("filter_icon.png"), UIBarButtonItemStyle.Plain, OnShowWorkspaceFilter);
                NavigationItem.RightBarButtonItems = new[] { filterBtn, addBtn };
            }
            else
            {
                NavigationItem.RightBarButtonItem = addBtn;
            }

            TableView.TableFooterView = null;

            UpdateTopProjectsHeader();
        }

        internal void UpdateTopProjectsHeader()
        {
            //Enumerates only once
            var topProjects = viewModel.TopProjects?.ToList();

            var numberOfProjects = topProjects?.Count ?? 0;
            if (numberOfProjects == 0)
            {
                TableView.TableHeaderView = null;
                return;
            }

            var constraints = new List<FluentLayout>();
            var headerHeight = HeaderCellHeight + ((CellHeight + 1) * numberOfProjects);

            var tableHeader = new UIView(new CGRect(0, 0, View.Frame.Width, headerHeight));

            var headerLabel = new UILabel() { Text = TopProjectsKey.Tr() };
            headerLabel.Apply(Style.Log.HeaderDateLabel);
            tableHeader.Add(headerLabel);

            constraints.Add(headerLabel.WithSameTop(tableHeader));
            constraints.Add(headerLabel.AtLeftOf(tableHeader, 16));
            constraints.Add(headerLabel.Height().EqualTo(HeaderCellHeight));
            constraints.Add(headerLabel.Width().EqualTo(View.Frame.Width));

            UIView previousView = headerLabel;
           
            foreach (var project in topProjects)
            {
                var buttonConstraints = new List<FluentLayout>();

                var hasTask = project.Task != null;
                var color = Color.FromHex(project.GetProperColor());
                var hasClient = !string.IsNullOrEmpty(project.ClientName);

                //Button
                var button = new UIButton();
                button.BackgroundColor = Color.White;
                button.TouchUpInside += (s, e) => OnItemSelected(project);
                tableHeader.Add(button);

                constraints.Add(button.Below(previousView, 1));
                constraints.Add(button.Height().EqualTo(CellHeight));
                constraints.Add(button.Width().EqualTo(View.Frame.Width));

                //Circle
                var circle = new CircleView();
                circle.Color = color.CGColor;
                button.Add(circle);

                buttonConstraints.Add(circle.AtLeftOf(button, 16));
                buttonConstraints.Add(circle.Width().EqualTo(12));
                buttonConstraints.Add(circle.Height().EqualTo(12));

                //Project & client label
                var projectLabel = new UILabel();
                var projectLabelText = hasClient ? $"{project.Name} - {project.ClientName}" : project.Name;

                var attributedText = new NSMutableAttributedString(projectLabelText);
                attributedText.AddAttribute(UIStringAttributeKey.ForegroundColor, color, new NSRange(0, project.Name.Length));
                if (hasClient)
                {
                    attributedText.AddAttribute(UIStringAttributeKey.ForegroundColor, Color.Black, new NSRange(project.Name.Length, 3 + project.ClientName.Length));
                }

                projectLabel.AttributedText = attributedText;
                button.Add(projectLabel);

                buttonConstraints.Add(projectLabel.ToRightOf(circle, 8));
                buttonConstraints.Add(projectLabel.Width().EqualTo(View.Frame.Width - 40));

                if (hasTask)
                {
                    var taskLabel = new UILabel();
                    taskLabel.Text = project.Task.Name;
                    taskLabel.TextColor = Color.Black;
                    button.Add(taskLabel);

                    buttonConstraints.Add(taskLabel.AtLeftOf(button, 36));
                    buttonConstraints.Add(taskLabel.Below(projectLabel, 2));

                    buttonConstraints.Add(circle.AtTopOf(button, 16));
                    buttonConstraints.Add(projectLabel.AtTopOf(button, 11));
                }
                else
                {
                    buttonConstraints.Add(circle.WithSameCenterY(button));
                    buttonConstraints.Add(projectLabel.WithSameCenterY(button));
                }

                button.SubviewsDoNotTranslateAutoresizingMaskIntoConstraints();
                button.AddConstraints(buttonConstraints);

                previousView = button;
            }

            tableHeader.SubviewsDoNotTranslateAutoresizingMaskIntoConstraints();
            tableHeader.AddConstraints(constraints);

            TableView.TableHeaderView = tableHeader;
        }

        protected void OnItemSelected(ICommonData m)
        {
            Guid projectId = Guid.Empty;
            Guid taskId = Guid.Empty;

            if (m is ProjectData)
            {
                if (!(m is ProjectsCollection.SuperProjectData) || !((ProjectsCollection.SuperProjectData)m).IsEmpty)
                {
                    projectId = m.Id;
                }

                if (m is ProjectListVM.CommonProjectData)
                {
                    var commonProjectData = (ProjectListVM.CommonProjectData)m;
                    if (commonProjectData.Task != null)
                    {
                        taskId = commonProjectData.Task.Id;
                    }
                }
            }
            else if (m is TaskData)
            {
                var task = (TaskData)m;
                projectId = task.ProjectId;
                taskId = task.Id;
            }

            handler.OnProjectSelected(projectId, taskId);
            NavigationController.PopViewController(true);
        }

        private void OnAddNewProject(object sender, EventArgs evt)
        {
            var newProjectController = new NewProjectViewController(viewModel.CurrentWorkspaceId, handler);
            NavigationController.PushViewController(newProjectController, true);
        }

        private void OnShowWorkspaceFilter(object sender, EventArgs evt)
        {
            var sourceRect = new CGRect(NavigationController.Toolbar.Bounds.Width - 45, NavigationController.Toolbar.Bounds.Height, 1, 1);

            bool hasPopover = ObjCRuntime.Class.GetHandle("UIPopoverPresentationController") != IntPtr.Zero;
            if (hasPopover)
            {
                var popoverController = new WorkspaceSelectorPopover(viewModel, UpdateTopProjectsHeader, sourceRect);
                PresentViewController(popoverController, true, null);
            }
            else
            {
                var nextWorkspace = viewModel.CurrentWorkspaceIndex + 1;
                if (nextWorkspace > viewModel.WorkspaceList.Count - 1)
                {
                    nextWorkspace = 0;
                }
                viewModel.ChangeWorkspaceByIndex(nextWorkspace);
            }
        }

        public class Source : ObservableCollectionViewSource<ICommonData, IClientData, IProjectData>
        {
            private readonly ProjectSelectionViewController owner;
            private readonly ProjectListVM viewModel;

            public Source(ProjectSelectionViewController owner, ProjectListVM viewModel) : base(owner.TableView, viewModel.ProjectList)
            {
                this.owner = owner;
                this.viewModel = viewModel;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                var index = GetPlainIndexFromRow(collection, indexPath);
                var data = collection[index];

                if (data is ProjectData)
                {
                    var cell = (ProjectCell)tableView.DequeueReusableCell(ProjectCellId);
                    cell.Bind((ProjectsCollection.SuperProjectData)data, viewModel.ProjectList.AddTasks);
                    return cell;
                }
                else
                {
                    var cell = (TaskCell)tableView.DequeueReusableCell(TaskCellId);
                    cell.Bind((TaskData)data);
                    return cell;
                }
            }

            public override UIView GetViewForHeader(UITableView tableView, nint section)
            {
                var index = GetPlainIndexFromSection(collection, section);
                var data = (ClientData)collection[index];

                var view = (SectionHeaderView)tableView.DequeueReusableHeaderFooterView(ClientHeaderId);
                view.Bind(data);
                return view;
            }

            public override nfloat GetHeightForHeader(UITableView tableView, nint section)
            {
                return EstimatedHeightForHeader(tableView, section);
            }

            public override nfloat EstimatedHeight(UITableView tableView, NSIndexPath indexPath) => CellHeight;

            public override nfloat EstimatedHeightForHeader(UITableView tableView, nint section) => HeaderCellHeight;

            public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath) => false;

            public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath) => CellHeight;

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                var index = GetPlainIndexFromRow(collection, indexPath);
                var data = collection[index];
                owner.OnItemSelected(data);

                tableView.DeselectRow(indexPath, true);
            }
        }

        private class ProjectCell : UITableViewCell
        {
            private CircleView circleView;
            private UILabel projectLabel;
            private UIButton tasksButton;

            private ProjectsCollection.SuperProjectData projectData;
            private Action<ProjectData> onPressedTagBtn;

            public ProjectCell(IntPtr handle) : base(handle)
            {
                this.Apply(Style.Screen);

                BackgroundColor = Color.White;

                Add(circleView = new CircleView());
                Add(tasksButton = new UIButton().Apply(Style.ProjectList.TasksButtons));
                Add(projectLabel = new UILabel().Apply(Style.ProjectList.ProjectLabel));

                tasksButton.TouchUpInside += OnTasksButtonTouchUpInside;

                this.SubviewsDoNotTranslateAutoresizingMaskIntoConstraints();
                this.AddConstraints(GenerateConstraints());
            }

            public void Bind(ProjectsCollection.SuperProjectData projectData, Action<ProjectData> onPressedTagBtn)
            {
                this.projectData = projectData;
                this.onPressedTagBtn = onPressedTagBtn;

                if (projectData.IsEmpty)
                {
                    circleView.Hidden = true;
                    tasksButton.Hidden = true;
                    projectLabel.Text = "ProjectNoProject".Tr();
                    projectLabel.Apply(Style.ProjectList.NoProjectLabel);
                    return;
                }

                var color = Color.FromHex(projectData.GetProperColor());

                circleView.Hidden = false;
                circleView.Color = color.CGColor;

                projectLabel.TextColor = color;
                projectLabel.Text = projectData.Name;

                tasksButton.Selected = false;
                tasksButton.Hidden = projectData.TaskNumber == 0;
                tasksButton.SetTitleColor(Color.Steel, UIControlState.Normal);
                tasksButton.SetTitle(projectData.TaskNumber.ToString(), UIControlState.Normal);
            }

            private IEnumerable<FluentLayout> GenerateConstraints()
            {
                //Circle
                yield return circleView.AtLeftOf(this, 16);
                yield return circleView.Width().EqualTo(12);
                yield return circleView.Height().EqualTo(12);
                yield return circleView.WithSameCenterY(this);

                //Tasks buttons
                if (!tasksButton.Hidden)
                {
                    yield return tasksButton.AtRightOf(this, 16);
                    yield return tasksButton.WithSameCenterY(this);
                }

                //Project
                yield return projectLabel.WithSameCenterY(this);
                yield return projectLabel.ToRightOf(circleView, 8);
                yield return projectLabel.Width().EqualTo(this.Frame.Width - 76);
            }

            private void OnTasksButtonTouchUpInside(object sender, EventArgs e)
            {
                if (onPressedTagBtn == null || projectData == null) return;
                onPressedTagBtn?.Invoke(projectData);
            }
        }

        private class TaskCell : UITableViewCell
        {
            private readonly UILabel taskNameLabel;

            public TaskCell(IntPtr handle) : base(handle)
            {
                Add(taskNameLabel = new UILabel().Apply(Style.ProjectList.TaskLabel));
                BackgroundColor = Color.FromHex("#ECEDED");

                taskNameLabel.TextColor = Color.Black;

                this.SubviewsDoNotTranslateAutoresizingMaskIntoConstraints();
                this.AddConstraints(GenerateConstraints());
            }

            public void Bind(TaskData taskData)
            {
                var taskName = string.IsNullOrWhiteSpace(taskData.Name) ? "ProjectNoNameTask".Tr() : taskData.Name;
                taskNameLabel.Text = taskName;
            }

            private IEnumerable<FluentLayout> GenerateConstraints()
            {
                yield return taskNameLabel.AtLeftOf(this, 36);
                yield return taskNameLabel.WithSameCenterY(this);
            }
        }

        private class SectionHeaderView : UITableViewHeaderFooterView
        {
            private readonly UILabel clientNameLabel;

            public SectionHeaderView(IntPtr ptr) : base(ptr)
            {
                BackgroundColor = Color.FromHex("#ECEDED");

                Add(clientNameLabel = new UILabel().Apply(Style.Log.HeaderDateLabel));

                this.SubviewsDoNotTranslateAutoresizingMaskIntoConstraints();
                this.AddConstraints(GenerateConstraints());
            }

            private IEnumerable<FluentLayout> GenerateConstraints()
            {
                yield return clientNameLabel.AtLeftOf(this, 16);
                yield return clientNameLabel.WithSameCenterY(this);
            }

            public void Bind(ClientData data)
            {
                var clientName = string.IsNullOrEmpty(data.Name) ? "ProjectNoClient".Tr() : data.Name;
                clientNameLabel.Text = clientName;
            }
        }

        class WorkspaceSelectorPopover : ObservableTableViewController<IWorkspaceData>, IUIPopoverPresentationControllerDelegate
        {
            private readonly ProjectListVM viewModel;
            private readonly Action updateTopProjectsHeader;
            private const int cellHeight = 45;

            public WorkspaceSelectorPopover(ProjectListVM viewModel, Action updateTopProjectsHeader, CGRect sourceRect)
            {
                this.viewModel = viewModel;
                this.updateTopProjectsHeader = updateTopProjectsHeader;
                ModalPresentationStyle = UIModalPresentationStyle.Popover;

                PopoverPresentationController.PermittedArrowDirections = UIPopoverArrowDirection.Up;
                PopoverPresentationController.BackgroundColor = UIColor.LightGray;
                PopoverPresentationController.SourceRect = sourceRect;
                PopoverPresentationController.Delegate = this;

                var height = (viewModel.WorkspaceList.Count < 5) ? (viewModel.WorkspaceList.Count + 1) : 5;
                PreferredContentSize = new CGSize(200, height * cellHeight);
            }

            public override void ViewDidLoad()
            {
                base.ViewDidLoad();

                UILabel headerLabel = new UILabel();
                headerLabel.Text = "Workspaces";
                headerLabel.Bounds = new CGRect(0, 10, 200, 40);
                headerLabel.Apply(Style.ProjectList.WorkspaceHeader);
                TableView.TableHeaderView = headerLabel;

                TableView.RowHeight = cellHeight;
                CreateCellDelegate = CreateWorkspaceCell;
                BindCellDelegate = BindCell;
                DataSource = new ObservableCollection<IWorkspaceData>(viewModel.WorkspaceList);
                PopoverPresentationController.SourceView = TableView;
            }

            private UITableViewCell CreateWorkspaceCell(NSString cellIdentifier)
            {
                return new UITableViewCell(UITableViewCellStyle.Default, cellIdentifier);
            }

            private void BindCell(UITableViewCell cell, IWorkspaceData workspaceData, NSIndexPath path)
            {
                // Set selected tags.
                cell.Accessory = (path.Row == viewModel.CurrentWorkspaceIndex) ? UITableViewCellAccessory.Checkmark : UITableViewCellAccessory.None;
                cell.TextLabel.Text = workspaceData.Name;
                cell.TextLabel.Apply(Style.ProjectList.WorkspaceLabel);
            }

            protected override void OnRowSelected(object item, NSIndexPath indexPath)
            {
                base.OnRowSelected(item, indexPath);
                TableView.DeselectRow(indexPath, true);
                if (indexPath.Row == viewModel.CurrentWorkspaceIndex)
                {
                    return;
                }

                viewModel.ChangeWorkspaceByIndex(indexPath.Row);
                // Set cell unselected
                foreach (var cell in TableView.VisibleCells)
                {
                    cell.Accessory = UITableViewCellAccessory.None;
                }

                updateTopProjectsHeader();
                TableView.CellAt(indexPath).Accessory = UITableViewCellAccessory.Checkmark;
                DismissViewController(true, null);
            }

            [Export("adaptivePresentationStyleForPresentationController:")]
            public UIModalPresentationStyle GetAdaptivePresentationStyle(UIPresentationController controller)
            {
                return UIModalPresentationStyle.None;
            }
        }
    }
}
