namespace Session.Common.Contracts
{
	public interface ISession
	{
		event EventHandler<string>? SessionStopped;
		string Id { get; }

		void Start();

#if DEBUG
		void SendMessageToClient(object message);
#endif
	}
}