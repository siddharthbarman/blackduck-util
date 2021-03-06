using System.Collections.Generic;

namespace SByteStream.BlackDuck
{
	public class BDProject
	{
		public string Name { get; set; }
		
		public string Version { get; set; }

		public List<string> Tags 
		{
			get { return m_tags; }
		}

		public override string ToString()
		{
			return string.Format("{0}:{1}", Name ?? string.Empty, Version ?? string.Empty);
		}

		private List<string> m_tags = new List<string>();
	}
}
