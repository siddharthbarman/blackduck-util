using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SByteStream.BlackDuck.API.InternalModels;
using SByteStream.BlackDuck.API.PublicModels;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SByteStream.BlackDuck.API
{
	public class BlackDuckApi
	{
		#region Initialization
		
		public BlackDuckApi(string serverUrl)
		{
			m_serverUrl = serverUrl;			

			ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) =>
			{
				return true;
			};
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

			m_client = new HttpClient();			
			m_client.BaseAddress = new Uri(m_serverUrl);
			MaxRetries = MAX_TRIES;
			RetryIntervalMS = RETRY_INTERVAL_MS;
		}

		public int MaxRetries
		{
			get;
			set;
		}

		public int RetryIntervalMS
		{
			get;
			set;
		}

		public async Task<string> Authenticate(string token)
		{
			m_client.DefaultRequestHeaders.Clear();
			m_client.DefaultRequestHeaders.Add("Authorization", string.Format("token {0}", token));			
			m_client.DefaultRequestHeaders.Add("Accept", "*/*");
			m_client.DefaultRequestHeaders.Add("User-Agent", USER_AGENT);
			
			Task<HttpResponseMessage> response = m_client.PostAsync("api/tokens/authenticate", null);
			HttpResponseMessage responseMessage = await response;
			
			string content = await responseMessage.Content.ReadAsStringAsync();
			JObject json = JObject.Parse(content);						
			m_bearerToken = json.SelectToken("bearerToken") ? .Value<string>();
			
			return m_bearerToken;
		}

		#endregion

		public async Task<BlackDuckProject> GetProject(string name)
		{
			m_client.DefaultRequestHeaders.Clear();
			m_client.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", m_bearerToken));
			m_client.DefaultRequestHeaders.Add("Accept", "*/*");
			m_client.DefaultRequestHeaders.Add("User-Agent", USER_AGENT);
			
			HttpResponseMessage response = await m_client.GetAsync(string.Format($"api/projects?q=name:{name}"));
			string content = await response.Content.ReadAsStringAsync();
			
			_BlackDuckProject _project = JsonConvert.DeserializeObject<_BlackDuckProject>(content);
			BlackDuckProject project = new BlackDuckProject
			{
				CreatedBy = _project.items[0].createdBy,
				Name = _project.items[0].name,
				UpdatedAt = _project.items[0].updatedAt,
				UpdatedBy = _project.items[0].updatedBy,
				VersionsUrl = _project.items[0]._meta.links.Where(l => l.rel == "versions").FirstOrDefault().href				
			};

			return project;
		}

		public async Task<BlackDuckProjectVersion> GetProjectVersion(BlackDuckProject project, string version)
		{
			_BlackDuckProjectVersions _versions = await GetProjectVersions(project);
			_BlackDuckProjectVersions.Item _versionItem = _versions.items.Where(i => i.versionName == version).FirstOrDefault();
			BlackDuckProjectVersion result = new BlackDuckProjectVersion
			{
				CreatedAt = _versionItem.createdAt,
				CreatedBy = _versionItem.createdBy,
				Name = _versionItem.versionName,
				Phase = _versionItem.phase,
				RiskProfileUrl = _versionItem._meta.links.Where(l => l.rel == "riskProfile").FirstOrDefault().href,
				VersionDetailReportUrl = _versionItem._meta.links.Where(l => l.rel == "versionReport").FirstOrDefault().href
			};
			return result;
		}

		public async Task<RiskProfile> GetRiskProfile(BlackDuckProjectVersion projectVersion)
		{
			m_client.DefaultRequestHeaders.Clear();
			m_client.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", m_bearerToken));
			m_client.DefaultRequestHeaders.Add("Accept", "*/*");
			m_client.DefaultRequestHeaders.Add("User-Agent", USER_AGENT);

			HttpResponseMessage response = await m_client.GetAsync(projectVersion.RiskProfileUrl);
			string content = await response.Content.ReadAsStringAsync();
			return JsonConvert.DeserializeObject<RiskProfile>(content);			
		}

		public async Task<string> GetVersionReport(BlackDuckProject project, string version, string directory)
		{
			_BlackDuckProjectVersions _versions = await GetProjectVersions(project);
			_BlackDuckProjectVersions.Item _versionItem = _versions.items.Where(i => i.versionName == version).FirstOrDefault();

			string reportUrl = _versionItem._meta.links.Where(l => l.rel == "versionReport").FirstOrDefault().href;
			BlackDuckReports.Item reportItem = await GenerateVersionReport(_versionItem);

			string report = await DownloadReport(reportItem, directory);
			return report;
		}

		private async Task<_BlackDuckProjectVersions> GetProjectVersions(BlackDuckProject project)
		{
			m_client.DefaultRequestHeaders.Clear();
			m_client.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", m_bearerToken));
			m_client.DefaultRequestHeaders.Add("Accept", "*/*");
			m_client.DefaultRequestHeaders.Add("User-Agent", USER_AGENT);

			HttpResponseMessage response = await m_client.GetAsync(project.VersionsUrl);
			string content = await response.Content.ReadAsStringAsync();
			return JsonConvert.DeserializeObject<_BlackDuckProjectVersions>(content);
		}		

		private async Task<BlackDuckReports.Item> GenerateVersionReport(_BlackDuckProjectVersions.Item item)
		{
			BlackDuckReports reports = await GetReports(item);
			int initialReportCount = reports.totalCount;			
			
			string reportGenerateUrl = item._meta.links.Where(l => l.rel == "versionReport").FirstOrDefault().href;
			string id = item._meta.href.Substring(item._meta.href.LastIndexOf("/") + 1);
			string bodyFormat = @"{{
				""categories"":[""COMPONENTS"",""BOM_COMPONENT_CUSTOM_FIELDS"",""LICENSE_TERM_FULFILLMENT"",""PROJECT_VERSION_CUSTOM_FIELDS"",""CODE_LOCATIONS"",""FILES"",""UPGRADE_GUIDANCE"",""VERSION"", ""SECURITY"",""VULNERABILITY_MATCH""],
				""versionId"":""{0}"",
				""reportType"":""VERSION"",
				""reportFormat"":""CSV""
			}}";

			m_client.DefaultRequestHeaders.Clear();
			m_client.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", m_bearerToken));
			m_client.DefaultRequestHeaders.Add("Accept", "application/json");
			m_client.DefaultRequestHeaders.Add("User-Agent", USER_AGENT);

			StringContent body = new StringContent(string.Format(bodyFormat, id), Encoding.UTF8, "application/json");			
			HttpResponseMessage response = await m_client.PostAsync(reportGenerateUrl, body);

			if (response.StatusCode != HttpStatusCode.Created)
			{
				throw new ApplicationException("Failed to generate report request");
			}

			bool stopLoop = false;
			BlackDuckReports.Item reportItem = null;

			while (!stopLoop)
			{
				Thread.Sleep(1000);
				reports = await GetReports(item);
				if (reports.totalCount > initialReportCount)
				{
					stopLoop = true;
					reportItem = reports.items.Where(i => i.createdAt == reports.items.Max(ii => ii.createdAt)).FirstOrDefault();					
				}
			}

			return reportItem;
		}

		private async Task<BlackDuckReports> GetVersionReports(BlackDuckProjectVersion item)
		{
			string reportUrl = item.VersionDetailReportUrl;

			m_client.DefaultRequestHeaders.Clear();
			m_client.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", m_bearerToken));
			m_client.DefaultRequestHeaders.Add("Accept", "application/json");
			m_client.DefaultRequestHeaders.Add("User-Agent", USER_AGENT);

			HttpResponseMessage response = await m_client.GetAsync(reportUrl);
			string content = await response.Content.ReadAsStringAsync();

			return JsonConvert.DeserializeObject<BlackDuckReports>(content);
		}

		public async Task<BlackDuckReports> GetReports(_BlackDuckProjectVersions.Item item)
		{
			string reportUrl = item._meta.links.Where(l => l.rel == "versionReport").FirstOrDefault().href;
			
			m_client.DefaultRequestHeaders.Clear();
			m_client.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", m_bearerToken));
			m_client.DefaultRequestHeaders.Add("Accept", "application/json");
			m_client.DefaultRequestHeaders.Add("User-Agent", USER_AGENT);

			HttpResponseMessage response = await m_client.GetAsync(reportUrl);
			string content = await response.Content.ReadAsStringAsync();

			return JsonConvert.DeserializeObject<BlackDuckReports>(content);
		}

		public async Task<string> DownloadReport(BlackDuckReports.Item reportItem, string downloadFolder)
		{
			string reportDownloadUrl = reportItem._meta.links.Where(l => l.rel == "download").FirstOrDefault().href;
			
			m_client.DefaultRequestHeaders.Clear();
			m_client.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", m_bearerToken));
			m_client.DefaultRequestHeaders.Add("Accept", "application/vnd.blackducksoftware.bdio+zip");
			m_client.DefaultRequestHeaders.Add("User-Agent", USER_AGENT);
			
			Stream istream = null;
			FileStream ostream = null;

			try
			{
				HttpResponseMessage response = null;
				string filename = null;
				int tries = 0;
				bool done = false;

				while (!done)
				{
					Thread.Sleep(RetryIntervalMS);
					tries++;

					response = await m_client.GetAsync(reportDownloadUrl);
					if (response.Content.Headers.ContentDisposition != null)
					{
						filename = response.Content.Headers.ContentDisposition.FileName;
						done = true;					
					}
					else
					{
						if (tries > MaxRetries) done = true;
					}					
				}

				if (filename == null)
				{
					throw new ApplicationException("DownloadReport timed out");
				}

				filename = filename.Trim('"');				
				string outputFilePath = Path.Combine(downloadFolder, filename);				
				istream = await response.Content.ReadAsStreamAsync();				
				byte[] buffer = new byte[READ_BUFFER_SIZE];

				ostream = File.Create(outputFilePath);

				while (true)
				{
					int read = istream.Read(buffer, 0, READ_BUFFER_SIZE);
					if (read > 0)
					{
						ostream.Write(buffer, 0, read);
					}
					if (read < READ_BUFFER_SIZE)
					{
						break;
					}
				}

				return outputFilePath;
			}			
			finally
			{
				if (istream != null) istream.Dispose();
				if (ostream != null) ostream.Dispose();
			}			
		}

		private string m_serverUrl;
		private string m_bearerToken;
		private HttpClient m_client;
		private const string USER_AGENT = "BlackDuck Report Generator";
		private const int READ_BUFFER_SIZE = 1024;
		private const int MAX_TRIES = 10;
		private const int RETRY_INTERVAL_MS = 2000;
	}
}
