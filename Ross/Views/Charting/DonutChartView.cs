using System;
using System.Collections.Generic;
using System.Linq;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using Toggl.Phoebe.Data.Models;
using Toggl.Phoebe.Reports;
using Toggl.Ross.Theme;
using UIKit;

namespace Toggl.Ross.Views.Charting
{
    public class DonutChartView : UIView, IReportChart, IXYDonutChartDataSource
    {
        private SummaryReportView _reportView;

        public SummaryReportView ReportView
        {
            get
            {
                return _reportView;
            }
            set
            {
                _reportView = value;

                if (_reportView.Projects == null)
                {
                    return;
                }

                var delayNoData = (_reportView.Projects.Count == 0) ? 0.6 : 0;
                var delayData = (_reportView.Projects.Count == 0) ? 0 : 0.6;

                if (_reportView.Projects.Count == 0)
                {
                    noProjectTitleLabel.Text = (_reportView.IsError) ? "ReportsDataErrorTitle".Tr() : "ReportsNoDataTitle".Tr();
                    noProjectTextLabel.Text = (_reportView.IsError) ? "ReportsDataErrorText".Tr() : "ReportsNoDataText".Tr();
                }

                UIView.Animate(0.4, delayNoData, UIViewAnimationOptions.TransitionNone,
                               () =>
                {
                    noProjectTextLabel.Alpha = (_reportView.Projects.Count == 0) ? 1 : 0;
                    noProjectTitleLabel.Alpha = (_reportView.Projects.Count == 0) ? 1 : 0;
                }, () => {});

                UIView.Animate(0.4, delayData, UIViewAnimationOptions.TransitionNone,
                               () =>
                {
                    moneyLabel.Alpha = (_reportView.Projects.Count == 0) ? 0 : 1;;
                    projectTableView.Alpha = (_reportView.Projects.Count == 0) ? 0 : 1;
                    totalTimeLabel.Alpha = (_reportView.Projects.Count == 0) ? 0 : 1;
                }, () =>
                {
                    grayCircle.Alpha = (_reportView.Projects.Count == 0) ? 1 : 0;
                });

                if (_reportView.Projects.Count == 0)
                {
                    grayCircle.Alpha = 1;
                }

                DonutProjectList = _reportView.CollapsedProjects;
                TableProjectList = _reportView.Projects;
                currencies = _reportView.TotalCost.OrderBy(s => s.Length).Reverse<string> ().ToList<string> ();

                donutChart.UserInteractionEnabled = (DonutProjectList.Count > 1);
                donutChart.ReloadData();
                projectTableView.ReloadData();

                SetTotalInfo();
            }
        }

        private bool _normalSelectionMode;

        public bool NormalSelectionMode
        {
            get
            {
                return _normalSelectionMode;
            }
            set
            {
                if (_normalSelectionMode == value)
                {
                    return;
                }
                _normalSelectionMode = value;

                for (var i = 0; i < projectTableView.NumberOfRowsInSection(0); i++)
                {
                    var cell = (ProjectReportCell)projectTableView.CellAt(NSIndexPath.FromRowSection(i, 0));
                    if (cell != null)
                    {
                        cell.NormalSelectionMode = value;
                        cell.SetSelected(cell.Selected, false);
                    }
                }
            }
        }

        public DonutChartView(CGRect frame) : base(frame)
        {
        }

