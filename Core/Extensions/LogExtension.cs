using System.Runtime.CompilerServices;
using Serilog.Events;

namespace Core.Extensions
{
	public static class LogExtension
	{
		public static void Log(this object anyObj, string logMessage, string sessionId = "",
			[CallerMemberName] string caller = "",
			[CallerFilePath] string file = "")
		{
			logMessage = PrepareLogMessage(logMessage, sessionId);
			WriteLog(logMessage, LogEventLevel.Debug, sessionId);
		}


		public static void LogInfo(this object anyObj, string logMessage, string sessionId = "",
			[CallerMemberName] string caller = "",
			[CallerFilePath] string file = "")
		{
			logMessage = PrepareLogMessage(logMessage, sessionId);
			WriteLog(logMessage, LogEventLevel.Information, sessionId);
		}

		public static void LogError(this object anyObj, string logMessage, string sessionId = "",
			[CallerMemberName] string caller = "",
			[CallerFilePath] string file = "")
		{
			logMessage = PrepareLogMessage(logMessage, sessionId);
			WriteLog(logMessage, LogEventLevel.Error, sessionId);
		}

		public static void LogWarning(this object anyObj, string logMessage, string sessionId = "",
			[CallerMemberName] string caller = "",
			[CallerFilePath] string file = "")
		{
			logMessage = PrepareLogMessage(logMessage, sessionId);
			WriteLog(logMessage, LogEventLevel.Warning, sessionId);
		}

		public static void LogFatal(this object anyObj, string logMessage, string sessionId = "",
			[CallerMemberName] string caller = "",
			[CallerFilePath] string file = "")
		{
			logMessage = PrepareLogMessage(logMessage, sessionId);
			WriteLog(logMessage, LogEventLevel.Fatal, sessionId);
		}

		public static void LogDebug(this object anyObj, string logMessage, string sessionId = "",
			[CallerMemberName] string caller = "",
			[CallerFilePath] string file = "")
		{
			logMessage = PrepareLogMessage(logMessage, sessionId);
			WriteLog(logMessage, LogEventLevel.Debug, sessionId);
		}

		public static void LogVerbose(this object anyObj, string logMessage, string sessionId = "",
			[CallerMemberName] string caller = "",
			[CallerFilePath] string file = "")
		{
			logMessage = PrepareLogMessage(logMessage, sessionId);
			WriteLog(logMessage, LogEventLevel.Verbose, sessionId);
		}

		private static string PrepareLogMessage(string logMessage, string sessionId)
		{
			if (!string.IsNullOrEmpty(sessionId))
			{
				logMessage += " ###{SessionId}";
			}

			return logMessage;
		}

		public static void Log(this object anyObj, string logMessage, string sessionId, LogEventLevel logType)
		{
			WriteLog(logMessage, logType, sessionId);
		}

		private static void WriteLog(string logMessage, LogEventLevel logType, string sessionId)
		{
			if (string.IsNullOrEmpty(logMessage))
				return;

			switch (logType)
			{
				case LogEventLevel.Debug:
					Serilog.Log.Debug(logMessage, sessionId);
					break;
				case LogEventLevel.Error:
					Serilog.Log.Error(logMessage, sessionId);
					break;
				case LogEventLevel.Fatal:
					Serilog.Log.Fatal(logMessage, sessionId);
					break;
				case LogEventLevel.Information:
					Serilog.Log.Information(logMessage, sessionId);
					break;
				case LogEventLevel.Verbose:
					Serilog.Log.Verbose(logMessage, sessionId);
					break;
				case LogEventLevel.Warning:
					Serilog.Log.Warning(logMessage, sessionId);
					break;
				default:
					Serilog.Log.Debug(logMessage, sessionId);
					break;
			}
		}
	}
}