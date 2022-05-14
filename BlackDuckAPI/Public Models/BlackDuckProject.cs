using System;

namespace SByteStream.BlackDuck.API.PublicModels
{
	public class BlackDuckProject
	{
		public string Name { get; set; }

		public string CreatedBy { get; set; }

		public string UpdatedBy { get; set; }

		public DateTime UpdatedAt { get; set; }

		public string VersionsUrl { get; set; }
	}
}
