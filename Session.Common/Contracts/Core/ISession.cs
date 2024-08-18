using Session.Common.Implementations;

namespace Session.Common.Contracts.Core
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