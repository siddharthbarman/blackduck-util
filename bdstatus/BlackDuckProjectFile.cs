using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SByteStream.BlackDuck
{
	public class BlackDuckProjectFile
	{
		public List<BDProject> GetProjectList(string projectListFile)
		{
			List<BDProject> result = new List<BDProject>();
			string[] lines = File.ReadAllLines(projectListFile);
			if (lines == null || lines.Length == 0)
			{
				return result;
			}

			foreach (string _line in lines)
			{
				string line = _line.Trim();
				
				if (line.StartsWith(COMMENT_CHARACTER))
				{
					continue;
				}

				if (!string.IsNullOrEmpty(line) && !line.StartsWith("#"))
				{
					string[] parts = line.Split(':');
					BDProject project = new BDProject
					{
						Name = parts[0],
						Version = parts[1]
					};
					result.Add(project);
				}
			}
			return result;
		}

		public const string COMMENT_CHARACTER = "#";
	}
}
