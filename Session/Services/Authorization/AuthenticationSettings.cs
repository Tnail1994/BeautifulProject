using Session.Common.Contracts;

namespace Session.Services.Authorization
{
	public class AuthenticationSettings : IAuthenticationSettings
	{
		private const int DefaultMaxAuthAttempts = 3;
		private const int DefaultAuthTimeoutInMinutes = 1;

		public int MaxAuthAttempts { get; init; } = DefaultMaxAuthAttempts;
		public int AuthTimeoutInMinutes { get; init; } = DefaultAuthTimeoutInMinutes;

		public static AuthenticationSettings Default => new()
		{
			MaxAuthAttempts = DefaultMaxAuthAttempts,
			AuthTimeoutInMinutes = DefaultAuthTimeoutInMinutes
		};
	}
}