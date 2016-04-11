using System;
using Foundation;
using UIKit;
using Toggl.Phoebe;
using Toggl.Phoebe.Net;
using CoreGraphics;
using Toggl.Ross.Theme;
using Toggl.Phoebe.Reactive;
using System.Reactive.Linq;
using System.Threading;
using Toggl.Phoebe.Data.Models;
using Toggl.Phoebe.Data;

namespace Toggl.Ross.ViewControllers
{
    public class MainViewController : UINavigationController
    {
        private UIAlertView upgradeAlert;
        private UIView fadeView;

        private UITapGestureRecognizer tapGesture;
        private UIPanGestureRecognizer mainPanGesture, leftPanGestue;
        private CGPoint draggingPoint;

        private const float menuSlideAnimationDuration = .3f;
        private const int menuOffset = 60;
        private const int velocityTreshold = 100;
        private LeftViewController menu;

        private nfloat Width { get { return View.Frame.Width; } }
        private nfloat CurrentX { get { return View.Frame.X; } }
        private nfloat MaxDraggingX { get { return Width - menuOffset; } }
        private nfloat MinDraggingX { get { return 0; } }
        private bool MenuOpen {  get { return 0 != CurrentX; }}
        private IDisposable stateObserver;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            View.Apply(Style.Screen);
            NavigationBar.Apply(Style.NavigationBar);
            Delegate = new NavDelegate();

            mainPanGesture = new UIPanGestureRecognizer(OnMainPanGesture)
            {
                // TODO: TableView scroll gestures are not
                // compatible with the open / close pan gesture.
                ShouldRecognizeSimultaneously = (a, b) => !(b.View is UITableView),
                CancelsTouchesInView = true,
            };
            View.AddGestureRecognizer(mainPanGesture);

            fadeView = new UIView();
            fadeView.BackgroundColor = UIColor.FromRGBA(29f / 255f, 29f / 255f, 28f / 255f, 0.5f);
            fadeView.Frame = new CGRect(0, 0, View.Frame.Width, View.Frame.Height);
            fadeView.Hidden = true;

            tapGesture = new UITapGestureRecognizer(CloseMenu)
            {
                ShouldReceiveTouch = (a, b) => true,
                ShouldRecognizeSimultaneously = (a, b) => true,
                CancelsTouchesInView = true,
            };
            fadeView.AddGestureRecognizer(tapGesture);
            View.Add(fadeView);

            // ATTENTION Suscription to state (settings) changes inside
            // the view. This will be replaced for "router"
            // modified in the reducers.
            stateObserver = StoreManager.Singleton
                            .Observe(x => x.State.User)
                            .StartWith(StoreManager.Singleton.AppState.User)
                            .ObserveOn(SynchronizationContext.Current)
                            .DistinctUntilChanged(x => x.Id)
                            .Subscribe(userData => ResetRootViewController(userData));
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            Application.MarkLaunched();
            menu = new LeftViewController(OnMenuButtonSelected);
            View.Window.InsertSubview(menu.View, 0);

            leftPanGestue = new UIPanGestureRecognizer(OnLeftPanGesture)
            {
                CancelsTouchesInView = true
            };
            menu.View.AddGestureRecognizer(leftPanGestue);

            var user = StoreManager.Singleton.AppState.User;
            menu.ConfigureUserData(user.Name, user.Email, user.ImageUrl);
        }

        public override void ViewWillDisappear(bool animated)
        {
            // Dispose elements created when ViewDidAppear.
            menu.View.RemoveGestureRecognizer(leftPanGestue);
            menu.View.RemoveFromSuperview();
            menu.Dispose();
            leftPanGestue.Dispose();
            base.ViewWillDisappear(animated);
        }

        private void ResetRootViewController(IUserData userData)
        {
            UIViewController vc = null;
            bool isUserLogged = userData.Id != Guid.Empty;
            bool emptyStack = ViewControllers.Length < 1;

            if (isUserLogged)
            {
                // TODO Rx @alfonso Keep this call here explicitly or init
                // the state with the request if user is logged.
                if (emptyStack)
                    RxChain.Send(new ServerRequest.GetChanges());
                vc = new LogViewController();
                MenuEnabled = true;
            }
            else
            {
                vc = new WelcomeViewController();
                MenuEnabled = false;
            }

            if (menu != null)
                menu.ConfigureUserData(userData.Name, userData.Email, userData.ImageUrl);
            SetViewControllers(new [] { vc }, !emptyStack);
        }

        private void OnMenuButtonSelected(int btnId)
        {
            if (btnId == LeftViewController.TimerPageId)
            {
                if (ViewControllers.Length > 1 && ViewControllers[0] is LogViewController)
                {
                    PopViewController(true);
                }
            }
            else if (btnId == LeftViewController.ReportsPageId)
            {
                PushViewController(new ReportsViewController(), true);
            }
            else if (btnId == LeftViewController.SettingsPageId)
            {
                PushViewController(new SettingsViewController(), true);
            }
            else if (btnId == LeftViewController.FeedbackPageId)
            {
                PushViewController(new FeedbackViewController(), true);
            }
            else
            {
                RxChain.Send(new DataMsg.ResetState());
            }

            CloseMenu();
        }

        #region LeftMenu utils