        public DonutChartView()
        {
            TableProjectList = new List<ReportProject> ();
            DonutProjectList = new List<ReportProject> ();

            grayCircle = new UIView();
            grayCircle.Layer.AddSublayer(new CAShapeLayer() { FillColor = Color.DonutInactiveGray.CGColor});
            Add(grayCircle);

            donutChart = new XYDonutChart()
            {
                PieRadius = pieRadius,
                DonutLineStroke = lineStroke,
                DataSource = this,
                UserInteractionEnabled = true,
                SelectedSliceStroke = 0,
                ShowPercentage = false,
                StartPieAngle = (nfloat)Math.PI * 3 / 2,
                ShowLabel = false,
                AnimationSpeed = 1.0f,
                SelectedSliceOffsetRadius = 15f
            };
            Add(donutChart);

            donutChart.DidSelectSliceAtIndex += (sender, e) =>
            {
                NormalSelectionMode = true;
                var selectedProject = DonutProjectList [e.Index];
                var idx = TableProjectList.FindIndex(p => AreEquals(p, selectedProject));
                projectTableView.SelectRow(NSIndexPath.FromRowSection(idx, 0), true, UITableViewScrollPosition.Top);
                ((ProjectListSource)projectTableView.Source).LastSelectedIndex = idx;
                SetProjectInfo(selectedProject);
            };
            donutChart.DidDeselectSliceAtIndex += (sender, e) =>
            {
                var selectedProject = DonutProjectList [e.Index];
                var idx = TableProjectList.FindIndex(p => AreEquals(p, selectedProject));
                projectTableView.DeselectRow(NSIndexPath.FromRowSection(idx, 0), true);
            };
            donutChart.DidDeselectAllSlices += (sender, e) => DeselectAllProjects();

            projectTableView = new UITableView();
            projectTableView.RegisterClassForCellReuse(typeof(ProjectReportCell), ProjectReportCell.ProjectReportCellId);
            projectTableView.Source = new ProjectListSource(this);
            projectTableView.RowHeight = 40f;
            projectTableView.TableFooterView = new UIView();
            var insets = projectTableView.ScrollIndicatorInsets;
            insets.Right -= 3.0f;
            projectTableView.ScrollIndicatorInsets = insets;
            Add(projectTableView);

            totalTimeLabel = new UILabel();
            totalTimeLabel.Apply(Style.ReportsView.DonutTimeLabel);
            Add(totalTimeLabel);

            moneyLabel = new UILabel();
            moneyLabel.Apply(Style.ReportsView.DonutMoneyLabel);
            Add(moneyLabel);

            noProjectTitleLabel = new UILabel();
            noProjectTitleLabel.Apply(Style.ReportsView.NoProjectTitle);
            noProjectTitleLabel.Text = "ReportsLoadingTitle".Tr();
            Add(noProjectTitleLabel);

            noProjectTextLabel = new UILabel();
            noProjectTextLabel.Apply(Style.ReportsView.DonutMoneyLabel);
            noProjectTextLabel.Lines = 2;
            noProjectTextLabel.Text = "ReportsLoadingText".Tr();
            Add(noProjectTextLabel);

            topBoder = new UIView();
            bottomBoder = new UIView();
            Add(topBoder);
            Add(bottomBoder);

            projectTableView.Alpha = 0;
            moneyLabel.Alpha = 0;
            totalTimeLabel.Alpha = 0;
        }

        private XYDonutChart donutChart;
        private UITableView projectTableView;
        private UILabel totalTimeLabel;
        private UILabel moneyLabel;
        private UILabel noProjectTitleLabel;
        private UILabel noProjectTextLabel;
        private UIView grayCircle;
        private UIView topBoder;
        private UIView bottomBoder;
        private List<string> currencies;

        const float pieRadius = 80.0f;
        const float lineStroke = 40f;
        const float padding = 24f;
        const float diameter = pieRadius * 2 + lineStroke;

        public List<ReportProject> TableProjectList;
        public List<ReportProject> DonutProjectList;

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            grayCircle.Frame = new CGRect(0, 0, Bounds.Width, diameter + padding);
            ((CAShapeLayer)grayCircle.Layer.Sublayers [0]).Path = CGPathCreateArc(grayCircle.Center, pieRadius, 0, (nfloat)Math.PI * 2, lineStroke);
            donutChart.Frame = new CGRect(0, 0, Bounds.Width, diameter + padding);
            projectTableView.Frame = new CGRect(0, donutChart.Bounds.Height, Bounds.Width, Bounds.Height - donutChart.Bounds.Height);

