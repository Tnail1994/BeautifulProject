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

		Task<TReplyMessageType> SendAndReceiveAsync<TReplyMessageType>(object messageToSend)
			where TReplyMessageType : IBaseMessage;

		/// <summary>
		/// /// Use this, if you do not need info of the awaited message for the sending message
		/// </summary>
		/// <typeparam name="TAwaitMessageType">The message type waiting for</typeparam>
		/// <param name="messageToSend">The object to send</param>
		/// <returns></returns>
		Task<TAwaitMessageType> ReceiveAndSendAsync<TAwaitMessageType>(object messageToSend)
			where TAwaitMessageType : IBaseMessage;

		void Stop();
	}
}