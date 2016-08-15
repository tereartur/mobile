using System;
using UIKit;

namespace Toggl.Ross.Theme
{
    public static partial class Style
    {
        public static class ReportsView
        {
            public static void SelectorButton(UIButton v)
            {
                v.Font = UIFont.FromName("HelveticaNeue-Thin", 18f);
                v.LineBreakMode = UILineBreakMode.Clip;
                v.SetTitleColor(Color.Black, UIControlState.Normal);
                v.SetTitleColor(Color.Gray, UIControlState.Highlighted);
            }

            public static void DateSelectorLabel(UILabel v)
            {
                v.Font = UIFont.FromName("HelveticaNeue-Thin", 20f);
                v.LineBreakMode = UILineBreakMode.Clip;
                v.TextColor = Color.Gray;
                v.BackgroundColor = UIColor.Clear;
                v.TextAlignment = UITextAlignment.Center;
            }

            public static void DateSelectorLeftArrowButton(UIButton v)
            {
                v.SetTitle(String.Empty, UIControlState.Normal);
                v.SetImage(UIImage.FromFile("btn-arrow-left.png"), UIControlState.Normal);
            }

            public static void DateSelectorRightArrowButton(UIButton v)
            {
                v.SetTitle(String.Empty, UIControlState.Normal);
                v.SetImage(UIImage.FromFile("btn-arrow-right.png"), UIControlState.Normal);
            }

            public static void ProjectCellTitleLabel(UILabel v)
            {
                v.Font = UIFont.FromName("HelveticaNeue", 14f);
                v.TextColor = Color.ChartTopLabel;
                v.TextAlignment = UITextAlignment.Left;
            }

            public static void ProjectCellTimeLabel(UILabel v)
            {
                v.Font = UIFont.FromName("HelveticaNeue-Thin", 14f);
                v.TextColor = Color.DarkGray;
                v.TextAlignment = UITextAlignment.Right;
            }

            public static void DonutTimeLabel(UILabel v)
            {
                v.Font = UIFont.FromName("HelveticaNeue-Thin", 18f);
                v.TextColor = Color.DarkGray;
                v.TextAlignment = UITextAlignment.Center;
            }

            public static void DonutMoneyLabel(UILabel v)
            {
                v.Font = UIFont.FromName("HelveticaNeue-Thin", 13f);
                v.TextColor = Color.DarkGray;
                v.TextAlignment = UITextAlignment.Center;
                v.Lines = 0;
                v.LineBreakMode = UILineBreakMode.WordWrap;
            }

            public static void NoProjectTitle(UILabel v)
            {
                v.Font = UIFont.FromName("HelveticaNeue-Bold", 15f);
                v.TextColor = Color.DarkGray;
                v.TextAlignment = UITextAlignment.Center;
            }

            public static void BarCharLabelTitle(UILabel v)
            {
                v.Font = UIFont.FromName("HelveticaNeue-Medium", 13f);
                v.TextColor = Color.ChartTopLabel;
                v.TextAlignment = UITextAlignment.Left;
            }

            public static void BarCharLabelValue(UILabel v)
            {
                v.Font = UIFont.FromName("HelveticaNeue", 12f);
                v.TextColor = Color.ChartTopLabel;
                v.TextAlignment = UITextAlignment.Right;
            }

            public static void NoUserTitle(UILabel v)
            {
                v.Font = Font.Main(20f);
                v.TextColor = Color.OffBlack;
                v.Text = "EmptyStatesBoostProductivity".Tr();
                v.TextAlignment = UITextAlignment.Center;
            }

            public static void NoUserSubtitle(UILabel v)
            {
                v.Font = Font.Main(16f);
                v.TextColor = Color.OffSteel;
                v.Text = "EmptyStatesSignUp".Tr();
                v.TextAlignment = UITextAlignment.Center;
            }
        }
    }
}