            totalTimeLabel.Bounds = new CGRect(0, 0, donutChart.PieRadius * 2 - donutChart.DonutLineStroke, 20);
            totalTimeLabel.Center = new CGPoint(donutChart.PieCenter.X, donutChart.PieCenter.Y - 10);
            moneyLabel.Bounds = new CGRect(0, 0, donutChart.PieRadius * 2 - donutChart.DonutLineStroke, moneyLabel.Bounds.Height);
            moneyLabel.Center = new CGPoint(donutChart.PieCenter.X, donutChart.PieCenter.Y + moneyLabel.Bounds.Height / 2);

            noProjectTitleLabel.Bounds = new CGRect(0, 0, donutChart.PieRadius * 2, 20);
            noProjectTitleLabel.Center = new CGPoint(donutChart.PieCenter.X, donutChart.PieCenter.Y - 20);
            noProjectTextLabel.Bounds = new CGRect(0, 0, donutChart.PieRadius * 2, 35);
            noProjectTextLabel.Center = new CGPoint(donutChart.PieCenter.X, donutChart.PieCenter.Y + 5);

            DrawViewBoders(projectTableView, topBoder, bottomBoder);
        }

        public void SelectProjectAt(int index)
        {
            if (donutChart.UserInteractionEnabled)
            {
                NormalSelectionMode = true;
                var selectedProject = TableProjectList [index];
                var idx = DonutProjectList.FindIndex(p => AreEquals(p, selectedProject));
                if (idx != -1)
                {
                    donutChart.SetSliceSelectedAtIndex(idx);
                }
                else
                {
                    donutChart.DeselectAllSlices();
                }
                SetProjectInfo(selectedProject);
            }
        }

        public void DeselectProjectAt(int index)
        {
            if (donutChart.UserInteractionEnabled)
            {
                var selectedProject = TableProjectList [index];
                var idx = DonutProjectList.FindIndex(p => string.Compare(p.Project, selectedProject.Project, StringComparison.Ordinal) == 0 &&
                                                     p.TotalTime == selectedProject.TotalTime);
                if (idx != -1)
                {
                    donutChart.SetSliceDeselectedAtIndex(idx);
                }
            }
        }

        public void DeselectAllProjects()
        {
            donutChart.DeselectAllSlices();
            NormalSelectionMode = false;
            for (int i = 0; i < TableProjectList.Count; i++)
            {
                projectTableView.DeselectRow(NSIndexPath.FromRowSection(0, i), true);
            }
            SetTotalInfo();
            ((ProjectListSource)projectTableView.Source).LastSelectedIndex = -1;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            donutChart.Dispose();
        }

        private void SetProjectInfo(ReportProject selectedProject)
        {
            totalTimeLabel.Text = selectedProject.FormattedTotalTime;
            int currCount = selectedProject.Currencies.Count;

            if (currCount > 0)
            {
                totalTimeLabel.Center = new CGPoint(donutChart.PieCenter.X, donutChart.PieCenter.Y - 10);
                string moneyInfo = "";

                currCount = (selectedProject.Currencies.Count > 3) ? 3 : selectedProject.Currencies.Count;
                var  projectCurrencies = selectedProject.Currencies.OrderBy(s => s.Amount.ToString().Length).Reverse<ReportCurrency> ().ToList<ReportCurrency>();
                for (int i = 0; i < currCount; i++)
                {
                    moneyInfo += projectCurrencies[i].Amount + " " + projectCurrencies[i].Currency + "\n";
                }

                moneyInfo = moneyInfo.Substring(0, moneyInfo.Length - 1);
                moneyLabel.Alpha = 1.0f;
                moneyLabel.Text = moneyInfo;
            }
            else
            {
                totalTimeLabel.Center = new CGPoint(donutChart.PieCenter.X, donutChart.PieCenter.Y);
                moneyLabel.Alpha = 0.0f;
            }
            moneyLabel.SizeToFit();
        }

