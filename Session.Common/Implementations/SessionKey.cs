using Core.Helpers;

namespace Session.Common.Implementations
{
	public interface ISessionKey
	{
		string SessionId { get; }
	}

	public class SessionKey : ISessionKey
	{
		public string SessionId { get; } = GuidIdCreator.CreateString();
	}
}