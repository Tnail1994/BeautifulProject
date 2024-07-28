namespace Session.Common.Contracts
{
	public interface ISessionManager
	{
#if DEBUG
		void SendMessageToRandomClient(object messageObject);
		void SendMessageToAllClients(object messageObject);
#endif
	}
}