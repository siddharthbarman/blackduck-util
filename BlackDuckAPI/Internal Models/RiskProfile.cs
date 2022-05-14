using Newtonsoft.Json;
using System;

namespace SByteStream.BlackDuck.API.InternalModels
{
	public partial class RiskProfile
    {
        [JsonProperty("categories")]
        public Categories Categories { get; set; }

        [JsonProperty("bomLastUpdatedAt")]
        public DateTimeOffset BomLastUpdatedAt { get; set; }

        [JsonProperty("_meta")]
        public Meta Meta { get; set; }
    }

    public partial class Categories
    {
        [JsonProperty("OPERATIONAL")]
        public Activity Operational { get; set; }

        [JsonProperty("VULNERABILITY")]
        public Activity Vulnerability { get; set; }

        [JsonProperty("VERSION")]
        public Activity Version { get; set; }

        [JsonProperty("ACTIVITY")]
        public Activity Activity { get; set; }

        [JsonProperty("LICENSE")]
        public Activity License { get; set; }
    }

    public partial class Activity
    {
        [JsonProperty("HIGH")]
        public long High { get; set; }

        [JsonProperty("MEDIUM")]
        public long Medium { get; set; }

        [JsonProperty("LOW")]
        public long Low { get; set; }

        [JsonProperty("OK")]
        public long Ok { get; set; }

        [JsonProperty("UNKNOWN")]
        public long Unknown { get; set; }

        [JsonProperty("CRITICAL")]
        public long Critical { get; set; }
    }

    public partial class Meta
    {
        [JsonProperty("allow")]
        public string[] Allow { get; set; }

        [JsonProperty("href")]
        public Uri Href { get; set; }

        [JsonProperty("links")]
        public Link[] Links { get; set; }
    }

    public partial class Link
    {
        [JsonProperty("rel")]
        public string Rel { get; set; }

        [JsonProperty("href")]
        public Uri Href { get; set; }
    }
}
