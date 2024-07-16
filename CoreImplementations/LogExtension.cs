using System.Runtime.CompilerServices;
using Serilog.Events;

namespace CoreImplementations
{
	public static class LogExtension
	{
		public static void Log(this object anyObj, string logMessage, [CallerMemberName] string caller = "",
			[CallerFilePath] string file = "")
		{
			logMessage = PrepareLogMessage(logMessage, caller, file);
			WriteLog(logMessage, LogEventLevel.Debug);
		}

		private static string PrepareLogMessage(string logMessage, string caller, string file)
		{
			if (!string.IsNullOrEmpty(caller) && !string.IsNullOrEmpty(file))
			{
				logMessage = $"[{Path.GetFileNameWithoutExtension(file)}, {caller}] {logMessage}";
			}
			else if (!string.IsNullOrEmpty(caller))
			{
				logMessage = $"[{caller}] {logMessage}";
			}
			else if (!string.IsNullOrEmpty(file))
			{
				logMessage = $"-{Path.GetFileNameWithoutExtension(file)}- {logMessage}";
			}

			return logMessage;
		}

		public static void LogInfo(this object anyObj, string logMessage, [CallerMemberName] string caller = "",
			[CallerFilePath] string file = "")
		{
			logMessage = PrepareLogMessage(logMessage, caller, file);
			WriteLog(logMessage, LogEventLevel.Information);
		}

		public static void LogError(this object anyObj, string logMessage, [CallerMemberName] string caller = "",
			[CallerFilePath] string file = "")
		{
			logMessage = PrepareLogMessage(logMessage, caller, file);
			WriteLog(logMessage, LogEventLevel.Error);
		}

		public static void LogWarning(this object anyObj, string logMessage, [CallerMemberName] string caller = "",
			[CallerFilePath] string file = "")
		{
			logMessage = PrepareLogMessage(logMessage, caller, file);
			WriteLog(logMessage, LogEventLevel.Warning);
		}

		public static void LogFatal(this object anyObj, string logMessage, [CallerMemberName] string caller = "",
			[CallerFilePath] string file = "")
		{
			logMessage = PrepareLogMessage(logMessage, caller, file);
			WriteLog(logMessage, LogEventLevel.Fatal);
		}

		public static void LogDebug(this object anyObj, string logMessage, [CallerMemberName] string caller = "",
			[CallerFilePath] string file = "")
		{
			logMessage = PrepareLogMessage(logMessage, caller, file);
			WriteLog(logMessage, LogEventLevel.Debug);
		}

		public static void LogVerbose(this object anyObj, string logMessage, [CallerMemberName] string caller = "",
			[CallerFilePath] string file = "")
		{
			logMessage = PrepareLogMessage(logMessage, caller, file);
			WriteLog(logMessage, LogEventLevel.Verbose);
		}

		public static void Log(this object anyObj, string logMessage, LogEventLevel logType)
		{
			WriteLog(logMessage, logType);
		}

		private static void WriteLog(string logMessage, LogEventLevel logType)
		{
			if (string.IsNullOrEmpty(logMessage))
				return;

			switch (logType)
			{
				case LogEventLevel.Debug:
					Serilog.Log.Debug(logMessage);
					break;
				case LogEventLevel.Error:
					Serilog.Log.Error(logMessage);
					break;
				case LogEventLevel.Fatal:
					Serilog.Log.Fatal(logMessage);
					break;
				case LogEventLevel.Information:
					Serilog.Log.Information(logMessage);
					break;
				case LogEventLevel.Verbose:
					Serilog.Log.Verbose(logMessage);
					break;
				case LogEventLevel.Warning:
					Serilog.Log.Warning(logMessage);
					break;
				default:
					Serilog.Log.Debug(logMessage);
					break;
			}
		}
	}
}