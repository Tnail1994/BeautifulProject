using Session.Common.Implementations;

namespace Session.Common.Contracts
{
	public interface ISession
	{
		event EventHandler<SessionStoppedEventArgs>? SessionStopped;
		string Id { get; }

		void Start();

#if DEBUG
		void SendMessageToClient(object message);
#endif
	}
}