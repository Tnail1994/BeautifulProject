using BeautifulFundamental.Core.Communication.Client;
using BeautifulFundamental.Core.Communication.Implementations;
using BeautifulFundamental.Core.Communication.Transformation;
using BeautifulFundamental.Core.Communication.Transformation.Implementations;
using BeautifulFundamental.Core.Extensions;
using BeautifulFundamental.Core.Identification;
using Newtonsoft.Json;
using System.Collections.Concurrent;
#if DEBUG
using System.Diagnostics;
#endif

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

	public class CommunicationService : ICommunicationService, IDisposable
	{
		private readonly ConcurrentDictionary<string, TransformedObject> _transformedObjects = new();
		private readonly ConcurrentDictionary<string, TransformedObjectWaiter> _transformedObjectWaiters = new();

		private readonly IAsyncClient _asyncClient;
		private readonly ITransformerService _transformerService;
		private readonly IIdentificationKey _identificationKey;

		private bool _running;

#if DEBUG
		private readonly Stopwatch _stopwatch = new();
#endif

		public CommunicationService(IAsyncClient asyncClient, ITransformerService transformerService,
			IIdentificationKey identificationKey)
		{
			_asyncClient = asyncClient;
			_transformerService = transformerService;
			_identificationKey = identificationKey;
		}


		public event EventHandler<string>? ConnectionLost;

		private string SessionId => _identificationKey.SessionId;


		public void Start()
		{
			if (_running)
			{
				this.LogVerbose("CommunicationService already running", SessionId);
				return;
			}

			_asyncClient.MessageReceived += OnMessageReceived;
			_asyncClient.ConnectionLost += OnConnectionLost;
			_asyncClient.StartReceivingAsync();

			_running = true;
		}

		private void OnConnectionLost(object? sender, string e)
		{
			ConnectionLost?.Invoke(sender, e);
		}

		public Task<T> ReceiveAsync<T>() where T : INetworkMessage
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
			if (_transformedObjectWaiters.Values.Any(waiter => waiter.Discriminator.Equals(discriminator)))
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

		public Task<TReplyMessageType> SendAndReceiveAsync<TReplyMessageType>(object messageObj)
			where TReplyMessageType : INetworkMessage
		{
			SendAsync(messageObj);
			return ReceiveAsync<TReplyMessageType>();
		}


		public async Task<TAwaitMessageType>
			ReceiveAndSendAsync<TAwaitMessageType>(object messageObj)
			where TAwaitMessageType : INetworkMessage
		{
			var awaitMessage = await ReceiveAsync<TAwaitMessageType>();
			SendAsync(messageObj);
			return awaitMessage;
		}

		public void Stop(bool force = false)
		{
			if (!_running)
			{
				this.LogVerbose("CommunicationService not running", SessionId);
				return;
			}

			_running = false;

			_asyncClient.MessageReceived -= OnMessageReceived;
			_asyncClient.ConnectionLost -= ConnectionLost;


			if (force)
			{
				_asyncClient.Disconnect();

				foreach (var waiter in _transformedObjectWaiters)
				{
					waiter.Value.TaskCompletionSource.SetCanceled();
				}

				_transformedObjectWaiters.Clear();
				_transformedObjects.Clear();
			}
		}


		private async Task<T> WaitForReceive<T>() where T : INetworkMessage
		{
			var transformedObjectWaiter = TransformedObjectWaiter.Create(GetDiscriminator<T>());
			_transformedObjectWaiters.TryAdd(transformedObjectWaiter.Id, transformedObjectWaiter);

			var transformedObject = await transformedObjectWaiter.TaskCompletionSource.Task;

			_transformedObjectWaiters.TryRemove(transformedObjectWaiter.Id, out _);

			TryRemoveTransformedObject(transformedObject.Discriminator, transformedObject);

			return (T)transformedObject.Object;
		}

		private static string GetDiscriminator<T>() where T : INetworkMessage
		{
			return typeof(T).Name;
		}

		private void OnMessageReceived(string jsonString)
		{
			try
			{
				this.LogDebug($"OnMessageReceived with \n" +
				              $"-----------------------\n" +
				              $"{jsonString}\n" +
				              $"-----------------------\n", SessionId);

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

				foreach (var transformedObjectWaiter in FindTransformedObjectWaiters(transformedObject))
				{
					transformedObjectWaiter.TaskCompletionSource.SetResult(transformedObject);
					_transformedObjectWaiters.TryRemove(transformedObjectWaiter.Id, out _);
				}
			}
			catch (JsonReaderException jsonReaderException)
			{
				this.LogWarning($"Json reader error for jsonString: {jsonString}.\n" +
				                $"{jsonReaderException.Message}", SessionId);
				this.LogWarning($"Try to handle and search for pattern ...");

				// We know, that this error can occur if:
				// 1) Send 2 or more Json messages at once like {"$type":"...}{"$type":"...}. This isn't a json object anymore.
				// Determine if jsonString has this format. Checking for existing '}{'
				// If so, then split by }{ and them back to the split strings
				if (!jsonString.Contains("}{"))
				{
					// Maybe save the string?
					this.LogError($"No pattern found, cannot transform {jsonString}");
					return;
				}

				this.LogWarning($"Pattern found, try to split and fire OnMessageReceived again ...");

				var possibleMessages = jsonString.Split("}{", StringSplitOptions.RemoveEmptyEntries);

				possibleMessages[0] += "}";

				var lastIndex = possibleMessages.Length - 1;

				if (lastIndex > 0)
				{
					for (int i = 1; i < lastIndex; i++)
					{
						possibleMessages[i] = "{" + possibleMessages[i] + "}";
					}

					possibleMessages[lastIndex] = "{" + possibleMessages[lastIndex];
				}

				foreach (var possibleMessage in possibleMessages)
				{
					OnMessageReceived(possibleMessage);
				}
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

		private IEnumerable<TransformedObjectWaiter> FindTransformedObjectWaiters(TransformedObject transformedObject)
		{
			return _transformedObjectWaiters.Values.Where(x => x.Discriminator == transformedObject.Discriminator);
		}

		private void AddTransformedObject(TransformedObject transformedObject)
		{
			_transformedObjects.TryAdd(transformedObject.Id, transformedObject);
		}

		public void Dispose()
		{
			Stop(true);
		}
	}
}