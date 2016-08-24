using UIKit;
using CoreGraphics;
using Toggl.Ross.Theme;
using Cirrious.FluentLayouts.Touch;

namespace Toggl.Ross.Views
{
    public class SimpleEmptyView : UIView
    {
        public SimpleEmptyView()
        {
  
            var border1 = new UIView().Apply(Style.EmptyView.Border);
            border1.Frame = new CGRect(0, 36, 375, 3);
            Add(border1);

            var border2 = new UIView().Apply(Style.EmptyView.Border);
            border2.Frame = new CGRect(0, 117, 375, 3);
            Add(border2);

            var border3 = new UIView().Apply(Style.EmptyView.Border);
            border3.Frame = new CGRect(0, 198, 375, 3);
            Add(border3);

            var border4 = new UIView().Apply(Style.EmptyView.Border);
            border4.Frame = new CGRect(0, 279, 375, 3);
            Add(border4);

            var item1 = new UIView().Apply(Style.EmptyView.Item);
            item1.Frame = new CGRect(0, 38, 375, 80);
            Add(item1);

            var item2 = new UIView().Apply(Style.EmptyView.Item);
            item2.Frame = new CGRect(0, 119, 375, 80);
            Add(item2);

            var item3 = new UIView().Apply(Style.EmptyView.Item);
            item3.Frame = new CGRect(0, 200, 375, 80);
            Add(item3);

            var circle1 = new CircleView();
            circle1.Color = Color.Pink.CGColor;
            circle1.SetFrame(108, 62, 8, 8);
            Add(circle1);
       
            var circle2 = new CircleView();
            circle2.SetFrame(132, 143, 8, 8);
            circle2.Color = Color.LightGreen.CGColor;
            Add(circle2);
       
            var circle3 = new CircleView();
            circle3.Color = Color.LightOrange.CGColor;
            circle3.SetFrame(100, 224, 8, 8);
            Add(circle3);

            // item1 content
            var item1_1 = new UIView().Apply(Style.EmptyView.ItemContentDark);
            item1_1.Frame = new CGRect(16, 62, 80, 8);
            Add(item1_1);

            var item1_2 = new UIView().Apply(Style.EmptyView.ItemContent);
            item1_2.Frame = new CGRect(311, 62, 48, 8);
            Add(item1_2);

            var item1_3 = new UIView().Apply(Style.EmptyView.ItemContent);
            item1_3.Frame = new CGRect(16, 86, 154, 8);
            Add(item1_3);

            //item2 content
            var item2_1 = new UIView().Apply(Style.EmptyView.ItemContentDark);
            item2_1.Frame = new CGRect(16, 143, 104, 8);
            Add(item2_1);

            var item2_2 = new UIView().Apply(Style.EmptyView.ItemContent);
            item2_2.Frame = new CGRect(311, 143, 48, 8);
            Add(item2_2);

            var item2_3 = new UIView().Apply(Style.EmptyView.ItemContent);
            item2_3.Frame = new CGRect(16, 167, 65, 8);
            Add(item2_3);

            // item3 content
            var item3_1 = new UIView().Apply(Style.EmptyView.ItemContentDark);
            item3_1.Frame = new CGRect(16, 224, 72, 8);
            Add(item3_1);

            var item3_2 = new UIView().Apply(Style.EmptyView.ItemContent);
            item3_2.Frame = new CGRect(311, 224, 48, 8);
            Add(item3_2);

            var item3_3 = new UIView().Apply(Style.EmptyView.ItemContent);
            item3_3.Frame = new CGRect(120, 224, 54, 8);
            Add(item3_3);

            var item3_4 = new UIView().Apply(Style.EmptyView.ItemContent);
            item3_4.Frame = new CGRect(16, 248, 126, 8);
            Add(item3_4);

           // Add(titleLabel = new UILabel().Apply(Style.EmptyView.TitleLabel));

           // Add(messageLabel = new UILabel().Apply(Style.EmptyView.MessageLabel));
        }

        public string Title
        {
            get;set;
        }

        public string Message
        {
            get; set;
        }
    }
}

