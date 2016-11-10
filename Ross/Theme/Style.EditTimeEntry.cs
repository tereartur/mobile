using System;
using UIKit;
using Toggl.Ross.Views;

namespace Toggl.Ross.Theme
{
    public static partial class Style
    {
        public static class EditTimeEntry
        {
            public static void DateLabel(UILabel v)
            {
                v.Font = UIFont.FromName("HelveticaNeue-Light", 13f);
                v.TextAlignment = UITextAlignment.Center;
                v.TextColor = Color.Gray;
            }

            public static void DateLabelActive(UILabel v)
            {
                v.Apply(DateLabel);
                v.TextColor = Color.Red;
            }

            public static void TimeLabel(UILabel v)
            {
                v.Font = UIFont.FromName("HelveticaNeue", 13f);
                v.TextAlignment = UITextAlignment.Center;
                v.TextColor = Color.Black;
            }

            public static void TimeLabelActive(UILabel v)
            {
                v.Apply(TimeLabel);
                v.TextColor = Color.Red;
            }

            public static void DatePicker(UIDatePicker v)
            {
                v.BackgroundColor = Color.White;
            }

            public static void ProjectHintLabel(UILabel v)
            {
                v.Font = UIFont.FromName("HelveticaNeue-Light", 17f);
                v.TextColor = Color.Gray;
            }

            public static void ProjectLabel(UILabel v)
            {
                v.Font = UIFont.FromName("HelveticaNeue-Medium", 17f);
                v.TextColor = Color.Black;
            }

            public static void ClientLabel(UILabel v)
            {
                v.Font = UIFont.FromName("HelveticaNeue", 13f);
                v.TextColor = Color.Black.ColorWithAlpha(0.75f);
            }

            public static void TaskLabel(UILabel v)
            {
                v.Font = UIFont.FromName("HelveticaNeue", 13f);
                v.TextColor = Color.Black.ColorWithAlpha(0.75f);
            }

            public static void DescriptionField(TextField v)
            {
                v.BackgroundColor = Color.White;
                v.Font = UIFont.FromName("HelveticaNeue-Light", 17f);
                v.TextColor = Color.Black;
                v.TextEdgeInsets = new UIEdgeInsets(0, 15f, 0, 15f);
            }

            public static void TagsButton(UIButton v)
            {
                v.SetBackgroundImage(Color.White.ToImage(), UIControlState.Normal);
                v.SetBackgroundImage(Color.LightestGray.ToImage(), UIControlState.Highlighted);
                v.ContentEdgeInsets = new UIEdgeInsets(0, 15f, 0, 15f);
                v.HorizontalAlignment = UIControlContentHorizontalAlignment.Fill;
                v.LineBreakMode = UILineBreakMode.TailTruncation;
            }

            public static UIStringAttributes NoTags
            {
                get
                {
                    return new UIStringAttributes()
                    {
                        Font = UIFont.FromName("HelveticaNeue-Light", 17f),
                        ForegroundColor = Color.Gray,
                    };
                }
            }

            public static UIStringAttributes WithTags
            {
                get
                {
                    return new UIStringAttributes()
                    {
                        Font = UIFont.FromName("HelveticaNeue", 13f),
                        ForegroundColor = Color.Gray,
                    };
                }
            }

            public static void BillableContainer<T> (T v)
            where T : UIView
            {
                v.BackgroundColor = Color.White;
            }

            public static void BillableLabel(UILabel v)
            {
                v.Font = UIFont.FromName("HelveticaNeue-Light", 17f);
                v.TextColor = Color.Black;
            }

            public static void DeleteButton(UIButton v)
            {
                v.Font = UIFont.FromName("HelveticaNeue-Light", 17f);
                v.SetTitleColor(Color.White, UIControlState.Normal);
                v.SetBackgroundImage(Color.Gray.ToImage(), UIControlState.Normal);
            }
        }
    }
}
