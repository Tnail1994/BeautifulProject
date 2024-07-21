using Core.Helpers;
using Session.Common.Contracts;

namespace Session.Common.Implementations
{
	public interface ISessionKey
	{
		string SessionId { get; }
		void Update(ISessionInfo sessionInfo);
	}

	public class SessionKey : ISessionKey
	{
		public string SessionId { get; private set; } = GuidIdCreator.CreateString();

		public void Update(ISessionInfo sessionInfo)
		{
			SessionId = sessionInfo.Id;
		}
	}
}