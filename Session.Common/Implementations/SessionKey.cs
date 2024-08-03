using Core.Helpers;
using Session.Common.Contracts;

namespace Session.Common.Implementations
{
	public interface ISessionKey
	{
		string SessionId { get; }
		string InstantiatedSessionId { get; }
	}

	public class SessionKey : ISessionKey
	{
		public SessionKey(ISessionKeySettings sessionKeySettings)
		{
			SessionId = sessionKeySettings.GenerateId ? GuidIdCreator.CreateString() : string.Empty;
			InstantiatedSessionId = SessionId;
		}

		public string SessionId { get; private set; }
		public string InstantiatedSessionId { get; }
	}
}