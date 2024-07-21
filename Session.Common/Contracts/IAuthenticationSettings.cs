namespace Session.Common.Contracts
{
	public interface IAuthenticationSettings
	{
		int MaxAuthAttempts { get; }
		int AuthTimeoutInMinutes { get; }
	}
}