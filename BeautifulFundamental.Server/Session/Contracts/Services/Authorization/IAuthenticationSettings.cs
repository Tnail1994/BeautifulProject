namespace BeautifulFundamental.Server.Session.Contracts.Services.Authorization
{
	public interface IAuthenticationSettings
	{
		int MaxAuthAttempts { get; }
		int AuthTimeoutInMinutes { get; }
		int MaxReactivateAuthenticationCounter { get; }
	}
}