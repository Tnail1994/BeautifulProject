using Session.Common.Contracts.Services.Authorization;

namespace Session.Services.Authorization
{
    public class AuthenticationSettings : IAuthenticationSettings
	{
		private const int DefaultMaxAuthAttempts = 3;
		private const int DefaultAuthTimeoutInMinutes = 1;
		private const int DefaultMaxReactivateAuthenticationCounter = 20;

		public int MaxAuthAttempts { get; init; } = DefaultMaxAuthAttempts;
		public int AuthTimeoutInMinutes { get; init; } = DefaultAuthTimeoutInMinutes;
		public int MaxReactivateAuthenticationCounter { get; init; } = DefaultMaxReactivateAuthenticationCounter;

		public static AuthenticationSettings Default => new()
		{
			MaxAuthAttempts = DefaultMaxAuthAttempts,
			AuthTimeoutInMinutes = DefaultAuthTimeoutInMinutes,
			MaxReactivateAuthenticationCounter = DefaultMaxReactivateAuthenticationCounter
		};
	}
}