using SByteStream.BlackDuck.API;
using SByteStream.BlackDuck.API.InternalModels;
using SByteStream.BlackDuck.API.PublicModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SByteStream.BlackDuck
{
	public class BlackDuckReportGenerator
	{
		public BlackDuckReportGenerator(string url, string token)
		{
			m_url = url;
			m_token = token;			
		}

		public void GenerateStatusReport(List<BDProject> projects, string reportFile)
		{
			var result = GetRiskProfiles(projects);
			object _lock = new object();

			using (StreamWriter sw = new StreamWriter(reportFile))
			{
				sw.WriteLine("\"Project\",\"Version\",\"Security Critical\",\"Security High\",\"Security Medium\",\"Security Low\",\"License High\",\"License Medium\",\"License Low\",\"Operational High\",\"Operational Medium\",\"Operational Low\"");

				foreach(var row in result.Item1)
				{					
					BDProject bdProject = row.Item1;
					RiskProfile riskProfile = row.Item2;						
					sw.WriteLine($"\"{bdProject.Name}\",\"{bdProject.Version}\",\"{riskProfile.Categories.Vulnerability.Critical}\",\"{riskProfile.Categories.Vulnerability.High}\",\"{riskProfile.Categories.Vulnerability.Medium}\",\"{riskProfile.Categories.Vulnerability.Low}\",\"{riskProfile.Categories.License.High}\",\"{riskProfile.Categories.License.Medium}\",\"{riskProfile.Categories.License.Low}\",\"{riskProfile.Categories.Operational.High}\",\"{riskProfile.Categories.Operational.Medium}\",\"{riskProfile.Categories.Operational.Low}\"");
				}
			}

			foreach (Tuple<BDProject, Exception> row in result.Item2)
			{
				BDProject bdProject = row.Item1;
				Exception e = row.Item2;
				Console.WriteLine("Error retrieving risk profile for {0}:{1}, message: {2}", bdProject.Name,
					bdProject.Version, e.Message);
			}

			Console.WriteLine("Report has been written to {0}", reportFile);
		}

		private T GetThreadLocalData<T>(string slotName, T defaultValue)
		{
			LocalDataStoreSlot slot = Thread.GetNamedDataSlot(slotName);
			object threadObject = Thread.GetData(slot);
			if (threadObject == null)
			{
				return defaultValue;
			}
			else
			{
				return (T)threadObject;
			}
		}

		private void SetThreadLocalData(string slotName, object value)
		{
			LocalDataStoreSlot slot = Thread.GetNamedDataSlot(slotName);
			Thread.SetData(slot, value);		
		}

		public void GenerateVulnerabilityReport(List<BDProject> projects, string reportDirectory)
		{
			Console.Write("Retrieving projects status: ");
			int left = Console.CursorLeft;
			int count = 1;

			List<Tuple<BDProject, Exception>> errors = new List<Tuple<BDProject, Exception>>();
			List<Tuple<BDProject, string>> reports = new List<Tuple<BDProject, string>>();
			object _lock = new object();

			Parallel.ForEach(projects, _project =>
			{
				try
				{
					BlackDuckApi api = GetThreadLocalData<BlackDuckApi>("api", null);
					if (api == null)
					{ 					
						api = CreateApi(m_url, m_token).Result;
						SetThreadLocalData("api", api);
					}
					
					BlackDuckProject project = api.GetProject(_project.Name).Result;
					string report = api.GetVersionReport(project, _project.Version, reportDirectory).Result;
					lock (_lock)
					{
						reports.Add(new Tuple<BDProject, string>(_project, report));
					}

					int percent = count * 100 / projects.Count;
					Console.CursorLeft = left;
					Console.Write(string.Format("{0}%", percent));
					count++;
				}
				catch (Exception e)
				{
					lock (_lock)
					{
						errors.Add(new Tuple<BDProject, Exception>(_project, e));
					}
				}
			});			

			foreach (Tuple<BDProject, string> tuple in reports)
			{
				Console.WriteLine("{0} -> {1}", tuple.Item1, tuple.Item2);
			}

			if (errors.Count > 0)
			{
				Console.WriteLine();
				using (new AutoResetConsole(ConsoleColor.Red))
				{
					Console.WriteLine("Errors:");
					foreach (Tuple<BDProject, Exception> tuple in errors)
					{
						Console.WriteLine("{0}: {1}", tuple.Item1, tuple.Item2.Message);
						Console.WriteLine(tuple.Item2);
					}
				}
			}
		}

		private Tuple<List<Tuple<BDProject, RiskProfile>>, List<Tuple<BDProject, Exception>>> GetRiskProfiles(List<BDProject> projects)
		{			
			List<Tuple<BDProject, RiskProfile>> riskProfiles = new List<Tuple<BDProject, RiskProfile>>();
			List<Tuple<BDProject, Exception>> errorList = new List<Tuple<BDProject, Exception>>();

			Console.Write("Retrieving projects status: ");
			int left = Console.CursorLeft;
			int count = 1;
			object _lock = new object();

			Parallel.ForEach(projects, bdProject =>
			{
				try
				{
					BlackDuckApi api = GetThreadLocalData<BlackDuckApi>("api", null);
					if (api == null)
					{
						api = CreateApi(m_url, m_token).Result;
						SetThreadLocalData("api", api);
					}

					RiskProfile riskProfile = GetStatusReport(api, bdProject.Name, bdProject.Version);
					var riskProfileTuple = new Tuple<BDProject, RiskProfile>(bdProject, riskProfile);
					riskProfiles.Add(riskProfileTuple);

					int percent = count * 100 / projects.Count;
					Console.CursorLeft = left;
					Console.Write(string.Format("{0}%", percent));
					Interlocked.Increment(ref count);
				}
				catch (Exception e)
				{
					lock (_lock)
					{
						errorList.Add(new Tuple<BDProject, Exception>(bdProject, e));
					}
				}
			});

			Console.WriteLine("");
			return new Tuple<List<Tuple<BDProject, RiskProfile>>, List<Tuple<BDProject, Exception>>>(riskProfiles, errorList);
		}
		
		private RiskProfile GetStatusReport(BlackDuckApi api, string projectName, string versionString)
		{
			BlackDuckProject project = api.GetProject(projectName).Result;
			BlackDuckProjectVersion version = api.GetProjectVersion(project, versionString).Result;
			RiskProfile riskProfile = api.GetRiskProfile(version).Result;
			return riskProfile;
		}

		private async Task<BlackDuckApi> CreateApi(string url, string token)
		{
			BlackDuckApi api = new BlackDuckApi(url);
			string bearerToken = await api.Authenticate(token);
			return api;
		}
				
		private string m_url;
		private string m_token;
	}
}
