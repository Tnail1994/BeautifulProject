using System.Collections.Concurrent;
#if DEBUG
using System.Diagnostics;
#endif
using Core.Extensions;
using Newtonsoft.Json;
using Remote.Communication.Common.Client.Contracts;
using Remote.Communication.Common.Contracts;
using Remote.Communication.Common.Implementations;
using Remote.Communication.Common.Transformation.Contracts;
using Remote.Communication.Common.Transformation.Implementations;
using Remote.Communication.Transformation;
using Session.Common.Implementations;

namespace Remote.Communication
{
	public class CommunicationService : ICommunicationService, IDisposable
	{
		private readonly ConcurrentDictionary<string, TransformedObject> _transformedObjects = new();
		private readonly ConcurrentDictionary<string, TransformedObjectWaiter> _transformedObjectWaiters = new();

		private readonly IAsyncClient _asyncClient;

		private readonly ITransformerService _transformerService;
		private readonly ISessionKey _sessionKey;
		private readonly CancellationTokenSource _ownCancellationReceivingTokenSource = new();
		private bool _running;

#if DEBUG
		private readonly Stopwatch _stopwatch = new();
#endif

		public CommunicationService(IAsyncClient asyncClient, ITransformerService transformerService,
			ISessionKey sessionKey)
		{
			_asyncClient = asyncClient;
			_transformerService = transformerService;
			_sessionKey = sessionKey;
		}


		public event EventHandler<string>? ConnectionLost;

		private string SessionId => _sessionKey.SessionId;


		public void Start()
		{
			if (_running)
			{
				this.LogVerbose("CommunicationService already running", SessionId);
				return;
			}

			_asyncClient.MessageReceived += OnMessageReceived;
			_asyncClient.ConnectionLost += ConnectionLost;
			_asyncClient.StartReceivingAsync();

			_running = true;
		}

		public Task<T> ReceiveAsync<T>() where T : IBaseMessage
		{
			return ReceiveAsync<T>(_ownCancellationReceivingTokenSource.Token);
		}

		public Task<T> ReceiveAsync<T>(CancellationToken cancellationToken) where T : IBaseMessage
		{
			var transformedObject = _transformedObjects.Values.FirstOrDefault(x => x.Object is T);

			if (transformedObject == null)
				return WaitForReceive<T>();

			var discriminator = GetDiscriminator<T>();
			TryRemoveTransformedObject(discriminator, transformedObject);
			return Task.FromResult((T)transformedObject.Object);
		}

		private void TryRemoveTransformedObject(string discriminator, TransformedObject transformedObject)
		{
			if (_transformedObjectWaiters.Values.Any(waiter => waiter.Discriminator == discriminator))
				return;

			var removeRes = _transformedObjects.TryRemove(transformedObject.Id, out _);
			this.LogDebug($"Removed item = {discriminator}, was successful {removeRes}", SessionId);
		}

		public void SendAsync(object messageObj)
		{
			var jsonString = JsonConvert.SerializeObject(messageObj, JsonConfig.Settings);
			this.LogDebug($"Sending {jsonString} to client.", SessionId);

			if (_asyncClient.IsNotConnected)
				throw new CommunicationServiceException("AsyncClient is not connected", 0);

			_asyncClient.Send(jsonString);
		}

		public Task<TReplyMessageType> SendAndReceiveAsync<TReplyMessageType>(object messageToSend)
			where TReplyMessageType : IBaseMessage
		{
			SendAsync(messageToSend);
			return ReceiveAsync<TReplyMessageType>();
		}


		public async Task<TAwaitMessageType>
			ReceiveAndSendAsync<TAwaitMessageType>(object messageToSend)
			where TAwaitMessageType : IBaseMessage
		{
			var awaitMessage = await ReceiveAsync<TAwaitMessageType>();
			SendAsync(messageToSend);
			return awaitMessage;
		}

		public void Stop()
		{
			if (!_running)
			{
				this.LogVerbose("CommunicationService not running", SessionId);
				return;
			}

			_asyncClient.MessageReceived -= OnMessageReceived;
			_asyncClient.ConnectionLost -= ConnectionLost;

			_running = false;
		}


		private async Task<T> WaitForReceive<T>() where T : IBaseMessage
		{
			var transformedObjectWaiter = TransformedObjectWaiter.Create(GetDiscriminator<T>());
			_transformedObjectWaiters.TryAdd(transformedObjectWaiter.Id, transformedObjectWaiter);

			var transformedObject = await transformedObjectWaiter.TaskCompletionSource.Task;

			_transformedObjectWaiters.TryRemove(transformedObjectWaiter.Id, out transformedObjectWaiter);

			TryRemoveTransformedObject(transformedObject.Discriminator, transformedObject);

			return (T)transformedObject.Object;
		}

		private static string GetDiscriminator<T>() where T : IBaseMessage
		{
			return typeof(T).Name;
		}

		private void OnMessageReceived(string jsonString)
		{
			try
			{
				this.LogDebug($"OnMessageReceived with {jsonString}", SessionId);

#if DEBUG
				this.LogDebug("Start transforming with Service...", SessionId);
				_stopwatch.Restart();

#endif

				var transformedObject = _transformerService.Transform(jsonString);

#if DEBUG
				_stopwatch.Stop();
				this.LogDebug($"Transforming took {_stopwatch.ElapsedMilliseconds}ms", SessionId);
#endif
				AddTransformedObject(transformedObject);
				this.LogDebug($"Finished and added: {transformedObject}", SessionId);

				var transformedObjectWaiters =
					_transformedObjectWaiters.Values.Where(x => x.Discriminator == transformedObject.Discriminator)
						.ToList();

				foreach (var transformedObjectWaiter in transformedObjectWaiters)
				{
					transformedObjectWaiter.TaskCompletionSource.SetResult(transformedObject);

					if (transformedObjectWaiter.IsPermanent)
						continue;

					_transformedObjectWaiters.TryRemove(transformedObjectWaiter.Id, out _);
				}
			}
			catch (JsonReaderException jsonReaderException)
			{
				this.LogError($"Json reader error for jsonString: {jsonString}.\n" +
				              $"{jsonReaderException.Message}", SessionId);

				// Todo check, why jsonString is not a valid json format
			}
			catch (JsonException ex)
			{
				this.LogError($"Json error for jsonString: {jsonString}.\n" +
				              $"{ex.Message}", SessionId);
			}
			catch (TransformException ex)
			{
				switch (ex.ErrorCode)
				{
					// Todo, depending on case do error handling

					case 1:
					case 2:
					case 3:
						this.LogError($"Transform error for jsonString: {jsonString}.\n" +
						              $"{ex.Message}", SessionId);
						break;
				}
			}
			catch (Exception ex)
			{
				this.LogFatal($"!!! Unexpected error while OnMessageReceived event\n" +
				              $"Message: {ex.Message}\n" +
				              $"Stacktrace: {ex.StackTrace}\n", SessionId);
			}
		}

		private void AddTransformedObject(TransformedObject transformedObject)
		{
			_transformedObjects.TryAdd(transformedObject.Id, transformedObject);
		}

		public void Dispose()
		{
			Stop();

			foreach (var waiter in _transformedObjectWaiters)
			{
				waiter.Value.TaskCompletionSource.SetCanceled();
			}

			_transformedObjectWaiters.Clear();
			_transformedObjects.Clear();
			_ownCancellationReceivingTokenSource.Dispose();
		}
	}
}