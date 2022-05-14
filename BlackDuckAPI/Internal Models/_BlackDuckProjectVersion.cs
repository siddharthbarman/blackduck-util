using System;
using System.Collections.Generic;

namespace SByteStream.BlackDuck.API.InternalModels
{
	public class _BlackDuckProjectVersions
    {
        public int totalCount { get; set; }
        public List<Item> items { get; set; }
        public List<object> appliedFilters { get; set; }
        public Meta _meta { get; set; }

        public class Link
        {
            public string rel { get; set; }
            public string href { get; set; }
            public string name { get; set; }
            public string label { get; set; }
        }

        public class Meta
        {
            public List<string> allow { get; set; }
            public string href { get; set; }
            public List<Link> links { get; set; }
        }

        public class LicenseFamilySummary
        {
            public string name { get; set; }
            public string href { get; set; }
        }

        public class License2
        {
            public string license { get; set; }
            public List<object> licenses { get; set; }
            public string name { get; set; }
            public string ownership { get; set; }
            public string licenseDisplay { get; set; }
            public LicenseFamilySummary licenseFamilySummary { get; set; }
        }

        public class License
        {
            public string type { get; set; }
            public List<License> licenses { get; set; }
            public string licenseDisplay { get; set; }
        }

        public class Count
        {
            public string countType { get; set; }
            public int count { get; set; }
        }

        public class SecurityRiskProfile
        {
            public List<Count> counts { get; set; }
        }

        public class LicenseRiskProfile
        {
            public List<Count> counts { get; set; }
        }

        public class OperationalRiskProfile
        {
            public List<Count> counts { get; set; }
        }

        public class PolicyStatusSummary
        {
            public string name { get; set; }
            public string status { get; set; }
        }

        public class Item
        {
            public DateTime createdAt { get; set; }
            public string createdBy { get; set; }
            public string createdByUser { get; set; }
            public DateTime settingUpdatedAt { get; set; }
            public string settingUpdatedBy { get; set; }
            public string settingUpdatedByUser { get; set; }
            public string versionName { get; set; }
            public string source { get; set; }
            public string phase { get; set; }
            public string distribution { get; set; }
            public Meta _meta { get; set; }
            public License license { get; set; }
            public SecurityRiskProfile securityRiskProfile { get; set; }
            public LicenseRiskProfile licenseRiskProfile { get; set; }
            public OperationalRiskProfile operationalRiskProfile { get; set; }
            public string policyStatus { get; set; }
            public List<PolicyStatusSummary> policyStatusSummaries { get; set; }
            public DateTime lastBomUpdateDate { get; set; }
            public DateTime? lastScanDate { get; set; }
        }
    }
}
