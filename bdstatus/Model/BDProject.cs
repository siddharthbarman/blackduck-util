namespace SByteStream.BlackDuck
{
	public class BDProject
	{
		public string Name { get; set; }
		
		public string Version { get; set; }

		public override string ToString()
		{
			return string.Format("{0}:{1}", Name ?? string.Empty, Version ?? string.Empty);
		}
	}
}
