using SByteStream.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

namespace SByteStream.BlackDuck
{
	class Program
	{
		static void Help()
		{
			Console.WriteLine("Create csv report for your BlackDuck projects.");
			Console.WriteLine("Syntax:");
			Console.WriteLine("bdutil <command> -p <project-file-list.txt> -o <reportfile> -u <url> -t <token> -d");
			Console.WriteLine("command: Either status or genrep. Status generated high level report, genrep created downloadable version report.");
			Console.WriteLine("-p     : Input file containining name and version of BlackDuck projects <name>:<version>");
			Console.WriteLine("-o     : Output report csv file or directory in case of generating report.");
			Console.WriteLine("-u     : Url of the Blackduck server. Optional, if empty read from appconfig.");
			Console.WriteLine("-t     : Token for the Blackduck server. Optional, if empty read from appconfig.");
			Console.WriteLine("-d     : Displays detailed errors, optional, default is OFF.");
		}

		static void GetConfiguration(out string url, out string token, out int maxRetries, out int retryInterval)
		{
			token = null;
			url = ConfigurationManager.AppSettings["url"];
			if (string.IsNullOrEmpty(url))
			{
				throw new ApplicationException("url is not configured in application settings.");				
			}

			token = ConfigurationManager.AppSettings["token"];
			if (string.IsNullOrEmpty(token))
			{
				throw new ApplicationException("token is not configured in application settings.");				
			}

			string value = ConfigurationManager.AppSettings["maxRetries"];
			if (string.IsNullOrEmpty(value))
			{
				throw new ApplicationException("Invalid value specified for maxRetries in application settings.");
			}			
			if (!int.TryParse(value, out maxRetries))
			{
				throw new ApplicationException("Invalid value specified for maxRetries in application settings.");
			}
			if (maxRetries < 0)
			{
				throw new ApplicationException("Invalid value specified for maxRetries in application settings.");
			}

			value = ConfigurationManager.AppSettings["retryInterval"];
			if (string.IsNullOrEmpty(value))
			{
				throw new ApplicationException("Invalid value specified for retryInterval in application settings.");
			}
			if (!int.TryParse(value, out retryInterval))
			{
				throw new ApplicationException("Invalid value specified for retryInterval in application settings.");
			}
			if (retryInterval < 0)
			{
				throw new ApplicationException("Invalid value specified for retryInterval in application settings.");
			}
		}

		static int Main(string[] args)
		{
			if (args.Length == 0)
			{
				Help();
				return PROGRAM_SUCCESS;
			}

			string command = args[0].ToLower();
			if (PROGRAM_CMD_STATUS != command &&PROGRAM_CMD_REPORT != command)
			{
				Console.WriteLine("Unrecognized command {0}", command);
				return PROGRAM_ERROR;
			}

			CmdLine cmdline = new CmdLine(args);

			try
			{ 
				GetConfiguration(out string url, out string token, out int maxRetries, out int retryIntervalMS);
				
				if (cmdline.GetFlagValue(FLAG_BLACKDUCK_TOKEN, null) != null)
				{
					token = cmdline.GetFlagValue(FLAG_BLACKDUCK_TOKEN);
				}

				if (cmdline.GetFlagValue(FLAG_BLACKDUCK_URL, null) != null)
				{
					url = cmdline.GetFlagValue(FLAG_BLACKDUCK_URL);
				}

				string projectListFile = cmdline.GetFlagValue(FLAG_PROJECT_FILE, null);
				if (string.IsNullOrEmpty(projectListFile))
				{
					Console.WriteLine("Project list file not specified.");
					return PROGRAM_ERROR;
				}

				List<BDProject> projects = new BlackDuckProjectFile().GetProjectList(projectListFile);
			
				if (command == PROGRAM_CMD_STATUS)
				{
					string outputFile = cmdline.GetFlagValue(FLAG_OUTPUT_FILE, null);
					if (string.IsNullOrEmpty(outputFile))
					{
						Console.WriteLine("Report file not specified.");
						return PROGRAM_ERROR;
					}

					if (projects.Count == 0)
					{
						Console.WriteLine("No projects found!");
						return PROGRAM_SUCCESS;
					}

					BlackDuckReportGenerator reportGenerator = new BlackDuckReportGenerator(url, token);
					reportGenerator.GenerateStatusReport(projects, outputFile);
				}
				else if (command == PROGRAM_CMD_REPORT)
				{
					string outputDirectory = cmdline.GetFlagValue(FLAG_OUTPUT_FILE, null);
					if (string.IsNullOrEmpty(outputDirectory))
					{
						Console.WriteLine("Report directory not specified.");
						return PROGRAM_ERROR;
					}

					if (projects.Count == 0)
					{
						Console.WriteLine("No projects found!");
						return PROGRAM_SUCCESS;
					}

					BlackDuckReportGenerator reportGenerator = new BlackDuckReportGenerator(url, token);
					reportGenerator.GenerateVulnerabilityReport(projects, outputDirectory);
				}
			}
			catch(Exception e)
			{
				Console.WriteLine("Something went terribly wrong!");
				Console.WriteLine(e.Message);
				if (cmdline.IsFlagPresent(FLAG_DETAILED_ERRORS))
				{
					Console.Error.WriteLine(e);
				}
			}
			return PROGRAM_SUCCESS;
		}

		const string PROGRAM_CMD_STATUS = "status";
		const string PROGRAM_CMD_REPORT = "genrep";
		const int PROGRAM_ERROR = 1;
		const int PROGRAM_SUCCESS = 0;
		
		const string FLAG_PROJECT_FILE = "p";
		const string FLAG_OUTPUT_FILE = "o";
		const string FLAG_BLACKDUCK_URL = "u";
		const string FLAG_BLACKDUCK_TOKEN = "t";
		const string FLAG_DETAILED_ERRORS = "d";

	}
}
