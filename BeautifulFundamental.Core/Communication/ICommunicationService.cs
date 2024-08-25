using BeautifulFundamental.Core.Communication.Implementations;

namespace BeautifulFundamental.Core.Communication
{
	public interface ICommunicationService
	{
		event EventHandler<string>? ConnectionLost;
		void Start();

		Task<T> ReceiveAsync<T>() where T : INetworkMessage;
		void SendAsync(object messageObj);

		Task<TReplyMessageType> SendAndReceiveAsync<TReplyMessageType>(object messageObj)
			where TReplyMessageType : INetworkMessage;

		/// <summary>
		/// /// Use this, if you do not need info of the awaited message for the sending message
		/// </summary>
		/// <typeparam name="TAwaitMessageType">The message type waiting for</typeparam>
		/// <param name="messageObj">The object to send</param>
		/// <returns></returns>
		Task<TAwaitMessageType> ReceiveAndSendAsync<TAwaitMessageType>(object messageObj)
			where TAwaitMessageType : INetworkMessage;

		void Stop(bool force = false);
	}
}