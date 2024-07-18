using Remote.Communication.Common.Implementations;

namespace Remote.Communication.Common.Contracts
{
	public interface ICommunicationService : IDisposable
	{
		event EventHandler<string>? ConnectionLost;
		Task Start();
		Task<T> ReceiveAsync<T>() where T : IBaseMessage;
		Task<T> ReceiveAsync<T>(CancellationToken cancellationToken) where T : IBaseMessage;
		void SendAsync(object messageObj);
	}
}