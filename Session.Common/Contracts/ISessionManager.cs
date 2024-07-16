using Microsoft.Extensions.Hosting;

namespace Session.Common.Contracts
{
	public interface ISessionManager : IHostedService
	{
#if DEBUG
		void SendMessageToRandomClient(object messageObject);
		void SendMessageToAllClients(object messageObject);
#endif
	}
}