        // TODO: Because the gesture of some events
        // is the same to the gesture of open/close
        // main Menu, this flag could let external objects to
        // deactivate it. This behaviour will change soon.
        public bool MenuEnabled { get; set; }

        public void ToggleMenu()
        {
            if (MenuOpen)
            {
                CloseMenu();
            }
            else
            {
                OpenMenu();
            }
        }

        private void OnMainPanGesture(UIPanGestureRecognizer recognizer)
        {
            if (!MenuEnabled)
            {
                return;
            }

            var translation = recognizer.TranslationInView(recognizer.View);
            var movement = translation.X - draggingPoint.X;

            switch (recognizer.State)
            {
                case UIGestureRecognizerState.Began:
                    draggingPoint = translation;
                    break;
                case UIGestureRecognizerState.Changed:
                    var newX = CurrentX;
                    newX += movement;
                    if (newX > MinDraggingX && newX < MaxDraggingX)
                    {
                        MoveToLocation(newX);
                    }
                    draggingPoint = translation;
                    break;
                case UIGestureRecognizerState.Ended:
                    if (Math.Abs(translation.X) >= velocityTreshold)
                    {
                        if (translation.X < 0)
                        {
                            CloseMenu();
                        }
                        else
                        {
                            OpenMenu();
                        }
                    }
                    else
                    {
                        if (Math.Abs(CurrentX) < (Width - menuOffset) / 2)
                        {
                            CloseMenu();
                        }
                        else
                        {
                            OpenMenu();
                        }
                    }
                    break;
            }
        }

        private void OnLeftPanGesture(UIPanGestureRecognizer recognizer)
        {
            var translation = recognizer.TranslationInView(recognizer.View);
            var movement = translation.X - draggingPoint.X;
            var currentX = View.Frame.X;

            switch (recognizer.State)
            {
                case UIGestureRecognizerState.Began:
                    draggingPoint = translation;
                    break;

                case UIGestureRecognizerState.Changed:
                    var newX = currentX;
                    newX += movement;
                    if (newX > MinDraggingX && newX < MaxDraggingX)
                    {
                        MoveToLocation(newX);
                    }
                    draggingPoint = translation;
                    break;

                case UIGestureRecognizerState.Ended:
                    if (Math.Abs(translation.X) >= velocityTreshold)
                    {
                        if (translation.X < 0)
                        {
                            CloseMenu();
                        }
                        else
                        {
                            OpenMenu();
                        }
                    }
                    else
                    {
                        if (Math.Abs(currentX) < (View.Frame.Width - menuOffset) / 2)
                        {
                            CloseMenu();
                        }
                        else
                        {
                            OpenMenu();
                        }
                    }
                    break;
            }
        }

        private void CloseMenu()
        {
            fadeView.Hidden = true;
            UIView.Animate(menuSlideAnimationDuration, 0, UIViewAnimationOptions.CurveEaseOut, () => MoveToLocation(0), null);
        }

        private void OpenMenu()
        {
            UIView.Animate(menuSlideAnimationDuration, 0, UIViewAnimationOptions.CurveEaseOut, () => MoveToLocation(Width - menuOffset), () =>
            {
                fadeView.Hidden = false;
            });
        }

        private void MoveToLocation(nfloat x)
        {
            var rect = View.Frame;
            rect.Y = 0;
            rect.X = x;
            View.Frame = rect;
        }

        private void OnTogglHttpResponse(TogglHttpResponseMessage msg)
        {
            // TODO Rx Activate update mechanism.
            if (msg.StatusCode == System.Net.HttpStatusCode.Gone)
            {
                if (upgradeAlert == null)
                {
                    upgradeAlert = new UIAlertView(
                        "MainUpdateNeededTitle".Tr(),
                        "MainUpdateNeededMessage".Tr(),
                        null, "MainUpdateNeededOk".Tr());
                    upgradeAlert.Clicked += (s, e) => UIApplication.SharedApplication.OpenUrl(new NSUrl(Build.AppStoreUrl));
                }
                upgradeAlert.Show();
            }
        }

        #endregion

        private class NavDelegate : UINavigationControllerDelegate
        {
            public UIPercentDrivenInteractiveTransition InteractiveTransition { get; set; }

            public override IUIViewControllerAnimatedTransitioning GetAnimationControllerForOperation(UINavigationController navigationController, UINavigationControllerOperation operation, UIViewController fromViewController, UIViewController toViewController)
            {
                if (toViewController is DurationChangeViewController)
                {
                    var durationController = (DurationChangeViewController)toViewController;
                    durationController.PreviousControllerType = fromViewController.GetType();
                    return new DurationChangeViewController.PushAnimator();
                }
                if (fromViewController is DurationChangeViewController)
                {
                    var durationController = (DurationChangeViewController)fromViewController;
                    if (durationController.PreviousControllerType == toViewController.GetType())
                    {
                        return new DurationChangeViewController.PopAnimator();
                    }
                    durationController.PreviousControllerType = null;
                }
                return null;
            }

            public override IUIViewControllerInteractiveTransitioning GetInteractionControllerForAnimationController(UINavigationController navigationController, IUIViewControllerAnimatedTransitioning animationController)
            {
                return InteractiveTransition;
            }
        }
    }

}