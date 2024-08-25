using BeautifulFundamental.Server.Session.Implementations;

namespace BeautifulFundamental.Server.Session.Contracts.Core
{
	public interface ISessionInfo
	{
		string Id { get; }
		string Username { get; }
		SessionState SessionState { get; }
		bool Authorized { get; }
	}
}