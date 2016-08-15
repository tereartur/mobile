using System;
using UIKit;

namespace Toggl.Ross.Theme
{
    public static partial class Style
    {
        public static class EmptyView
        {
            public static void TitleLabel(UILabel v)
            {
                v.Font = UIFont.FromName("HelveticaNeue", 17f);
                v.TextAlignment = UITextAlignment.Center;
                v.TextColor = Color.DarkGray;
            }

            public static void MessageLabel(UILabel v)
            {
                v.Font = UIFont.FromName("HelveticaNeue", 14f);
                v.Lines = 5;
                v.TextAlignment = UITextAlignment.Center;
                v.TextColor = Color.DarkGray;
            }

            public static void SignUpForFreeButton(UIButton v)
            {
                v.TitleLabel.Font = Font.Main(16f);
                v.SetTitleColor(Color.LightishGreen, UIControlState.Normal);
                v.Layer.BorderColor = Color.LightishGreen.CGColor;
                v.Layer.BorderWidth = 1;
                v.Layer.CornerRadius = 30f;
            }
        }
    }
}
