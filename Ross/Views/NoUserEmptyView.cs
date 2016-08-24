using System;
using Cirrious.FluentLayouts.Touch;
using Toggl.Ross.Theme;
using UIKit;

namespace Toggl.Ross.Views
{
    public class NoUserEmptyView : UIView
    {
        private readonly Action clickHandler;

        public NoUserEmptyView(Screen screen, Action clickHandler)
        {
            this.clickHandler = clickHandler;

            var firstLabelText = string.Empty;
            var secondLabelText = string.Empty;
            UIImage image = null;

            nfloat buttonToBottomMargin = 0;
            nfloat imageToTitleMargin = 0;
            nfloat subtitleToButtonMargin = 0;
           
            BackgroundColor = UIColor.FromRGB(250f, 251f, 252f);

            var firstLabel = CreateLabel();
            var secondLabel = CreateLabel();


            switch (screen)
            {
                case Screen.Reports:
                    firstLabel.Apply(Style.ReportsView.NoUserTitle);
                    secondLabel.Apply(Style.ReportsView.NoUserSubtitle);
                    image = Image.ReportsImageNoUser;
                    buttonToBottomMargin = 112;
                    imageToTitleMargin = 29;
                    subtitleToButtonMargin = 32;
                    break;
                    
                case Screen.Feedback:
                    firstLabel.Apply(Style.Feedback.NoUserTitle);
                    secondLabel.Apply(Style.Feedback.NoUserSubtitle);
                    image = Image.FeedbackImageNoUser;
                    buttonToBottomMargin = 201;
                    imageToTitleMargin = 16;
                    subtitleToButtonMargin = 40;
                    break;
            }


            var signUpButton = CreateButton("EmptyStatesSignUpForFree");

            var imageView = new UIImageView(image);

            InsertSubview(imageView, 0);
            Add(firstLabel);
            Add(secondLabel);
            secondLabel.Lines = 0;
            secondLabel.LineBreakMode = UILineBreakMode.WordWrap;
            Add(signUpButton);

			this.SubviewsDoNotTranslateAutoresizingMaskIntoConstraints();

            this.AddConstraints(

                //Bottom button
                signUpButton.AtBottomOf(this, buttonToBottomMargin),
                signUpButton.WithSameCenterX(this),
                signUpButton.Width().EqualTo(300),
                signUpButton.Height().EqualTo(56),

                //Sign up label
                secondLabel.Above(signUpButton, subtitleToButtonMargin),
                secondLabel.WithSameCenterX(this),

                //Boost productivity label
                firstLabel.Above(secondLabel, 8),
                firstLabel.WithSameCenterX(this),

                // Image
                imageView.Above(firstLabel, imageToTitleMargin),
                imageView.WithSameCenterX(this)
            );
        }

        private UILabel CreateLabel()
        {
            var label = new UILabel();
            return label;
        }

        private UIButton CreateButton(string text)
        {
            var button = new UIButton();
            button.TouchUpInside += HandleClick;
            button.SetTitle(text.Tr(), UIControlState.Normal);
            button.Apply(Style.EmptyView.SignUpForFreeButton);
            return button;
        }

        private void HandleClick(object sender, EventArgs e) => clickHandler?.Invoke();

        public enum Screen
        {
            Reports,
            Feedback
        }
   }
}