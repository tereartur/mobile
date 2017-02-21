using System;
using CoreGraphics;
using Foundation;
using UIKit;
using Toggl.Phoebe.Data.Models;
using Toggl.Ross.Theme;

namespace Toggl.Ross.Views
{
    public class ProjectReportCell : UITableViewCell
    {
        public static NSString ProjectReportCellId = new NSString("ProjectCellId");

        private ReportProject _data;
        public ReportProject Data
        {
            get
            {
                return _data;
            }
            set
            {
                _data = value;

                string hex;
                if (_data.Color == ProjectData.GroupedProjectColor)
                {
                    hex = ProjectData.GroupedProjectColor;
                    projectTitleLabel.Text = string.Format("ReportsCellGroupedProject".Tr(), _data.Project);
                }
                else
                {
                    hex = _data.Color;
                    projectTitleLabel.Text = string.IsNullOrEmpty(_data.Project) ? "ReportsCellNoProject".Tr() : _data.Project;
                }
                circleView.BackgroundColor = Color.FromHex(hex);
                timeLabel.Text = _data.FormattedTotalTime;
                projectTitleLabel.SetNeedsDisplay();
            }
        }

        public bool NormalSelectionMode
        {
            get;
            set;
        }

        UILabel projectTitleLabel;
        UILabel timeLabel;
        UIView circleView;

        public ProjectReportCell(IntPtr handle) : base(handle)
        {
            SelectionStyle = UITableViewCellSelectionStyle.None;

            projectTitleLabel = new UILabel();
            projectTitleLabel.Apply(Style.ReportsView.ProjectCellTitleLabel);
            ContentView.Add(projectTitleLabel);

            timeLabel = new UILabel();
            timeLabel.Apply(Style.ReportsView.ProjectCellTimeLabel);
            ContentView.Add(timeLabel);

            circleView = new UIView();
            ContentView.Add(circleView);
            NormalSelectionMode = false;
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            nfloat radius = 17.0f;
            var contentFrame = ContentView.Frame;

            projectTitleLabel.Frame = new CGRect(radius + radius * 0.5f, (contentFrame.Height - 20) / 2, 250, 20);
            timeLabel.Frame = new CGRect(contentFrame.Width - 104, (contentFrame.Height - 20) / 2, 100, 20);
            circleView.Frame = new CGRect(0, (contentFrame.Height - radius) / 2, radius, radius);
        }

        public override void SetSelected(bool selected, bool animated)
        {
            base.SetSelected(selected, animated);

            if (NormalSelectionMode)
            {
                projectTitleLabel.Alpha = (selected) ? 1f : 0.4f;
                timeLabel.Alpha = (selected) ? 1f : 0.4f;
                circleView.Alpha = (selected) ? 1f : 0.4f;
                circleView.Layer.CornerRadius = (selected) ? circleView.Frame.Width / 2 : 5;
            }
            else
            {
                projectTitleLabel.Alpha = 1.0f;
                timeLabel.Alpha = 1.0f;
                circleView.Alpha = 1.0f;
                circleView.Layer.CornerRadius = 5;
            }
            SetNeedsDisplay();
        }
    }
}