        private void SetTotalInfo()
        {
            totalTimeLabel.Text = _reportView.TotalGrand;
            int currCount = currencies.Count;

            if (currCount > 0)
            {
                string moneyInfo = "";
                currCount = (currencies.Count > 3) ? 3 : currencies.Count;
                for (int i = 0; i < currCount; i++)
                {
                    moneyInfo += currencies[i] + "\n";
                }
                moneyInfo = moneyInfo.Substring(0, moneyInfo.Length - 1);
                moneyLabel.Alpha = 1.0f;
                moneyLabel.Text = moneyInfo;
            }
            else
            {
                totalTimeLabel.Center = new CGPoint(donutChart.PieCenter.X, donutChart.PieCenter.Y);
                moneyLabel.Alpha = 0.0f;
            }
            moneyLabel.SizeToFit();
        }

        private CGPath CGPathCreateArc(CGPoint center, nfloat radius, nfloat startAngle, nfloat endAngle, nfloat lineStroke)
        {
            var path = new CGPath();
            path.AddArc(center.X, center.Y, radius, startAngle, endAngle, false);
            return path.CopyByStrokingPath(lineStroke, CGLineCap.Butt, CGLineJoin.Miter, 10);
        }

        private bool AreEquals(ReportProject a, ReportProject b)
        {
            return false || a.TotalTime == b.TotalTime && string.Compare(a.Project, b.Project, StringComparison.Ordinal) == 0 && a.Color == b.Color;
        }

        private void DrawViewBoders(UIView targetView, UIView tp, UIView btm)
        {
            var mask = new CAGradientLayer();
            mask.Frame = new CGRect(0, 0, targetView.Bounds.Width, 10);
            mask.Colors = new [] { UIColor.White.CGColor, UIColor.Clear.CGColor };

            tp.Frame = new CGRect(targetView.Frame.X, targetView.Frame.Y, targetView.Bounds.Width, 10);
            tp.BackgroundColor = UIColor.White;
            tp.UserInteractionEnabled = false;
            tp.Layer.Mask = mask;

            var maskInverted = new CAGradientLayer();
            maskInverted.Frame = new CGRect(0, 0, targetView.Frame.Width, 20);
            maskInverted.Colors = new [] { UIColor.Clear.CGColor, UIColor.White.CGColor};

            btm.Frame = new CGRect(targetView.Frame.X, targetView.Frame.Y + targetView.Bounds.Height - 20, targetView.Bounds.Width, 20);
            btm.BackgroundColor = UIColor.White;
            btm.UserInteractionEnabled = false;
            btm.Layer.Mask = maskInverted;
        }

        #region Pie Datasource

        public nint NumberOfSlicesInPieChart(XYDonutChart pieChart)
        {
            return DonutProjectList.Count;
        }

        public nfloat ValueForSliceAtIndex(XYDonutChart pieChart, nint index)
        {
            return DonutProjectList [(int)index].TotalTime;
        }

        public UIColor ColorForSliceAtIndex(XYDonutChart pieChart, nint index)
        {
            string hex;
            if (DonutProjectList [(int)index].Color == ProjectData.GroupedProjectColor)
            {
                hex = ProjectData.GroupedProjectColor;
            }
            else
            {
                hex = DonutProjectList[(int)index].Color;
            }
            return Color.FromHex(hex);
        }

        public string TextForSliceAtIndex(XYDonutChart pieChart, nint index)
        {
            return String.Empty;
        }

        #endregion

        internal class ProjectListSource : UITableViewSource
        {
            private readonly DonutChartView _owner;

            public int LastSelectedIndex
            {
                get;
                set;
            }

            public ProjectListSource(DonutChartView pieChart)
            {
                _owner = pieChart;
                LastSelectedIndex = -1;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = (ProjectReportCell)tableView.DequeueReusableCell(ProjectReportCell.ProjectReportCellId);
                cell.Data = _owner.TableProjectList [indexPath.Row];
                cell.NormalSelectionMode = _owner.NormalSelectionMode;
                return cell;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                return _owner.TableProjectList.Count;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                if (indexPath.Row != LastSelectedIndex)
                {
                    _owner.SelectProjectAt(indexPath.Row);
                    LastSelectedIndex = indexPath.Row;
                }
                else
                {
                    _owner.DeselectAllProjects();
                }
            }

            public override void RowDeselected(UITableView tableView, NSIndexPath indexPath)
            {
                _owner.DeselectProjectAt(indexPath.Row);
            }
        }
    }
}