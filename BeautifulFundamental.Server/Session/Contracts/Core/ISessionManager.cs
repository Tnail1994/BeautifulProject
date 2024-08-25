namespace BeautifulFundamental.Server.Session.Contracts.Core
{
	public interface ISessionManager
	{
#if DEBUG
		void SendMessageToRandomClient(object messageObject);
		void SendMessageToAllClients(object messageObject);
#endif
	}
}