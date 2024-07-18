using Remote.Communication.Common.Implementations;

namespace Remote.Communication.Common.Contracts
{
	public interface ICommunicationService
	{
		event EventHandler<string>? ConnectionLost;
		void Start();
		Task<T> ReceiveAsync<T>() where T : IBaseMessage;
		Task<T> ReceiveAsync<T>(CancellationToken cancellationToken) where T : IBaseMessage;
		void SendAsync(object messageObj);
	}
}