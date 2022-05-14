using System;
using System.Collections.Generic;

namespace SByteStream.BlackDuck.API.InternalModels
{
    public class _BlackDuckProject
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

        public class Item
        {
            public string name { get; set; }
            public bool projectLevelAdjustments { get; set; }
            public List<string> cloneCategories { get; set; }
            public bool customSignatureEnabled { get; set; }
            public int customSignatureDepth { get; set; }
            public bool deepLicenseDataEnabled { get; set; }
            public bool snippetAdjustmentApplied { get; set; }
            public bool licenseConflictsEnabled { get; set; }
            public DateTime createdAt { get; set; }
            public string createdBy { get; set; }
            public string createdByUser { get; set; }
            public DateTime updatedAt { get; set; }
            public string updatedBy { get; set; }
            public string updatedByUser { get; set; }
            public string source { get; set; }
            public Meta _meta { get; set; }
        }
    }
}
