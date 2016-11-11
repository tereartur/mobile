using System;
using Newtonsoft.Json;

namespace Toggl.Phoebe.Data.Json
{
    public class UserJson : CommonJson
    {
        [JsonProperty("fullname")]
        public string Name { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("password", NullValueHandling = NullValueHandling.Include)]
        public string Password { get; set; }

        [JsonProperty("google_access_token", NullValueHandling = NullValueHandling.Ignore)]
        public string GoogleAccessToken { get; set; }

        [JsonProperty("api_token", NullValueHandling = NullValueHandling.Ignore)]
        public string ApiToken { get; set; }

        [JsonProperty("beginning_of_week")]
        public DayOfWeek StartOfWeek { get; set; }

        [JsonProperty("date_format")]
        public string DateFormat { get; set; }

        [JsonProperty("timeofday_format")]
        public string TimeFormat { get; set; }

        [JsonProperty("image_url")]
        public string ImageUrl { get; set; }

        [JsonProperty("language")]
        public string Locale { get; set; }

        [JsonProperty("timezone")]
        public string Timezone { get; set; }

        [JsonProperty("send_product_emails")]
        public bool SendProductEmails { get; set; }

        [JsonProperty("send_timer_notifications")]
        public bool SendTimerNotifications { get; set; }

        [JsonProperty("send_weekly_report")]
        public bool SendWeeklyReport { get; set; }

        [JsonProperty("store_start_and_stop_time")]
        public bool StoreStartAndStopTime { get; set; }

        [JsonProperty("created_with")]
        public string CreatedWith { get; set; }

        [JsonProperty("default_wid")]
        public long DefaultWorkspaceRemoteId { get; set; }

        [JsonProperty("duration_format")]
        public DurationFormat DurationFormat { get; set; }
    }

    public class CloseAccountInfo
    {
        public class Options
        {
            [JsonProperty("question")]
            public int Question { get; set; }

            [JsonProperty("variants")]
            public string[] Variants { get; set; }
        }

        [JsonProperty("comments")]
        public string Comments { get; set; }

        [JsonProperty("closing_options")]
        public Options ClosingOptions { get; set; }

        public CloseAccountInfo()
        {
            Comments = string.Empty;
            ClosingOptions = new Options()
            {
                Question = 0,
                Variants = new string[0]
            };
        }
    }
}
