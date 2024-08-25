namespace BeautifulFundamental.Server.Session.Implementations
{
	public enum SessionState
	{
		Starting = 0,
		Authorizing = 1,
		Running = 2,
		Stopped = 3, // When a session can be restored
		FailedAuthorization = 5,
		Empty = 6,
		Connecting = 7,
		None = 8,
	}
}