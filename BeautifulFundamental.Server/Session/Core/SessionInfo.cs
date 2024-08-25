using BeautifulFundamental.Core;
using BeautifulFundamental.Server.Session.Contracts.Core;
using BeautifulFundamental.Server.Session.Implementations;

namespace BeautifulFundamental.Server.Session.Core
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

		public static SessionInfo Empty => new(string.Empty, string.Empty, SessionState.Empty);

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