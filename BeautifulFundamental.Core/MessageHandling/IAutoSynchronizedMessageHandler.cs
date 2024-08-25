using BeautifulFundamental.Core.Communication.Implementations;

namespace BeautifulFundamental.Core.MessageHandling
{
	public interface IAutoSynchronizedMessageHandler
	{
		/// <summary>
		/// Subscribing to a message type to automatically get informed by an (auto synchronized context) event.
		/// Hint: If you set autoSyncType to Custom and do not set the synchronizationContext, then the main synchronization context will be selected automatically.
		/// </summary>
		/// <typeparam name="TRequestMessage">The requested message type to receive</typeparam>
		/// <param name="replyMessageAction">The action to get informed. When return null for IReplyMessage, then this meas no reply should be sent.</param>
		/// <param name="autoSyncType">The synchronization context, default is the main application context</param>
		/// <param name="synchronizationContext">Set to you custom syncContext. If this is not null, autoSyncType is automatically set to Custom</param>
		/// <returns>The id to later unsubscribe by id</returns>
		string Subscribe<TRequestMessage>(Func<INetworkMessage, INetworkMessage?> replyMessageAction,
			AutoSyncType autoSyncType = AutoSyncType.Main, SynchronizationContext? synchronizationContext = null)
			where TRequestMessage : INetworkMessage;

		bool Unsubscribe(string id);
	}
}