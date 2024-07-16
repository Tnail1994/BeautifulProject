using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace BeautifulServerApplication;

public class SetupConfig
{
	[ModuleInitializer]
	public static void Init()
	{
		Initialize();
	}

	public static void Initialize()
	{
		var currentDirectory = Directory.GetCurrentDirectory();
		var basePath = Directory.GetParent(currentDirectory)?.Parent?.Parent?.ToString();

		try
		{
			if (!string.IsNullOrEmpty(basePath))
			{
				new ConfigurationBuilder()
					.SetBasePath(basePath)
					.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
					.AddEnvironmentVariables()
					.Build();
			}
		}
		catch (ArgumentException argumentException)
		{
			Log.Error($"Wrong basePath: {basePath}\n" +
			          $"[{argumentException.ParamName}]: {argumentException.Message}" +
			          " ||{SessionKey}||", "server");
		}
		catch (FileNotFoundException fileNotFoundException)
		{
			Log.Error($"File not found: {fileNotFoundException.FileName}\n" +
			          $"[{fileNotFoundException.GetType()}]: {fileNotFoundException.Message}" +
			          " ||{SessionKey}||", "server");
		}
		catch (Exception ex)
		{
			Log.Fatal("!!! Unexpected error\n" +
			          "Base path is null or empty. Cannot load configuration." +
			          $"{ex.Message}" +
			          " ||{SessionKey}||", "server");
		}
	}
}