namespace Session.Common.Implementations
{
	public class SessionStoppedEventArgs : EventArgs
	{
		public ISessionKey SessionKey { get; }
		public string Reason { get; }

		private SessionStoppedEventArgs(ISessionKey sessionKey, string reason)
		{
			SessionKey = sessionKey;
			Reason = reason;
		}

		public static SessionStoppedEventArgs Create(ISessionKey sessionKey, string reason)
		{
			return new SessionStoppedEventArgs(sessionKey, reason);
		}
	}
}