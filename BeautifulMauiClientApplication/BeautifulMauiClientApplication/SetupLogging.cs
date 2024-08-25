using BeautifulFundamental.Core.Constants;
using Serilog;
using System.Runtime.CompilerServices;

namespace BeautifulMauiClientApplication
{
	public class SetupLogging
	{
		[ModuleInitializer]
		public static void Init()
		{
			Initialize();
		}

		public static void Initialize()
		{
			var outputTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} | [{Level}] | {Message}{NewLine}{Exception}";

			Log.Logger = new LoggerConfiguration()
				.MinimumLevel.Verbose()
				.WriteTo.Map("SessionId", LoggerConstants.DefaultLoggingKey,
					(sessionId, wt) =>
					{
						var currentDomainBaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
						wt.File(Path.Combine(
								currentDomainBaseDirectory, "LogFiles",
								$"{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day}",
								$"Log_{sessionId}_.txt"),
							rollingInterval: RollingInterval.Day,
							outputTemplate:
							outputTemplate);
					})
				.CreateLogger();
		}
	}
}