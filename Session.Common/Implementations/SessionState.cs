namespace Session.Common.Implementations
{
	public enum SessionState
	{
		Starting = 0,
		Authorizing = 1,
		Running = 2,
		Stopped = 3, // When a session can be restored
		Down = 4, // Unrecoverable session
		FailedAuthorization = 5,
		Empty = 6
	}
}