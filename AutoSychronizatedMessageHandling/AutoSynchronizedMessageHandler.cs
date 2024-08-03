using AutoSynchronizedMessageHandling.Common.Contracts;
using AutoSynchronizedMessageHandling.Common.Implementations;
using Core.Extensions;
using Core.Helpers;
using Remote.Communication.Common.Contracts;
using Remote.Communication.Common.Implementations;
using System.Collections.Concurrent;

namespace AutoSynchronizedMessageHandling
{
	public class AutoSynchronizedMessageHandler : IAutoSynchronizedMessageHandler, IDisposable
	{
		private readonly ICommunicationService _communicationService;

		private readonly ConcurrentDictionary<string, AutoSynchronizedMessageContext>
			_autoSynchronizedMessageContexts = new();

		private readonly SynchronizationContext _syncContext;


		public AutoSynchronizedMessageHandler(ICommunicationService communicationService)
		{
			_communicationService = communicationService;

			_syncContext = SynchronizationContext.Current ?? new SynchronizationContext();
		}

		public string Subscribe<TRequestMessage>(
			Func<IRequestMessage, IReplyMessage> replyMessageAction,
			AutoSyncType autoSyncType = AutoSyncType.Main) where TRequestMessage : IRequestMessage
		{
			if (replyMessageAction == null)
				throw new InvalidOperationException("Reply message action not set.");

			string typeDiscriminator = typeof(TRequestMessage).Name;
			var publishingLoopCts = new CancellationTokenSource();

			var newAutoSynchronizedMessageContext = AutoSynchronizedMessageContext.Create(typeDiscriminator,
				replyMessageAction, autoSyncType, publishingLoopCts);

			var addingResult = TryAddAutoSyncContext(newAutoSynchronizedMessageContext);

			try
			{
				if (addingResult)
				{
					this.LogInfo($"Successfully registered {typeDiscriminator} to AutoSyncContext: {autoSyncType}\n" +
					             $"Start publishing loop.");
					StartPublishingLoop<TRequestMessage>(publishingLoopCts);
				}
				else
				{
					this.LogError($"Not registered {typeDiscriminator} to AutoSyncContext: {autoSyncType}\n" +
					              $"Cannot start publishing loop");
				}
			}
			catch (NullReferenceException ex)
			{
				this.LogWarning($"[AutoSynchronizedMessageHandler]: {ex.Message}");
				Unsubscribe(newAutoSynchronizedMessageContext.Id);
			}
			catch (OperationCanceledException)
			{
				this.LogDebug($"Publishing loop {typeDiscriminator} cancelled.");
				Unsubscribe(newAutoSynchronizedMessageContext.Id);
			}
			catch (Exception ex)
			{
				this.LogFatal($"Unexpected in subscribing or publishing loop {typeDiscriminator}" +
				              $"{ex.Message}" +
				              $"Stacktrace: {ex.StackTrace}.");
			}

			return newAutoSynchronizedMessageContext.Id;
		}

		public bool Unsubscribe(string id)
		{
			if (!_autoSynchronizedMessageContexts.TryRemove(id, out var autoSynchronizedMessageContext))
				return false;

			autoSynchronizedMessageContext.PublishingLoopCts.Cancel();
			return true;
		}

		private bool TryAddAutoSyncContext(
			AutoSynchronizedMessageContext newAutoSynchronizedMessageContext)
		{
			return _autoSynchronizedMessageContexts.TryAdd(newAutoSynchronizedMessageContext.Id,
				newAutoSynchronizedMessageContext);
		}

		private void StartPublishingLoop<TRequestMessage>(CancellationTokenSource publishingLoopCts)
			where TRequestMessage : IRequestMessage
		{
			Task.Factory.StartNew(async () =>
			{
				while (!publishingLoopCts.Token.IsCancellationRequested)
				{
					var receivedRequestMessage =
						await _communicationService.ReceiveAsync<TRequestMessage>();

					var discriminator = receivedRequestMessage.GetType().Name;

					foreach (var autoSynchronizedMessageContext in _autoSynchronizedMessageContexts.Values.Where(
						         context => context.TypeDiscriminator.Equals(discriminator)))
					{
						if (autoSynchronizedMessageContext.AutoSyncType.Equals(AutoSyncType.Main))
							_syncContext.Post(
								_ =>
								{
									ExecuteReplyMessageAction(autoSynchronizedMessageContext, receivedRequestMessage);
								}, null);
						else
						{
							ExecuteReplyMessageAction(autoSynchronizedMessageContext, receivedRequestMessage);
						}
					}
				}
			}, publishingLoopCts.Token);
		}

		private void ExecuteReplyMessageAction<TRequestMessage>(
			AutoSynchronizedMessageContext autoSynchronizedMessageContext,
			TRequestMessage receivedRequestMessage) where TRequestMessage : IRequestMessage
		{
			var replyMessage = autoSynchronizedMessageContext.ReplyMessageAction(receivedRequestMessage);
			_communicationService.SendAsync(replyMessage);
		}


		public void Dispose()
		{
			foreach (var autoSynchronizedMessageContext in _autoSynchronizedMessageContexts.Values)
			{
				autoSynchronizedMessageContext.PublishingLoopCts.Cancel();
			}
		}

		private class AutoSynchronizedMessageContext
		{
			private AutoSynchronizedMessageContext(string id, string typeDiscriminator,
				Func<IRequestMessage, IReplyMessage> replyMessageAction, AutoSyncType autoSyncType,
				CancellationTokenSource publishingLoopCts)
			{
				Id = id;
				AutoSyncType = autoSyncType;
				TypeDiscriminator = typeDiscriminator;
				ReplyMessageAction = replyMessageAction;
				PublishingLoopCts = publishingLoopCts;
			}

			public CancellationTokenSource PublishingLoopCts { get; set; }

			public static AutoSynchronizedMessageContext Create(string typeDiscriminator,
				Func<IRequestMessage, IReplyMessage> replyMessageAction, AutoSyncType autoSyncType,
				CancellationTokenSource publishingLoopCts)
			{
				var guidId = GuidIdCreator.CreateString();
				return new AutoSynchronizedMessageContext(guidId, typeDiscriminator, replyMessageAction, autoSyncType,
					publishingLoopCts);
			}

			public string Id { get; }
			public AutoSyncType AutoSyncType { get; }
			public Func<IRequestMessage, IReplyMessage> ReplyMessageAction { get; }
			public string TypeDiscriminator { get; }
		}
	}
}