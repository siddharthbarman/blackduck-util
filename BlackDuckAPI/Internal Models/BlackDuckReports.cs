using System;
using System.Collections.Generic;

namespace SByteStream.BlackDuck.API.InternalModels
{
	public class BlackDuckReports
	{
        public class Link
        {
            public string rel { get; set; }
            public string href { get; set; }
        }

        public class Meta
        {
            public List<string> allow { get; set; }
            public string href { get; set; }
            public List<Link> links { get; set; }
        }

        public class Item
        {
            public string reportFormat { get; set; }
            public string reportType { get; set; }
            public string locale { get; set; }
            public string fileName { get; set; }
            public string fileNamePrefix { get; set; }
            public int fileSize { get; set; }
            public string status { get; set; }
            public DateTime createdAt { get; set; }
            public DateTime updatedAt { get; set; }
            public DateTime finishedAt { get; set; }
            public string createdBy { get; set; }
            public string createdByUser { get; set; }
            public Meta _meta { get; set; }
        }
                
        public int totalCount { get; set; }
        public List<Item> items { get; set; }
        public List<object> appliedFilters { get; set; }
        public Meta _meta { get; set; }
    }
}
