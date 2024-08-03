﻿using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace BeautifulServerApplication
{
	public class SetupConfig
	{
		[ModuleInitializer]
		public static void Init()
		{
			Initialize();
		}

		public static void Initialize()
		{
			Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");

			var currentDirectory = Directory.GetCurrentDirectory();
			var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");

			try
			{
				if (!string.IsNullOrEmpty(currentDirectory))
				{
					new ConfigurationBuilder()
						.SetBasePath(currentDirectory)
						.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
						.AddJsonFile($"appsettings.{environment}.json", optional: true)
						.AddEnvironmentVariables()
						.Build();
				}
			}
			catch (ArgumentException argumentException)
			{
				Log.Error($"Wrong basePath: {currentDirectory}\n" +
				          $"[{argumentException.ParamName}]: {argumentException.Message}" +
				          " ||{SessionKey}||", "server_config");
			}
			catch (FileNotFoundException fileNotFoundException)
			{
				Log.Error($"File not found: {fileNotFoundException.FileName}\n" +
				          $"[{fileNotFoundException.GetType()}]: {fileNotFoundException.Message}" +
				          " ||{SessionKey}||", "server_config");
			}
			catch (Exception ex)
			{
				Log.Fatal("!!! Unexpected error\n" +
				          "Base path is null or empty. Cannot load configuration." +
				          $"{ex.Message}" +
				          " ||{SessionKey}||", "server_config");
			}
		}
	}
}