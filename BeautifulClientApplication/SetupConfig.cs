using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace BeautifulClientApplication
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
			var basePath = Directory.GetParent(currentDirectory)?.Parent?.Parent?.ToString();
			var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");

			try
			{
				if (!string.IsNullOrEmpty(basePath))
				{
					new ConfigurationBuilder()
						.SetBasePath(basePath)
						.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
						.AddJsonFile($"appsettings.{environment}.json", optional: true)
						.AddEnvironmentVariables()
						.Build();
				}
			}
			catch (ArgumentException argumentException)
			{
				Log.Error($"Wrong basePath: {basePath}\n" +
				          $"[{argumentException.ParamName}]: {argumentException.Message}" +
				          " ||{SessionKey}||", "client");
			}
			catch (FileNotFoundException fileNotFoundException)
			{
				Log.Error($"File not found: {fileNotFoundException.FileName}\n" +
				          $"[{fileNotFoundException.GetType()}]: {fileNotFoundException.Message}" +
				          " ||{SessionKey}||", "client");
			}
			catch (Exception ex)
			{
				Log.Fatal("!!! Unexpected error\n" +
				          "Base path is null or empty. Cannot load configuration." +
				          $"{ex.Message}" +
				          " ||{SessionKey}||", "client");
			}
		}
	}
}