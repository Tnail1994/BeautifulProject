using Session.Common.Implementations;

namespace Session.Common.Contracts
{
	public interface ISessionInfo
	{
		string Id { get; }
		string Username { get; }
		SessionState SessionState { get; }
		bool Authorized { get; }
	}
}