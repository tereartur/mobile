using System;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using Android.Widget;
using Toggl.Chandler.UI.Activities;

namespace Toggl.Chandler.UI.Fragments
{
    public class TimerFragment : Fragment
    {
        private readonly Color greenButtonColor = Color.ParseColor("#ee4dd965");
        private readonly Color redButtonColor = Color.ParseColor("#eeff3c47");

        private readonly Handler handler = new Handler();
        private TextView DurationTextView;
        private TextView DescriptionTextView;
        private TextView ProjectTextView;
        private ProgressBar ProgressBar;
        private ImageButton ActionButton;
        private bool userLoggedIn = true;
        private bool timerEnabled = true;
        private Context context;
        private MainActivity activity;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.TimeEntryFragment, container, false);

            ActionButton = view.FindViewById<ImageButton>(Resource.Id.testButton);
            DurationTextView = view.FindViewById<TextView>(Resource.Id.DurationTextView);
            DescriptionTextView = view.FindViewById<TextView>(Resource.Id.DescriptionTextView);
            ProjectTextView = view.FindViewById<TextView>(Resource.Id.ProjectTextView);
            ProgressBar = view.FindViewById<ProgressBar>(Resource.Id.TimerProgressBar);

            ActionButton.Click += OnActionButtonClicked;

            activity = ((MainActivity)Activity);
            context = Activity.ApplicationContext;

            Rebind();

            return view;
        }

        public bool UserLoggedIn
        {
            get { return userLoggedIn; }
            set
            {
                userLoggedIn = value;
                Rebind();
            }
        }

        public bool TimerEnabled
        {
            get { return timerEnabled; }
            set
            {
                timerEnabled = value;
                Rebind();
            }
        }

        private SimpleTimeEntryData data => activity.Data[0];

        private void OnActionButtonClicked(object sender, EventArgs e)
        => activity.RequestStartStop();

        private void Rebind()
        {
            if (!IsAdded) return;

            switch (CurrentState)
            {
                case TimerState.New:
                    ButtonColor = greenButtonColor;
                    ActionButton.SetImageDrawable(context.Resources.GetDrawable(Resource.Drawable.IcPlay));
                    DurationTextView.Text = Resources.GetString(Resource.String.DurationNotRunningState);
                    ProjectTextView.Text = Resources.GetString(Resource.String.TimerBlankIntroduction);
                    DescriptionTextView.Text = Resources.GetString(Resource.String.WearNewBlankDescription);
                    break;

                case TimerState.Running:
                    ButtonColor = redButtonColor;
                    ActionButton.SetImageDrawable(context.Resources.GetDrawable(Resource.Drawable.IcStop));
                    var dur = data.GetDuration();
                    DurationTextView.Text = TimeSpan.FromSeconds((long)dur.TotalSeconds).ToString();
                    ProjectTextView.Text = data.Project;
                    DescriptionTextView.Text = string.IsNullOrWhiteSpace(data.Description)
                                               ? Resources.GetString(Resource.String.TimeEntryNoDescription)
                                               : data.Description;
                    break;

                case TimerState.Loading:
                    ActionButton.Visibility = ViewStates.Gone;

                    if (UserLoggedIn)
                    {
                        DescriptionTextView.Text = string.Empty;
                        ProgressBar.Visibility = ViewStates.Visible;
                        DurationTextView.Text = Resources.GetString(Resource.String.TimerLoading);
                    }
                    else
                    {
                        ProgressBar.Visibility = ViewStates.Gone;
                        DurationTextView.Text = Resources.GetString(Resource.String.TimerWaiting);
                        DescriptionTextView.Text = Resources.GetString(Resource.String.TimerNotLoggedIn);
                    }
                    break;

                case TimerState.Waiting:
                    ProjectTextView.Text = string.Empty;
                    DescriptionTextView.Text = Resources.GetString(Resource.String.TimerRequestSent);
                    DurationTextView.Text = string.Empty;
                    ActionButton.Visibility = ViewStates.Gone;
                    ProgressBar.Visibility = ViewStates.Visible;
                    break;
            }

            handler.RemoveCallbacks(Rebind);
            handler.PostDelayed(Rebind, 1000);
        }

        private Color ButtonColor
        {
            set
            {
                ProgressBar.Visibility = ViewStates.Gone;
                ActionButton.Visibility = ViewStates.Visible;
                var shape = ActionButton.Background as GradientDrawable;
                shape.SetColor(value);
            }
        }

        private TimerState CurrentState
        {
            get
            {
                if (activity.Data.Count == 0)
                {
                    return TimerState.Loading;
                }

                if (!timerEnabled)
                {
                    return TimerState.Waiting;
                }

                return data.IsRunning ? TimerState.Running : TimerState.New;
            }
        }

        private enum TimerState
        {
            New,
            Running,
            Loading,
            Waiting
        }
    }
}
