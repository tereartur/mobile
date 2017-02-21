using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Toggl.Phoebe.Data.Json
{
    public class ReportJson
    {
        [JsonProperty("total_grand")]
        public long TotalGrand { get; set; }

        [JsonProperty("total_billable")]
        public long TotalBillable { get; set; }

        [JsonProperty("total_currencies")]
        public List<ReportCurrencyJson> TotalCurrencies { get; set; }

        [JsonProperty("activity")]
        public ReportActivityJson ActivityContainer { get; set; }

        [JsonProperty("data")]
        public List<ReportProjectJson> Projects { get; set; }
    }

    #region Projects

    public class ReportProjectJson
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("title")]
        public ReportProjectDescJson Description { get; set; }

        [JsonProperty("time")]
        public long TotalTime { get; set; }

        [JsonProperty("total_currencies")]
        public List<ReportCurrencyJson> Currencies { get; set; }

        [JsonProperty("items")]
        public List<ReportTimeEntryJson> Items { get; set; }
    }

    public class ReportProjectDescJson
    {
        [JsonProperty("project")]
        public string Project { get; set; }

        [JsonProperty("client")]
        public string Client { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }

        [JsonProperty("hex_color")]
        public string HexColor { get; set; }
    }

    #endregion

    #region TimeEntry

    public class ReportTimeEntryJson
    {
        [JsonProperty("ids")]
        public string Ids { get; set; }

        [JsonProperty("title")]
        public ReportTimeEntryDescJson Description { get; set; }

        [JsonProperty("time")]
        public long Time { get; set; }

        [JsonProperty("cur")]
        public string Currency { get; set; }

        [JsonProperty("sum", Required = Required.AllowNull)]
        public float Sum { get; set; }

        [JsonProperty("rate", Required = Required.AllowNull)]
        public float Rate { get; set; }
    }

    public class ReportTimeEntryDescJson
    {
        [JsonProperty("time_entry")]
        public string Title { get; set; }
    }

    #endregion

    #region Activity

    public class ReportActivityJson : CommonJson
    {
        [JsonProperty("rows")]
        public List<List<string>> Rows { get; set; }

        [JsonProperty("zoom_level")]
        public string ZoomLevel { get; set; }
    }

    #endregion

    public class ReportCurrencyJson
    {
        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("amount")]
        public float Amount { get; set; }
    }
}