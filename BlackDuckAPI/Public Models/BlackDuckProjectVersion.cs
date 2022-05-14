using System;

namespace SByteStream.BlackDuck.API.PublicModels
{
	public class BlackDuckProjectVersion
	{
		public string Name { get; set; }

		public DateTime CreatedAt { get; set; }

		public string CreatedBy { get; set; }

		public string SettingUpdatedBy { get; set; }

		public string Phase { get; set; }

		public string RiskProfileUrl { get; set; }

		public string VersionDetailReportUrl { get; set; }
	}
}
