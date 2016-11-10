using System;
using CoreGraphics;
using System.Globalization;
using UIKit;

namespace Toggl.Ross.Theme
{
    public static class UIColorExtensions
    {
        public static UIImage ToImage(this UIColor color)
        {
            var size = new CGSize(1f, 1f);

            UIGraphics.BeginImageContext(size);
            var ctx = UIGraphics.GetCurrentContext();

            ctx.SetFillColor(color.CGColor);
            ctx.FillRect(new CGRect(CGPoint.Empty, size));

            var image = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();

            return image;
        }
    }
}