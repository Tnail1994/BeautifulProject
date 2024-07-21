using Core.Helpers;
using Session.Common.Contracts;

namespace Session.Common.Implementations
{
	public interface ISessionKey
	{
		string SessionId { get; }
		string InstantiatedSessionId { get; }
		void Update(ISessionInfo sessionInfo);
	}

	public class SessionKey : ISessionKey
	{
		public SessionKey()
		{
			SessionId = GuidIdCreator.CreateString();
			InstantiatedSessionId = SessionId;
		}

		public string SessionId { get; private set; }
		public string InstantiatedSessionId { get; }

		public void Update(ISessionInfo sessionInfo)
		{
			SessionId = sessionInfo.Id;
		}
	}
}