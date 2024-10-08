﻿using System.Collections.Concurrent;
using BeautifulFundamental.Core.Communication;
using BeautifulFundamental.Core.Communication.Implementations;
using BeautifulFundamental.Core.Extensions;
using BeautifulFundamental.Core.Helpers;

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

	public class AutoSynchronizedMessageHandler : IAutoSynchronizedMessageHandler, IDisposable
	{
		private readonly ICommunicationService _communicationService;

		private readonly ConcurrentDictionary<string, AutoSynchronizedMessageContext>
			_autoSynchronizedMessageContexts = new();

		private readonly Dictionary<string, CancellationTokenSource> _publishingCtSources = new();

		private readonly SynchronizationContext _syncContext;
		private List<Task>? _publishingLoopTasks;


		public AutoSynchronizedMessageHandler(ICommunicationService communicationService)
		{
			_communicationService = communicationService;

			_syncContext = SynchronizationContext.Current ?? new SynchronizationContext();
		}

		public string Subscribe<TRequestMessage>(
			Func<INetworkMessage, INetworkMessage?> replyMessageAction,
			AutoSyncType autoSyncType = AutoSyncType.Main, SynchronizationContext? synchronizationContext = null)
			where TRequestMessage : INetworkMessage
		{
			if (replyMessageAction == null)
				throw new InvalidOperationException("Reply message action not set.");

			string typeDiscriminator = typeof(TRequestMessage).Name;

			if (synchronizationContext != null)
				autoSyncType = AutoSyncType.Custom;

			var newAutoSynchronizedMessageContext =
				AutoSynchronizedMessageContext.Create(typeDiscriminator, replyMessageAction, autoSyncType,
					synchronizationContext);

			var addingResult = TryAddAutoSyncContext(newAutoSynchronizedMessageContext);

			try
			{
				if (addingResult)
				{
					this.LogInfo($"Successfully registered {typeDiscriminator} to AutoSyncContext: {autoSyncType}\n" +
					             $"Start publishing loop.");
					StartPublishingLoop<TRequestMessage>(typeDiscriminator);
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

			var typeDiscriminator = autoSynchronizedMessageContext.TypeDiscriminator;

			if (_autoSynchronizedMessageContexts.Values.Any(context =>
				    context.TypeDiscriminator.Equals(typeDiscriminator)))
				return true;

			this.LogDebug($"No auto sync registrations for {typeDiscriminator}. Stopping publishing loop...");

			if (!_publishingCtSources.TryGetValue(typeDiscriminator, out var publishingCts))
			{
				this.LogError($"Cannot stop publishing loop, because did not find CTS for {typeDiscriminator}");
				return false;
			}

			publishingCts.Cancel();
			_publishingCtSources.Remove(typeDiscriminator);

			return true;
		}

		private bool TryAddAutoSyncContext(
			AutoSynchronizedMessageContext newAutoSynchronizedMessageContext)
		{
			return _autoSynchronizedMessageContexts.TryAdd(newAutoSynchronizedMessageContext.Id,
				newAutoSynchronizedMessageContext);
		}

		private void StartPublishingLoop<TRequestMessage>(string typeDiscriminator)
			where TRequestMessage : INetworkMessage
		{
			var publishingLoopCts = new CancellationTokenSource();

			if (!_publishingCtSources.TryAdd(typeDiscriminator, publishingLoopCts))
			{
				this.LogDebug($"Do not start publishing loop for {typeDiscriminator}, because already running...");
				return;
			}

			_publishingLoopTasks ??= new List<Task>();

			_publishingLoopTasks.Add(Task.Factory.StartNew(async () =>
			{
				while (!publishingLoopCts.Token.IsCancellationRequested)
				{
					var receivedRequestMessage =
						await _communicationService.ReceiveAsync<TRequestMessage>();

					var discriminator = receivedRequestMessage.GetType().Name;

					foreach (var autoSynchronizedMessageContext in _autoSynchronizedMessageContexts.Values.Where(
						         context => context.TypeDiscriminator.Equals(discriminator)))
					{
						switch (autoSynchronizedMessageContext.AutoSyncType)
						{
							case AutoSyncType.Main:
								PostExecuteReplyMessageAction(autoSynchronizedMessageContext, receivedRequestMessage,
									_syncContext);
								break;

							case AutoSyncType.This:
								ExecuteReplyMessageAction(autoSynchronizedMessageContext, receivedRequestMessage);
								break;

							case AutoSyncType.Custom:
								PostExecuteReplyMessageAction(autoSynchronizedMessageContext, receivedRequestMessage,
									autoSynchronizedMessageContext.SynchronizationContext);
								break;
						}
					}
				}
			}, publishingLoopCts.Token));
		}

		private void PostExecuteReplyMessageAction<TRequestMessage>(
			AutoSynchronizedMessageContext autoSynchronizedMessageContext,
			TRequestMessage receivedRequestMessage, SynchronizationContext? synchronizationContext)
			where TRequestMessage : INetworkMessage
		{
			synchronizationContext ??= _syncContext;

			synchronizationContext.Post(
				_ =>
				{
					ExecuteReplyMessageAction(autoSynchronizedMessageContext,
						receivedRequestMessage);
				}, null);
		}

		private void ExecuteReplyMessageAction<TRequestMessage>(
			AutoSynchronizedMessageContext autoSynchronizedMessageContext,
			TRequestMessage receivedRequestMessage) where TRequestMessage : INetworkMessage
		{
			var replyMessage = autoSynchronizedMessageContext.ReplyMessageAction(receivedRequestMessage);

			if (replyMessage == null)
				return;

			_communicationService.SendAsync(replyMessage);
		}


		public void Dispose()
		{
			foreach (var publishingCts in _publishingCtSources.Values)
			{
				publishingCts.Cancel();
			}

			if (_publishingLoopTasks == null)
				return;

			foreach (var publishingLoopTask in _publishingLoopTasks)
			{
				publishingLoopTask.Dispose();
			}
		}

		private class AutoSynchronizedMessageContext
		{
			private AutoSynchronizedMessageContext(string id, string typeDiscriminator,
				Func<INetworkMessage, INetworkMessage?> replyMessageAction, AutoSyncType autoSyncType,
				SynchronizationContext? synchronizationContext)
			{
				Id = id;
				AutoSyncType = autoSyncType;
				SynchronizationContext = synchronizationContext;
				TypeDiscriminator = typeDiscriminator;
				ReplyMessageAction = replyMessageAction;
			}


			public static AutoSynchronizedMessageContext Create(string typeDiscriminator,
				Func<INetworkMessage, INetworkMessage?> replyMessageAction, AutoSyncType autoSyncType,
				SynchronizationContext? synchronizationContext)
			{
				var guidId = GuidIdCreator.CreateString();
				return new AutoSynchronizedMessageContext(guidId, typeDiscriminator, replyMessageAction, autoSyncType,
					synchronizationContext);
			}

			public string Id { get; }
			public AutoSyncType AutoSyncType { get; }
			public SynchronizationContext? SynchronizationContext { get; }
			public Func<INetworkMessage, INetworkMessage?> ReplyMessageAction { get; }
			public string TypeDiscriminator { get; }
		}
	}
}