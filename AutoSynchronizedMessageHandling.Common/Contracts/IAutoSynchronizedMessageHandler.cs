using AutoSynchronizedMessageHandling.Common.Implementations;
using Remote.Communication.Common.Implementations;

namespace AutoSynchronizedMessageHandling.Common.Contracts
{
	public interface IAutoSynchronizedMessageHandler
	{
		/// <summary>
		/// Subscribing to a message type to automatically get informed by an (auto synchronized context) event.
		/// </summary>
		/// <typeparam name="TRequestMessage">The requested message type to receive</typeparam>
		/// <param name="replyMessageAction">The action to get informed.</param>
		/// <param name="autoSyncType">The synchronization context, default is the main application context</param>
		/// <returns>The id to later unsubscribe by id</returns>
		string Subscribe<TRequestMessage>(Func<IRequestMessage, IReplyMessage> replyMessageAction,
			AutoSyncType autoSyncType = AutoSyncType.Main) where TRequestMessage : IRequestMessage;

		bool Unsubscribe(string id);
	}
}