using System.Runtime.CompilerServices;
using Serilog;

namespace BeautifulServerApplication
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
				.WriteTo.Map("SessionId", "Misc",
					(sessionId, wt) => wt.File(Path.Combine(
							AppDomain.CurrentDomain.BaseDirectory, "LogFiles",
							$"{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day}", $"Log_{sessionId}_.txt"),
						rollingInterval: RollingInterval.Day,
						outputTemplate:
						outputTemplate))
				.WriteTo.Console(outputTemplate:
					outputTemplate)
				.CreateLogger();
		}
	}
}