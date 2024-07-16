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
			Log.Logger = new LoggerConfiguration()
				.MinimumLevel.Verbose()
				.WriteTo.Map("SessionId", "Misc",
					(sessionId, wt) => wt.File(Path.Combine(
							AppDomain.CurrentDomain.BaseDirectory, "LogFiles",
							$"{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day}", $"Log-{sessionId}.txt"),
						rollingInterval: RollingInterval.Day,
						outputTemplate:
						"[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} | [{Level}] | {Message}{NewLine}{Exception}"))
				.WriteTo.Console(outputTemplate:
					"[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} | [{Level}] | {Message}{NewLine}{Exception}")
				.CreateLogger();
		}
	}
}