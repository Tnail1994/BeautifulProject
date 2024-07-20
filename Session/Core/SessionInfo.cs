using Session.Common.Contracts;
using Session.Common.Implementations;
using SharedBeautifulData.Contracts;

namespace Session.Core
{
	internal class SessionInfo : ISessionInfo, IEntity
	{
		private SessionInfo(string id, string username, SessionState sessionState)
		{
			Id = id;
			Username = username;
			SessionState = sessionState;
		}

		public static SessionInfo Create(string id, string username,
			SessionState sessionState = SessionState.Starting) =>
			new(id, username, sessionState);

		public string Id { get; }
		public string Username { get; private set; }
		public SessionState SessionState { get; private set; }
		public bool Authorized { get; private set; }

		public void SetState(SessionState sessionState)
		{
			SessionState = sessionState;
		}

		public void SetUsername(string username)
		{
			Username = username;
		}

		public void SetAuthorized(bool authorized)
		{
			Authorized = authorized;
		}
	}
}