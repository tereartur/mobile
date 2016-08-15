using System;
using UIKit;
using Toggl.Ross.Views;

namespace Toggl.Ross.Theme
{
    public static partial class Style
    {
        public static class Feedback
        {
            public static void MoodLabel(UILabel v)
            {
                v.Font = UIFont.FromName("HelveticaNeue", 13f);
                v.TextAlignment = UITextAlignment.Center;
                v.TextColor = Color.LightGray;
            }

            public static void MoodSeparator(UIView v)
            {
                v.BackgroundColor = Color.LightGray;
            }

            public static void PositiveMoodButtonSelected(UIButton v)
            {
                PositiveMoodButton(v);
                v.SetImage(Image.IconPositiveFilled, UIControlState.Normal);
            }

            public static void PositiveMoodButton(UIButton v)
            {
                v.SetImage(Image.IconPositive, UIControlState.Normal);
            }

            public static void NeutralMoodButtonSelected(UIButton v)
            {
                NeutralMoodButton(v);
                v.SetImage(Image.IconNeutralFilled, UIControlState.Normal);
            }

            public static void NeutralMoodButton(UIButton v)
            {
                v.SetImage(Image.IconNeutral, UIControlState.Normal);
            }

            public static void NegativeMoodButtonSelected(UIButton v)
            {
                NegativeMoodButton(v);
                v.SetImage(Image.IconNegativeFilled, UIControlState.Normal);
            }

            public static void NegativeMoodButton(UIButton v)
            {
                v.SetImage(Image.IconNegative, UIControlState.Normal);
            }

            public static void MessageBorder(UIView v)
            {
                v.BackgroundColor = Color.LightGray.ColorWithAlpha(0.75f);
            }

            public static void MessageField(UITextView v)
            {
                v.BackgroundColor = Color.White;
                v.Font = UIFont.FromName("HelveticaNeue-Light", 17f);
                v.TextColor = Color.Black;
            }

            public static void SendButton(UIButton v)
            {
                v.Font = UIFont.FromName("HelveticaNeue-Light", 20f);
                v.SetBackgroundImage(Color.Green.ToImage(), UIControlState.Normal);
                v.SetTitleColor(Color.White, UIControlState.Normal);
            }

            public static void NoUserTitle(UILabel v)
            {
                v.Font = Font.Main(20f);
                v.TextColor = Color.OffBlack;
                v.Text = "EmptyStatesFeedbackTitle".Tr();
                v.TextAlignment = UITextAlignment.Center;
            }

            public static void NoUserSubtitle(UILabel v)
            {
                v.Font = Font.Main(16f);
                v.TextColor = Color.OffSteel;
                v.Text = "EmptyStatesFeedbackSubtitle".Tr();
                v.TextAlignment = UITextAlignment.Center;
            }
        }
    }
}
