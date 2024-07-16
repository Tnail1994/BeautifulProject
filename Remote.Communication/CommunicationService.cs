using System.Collections.Concurrent;
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
	public class CommunicationService : ICommunicationService
	{
		private readonly ConcurrentDictionary<string, TransformedObject> _transformedObjects = new();
		private readonly ConcurrentDictionary<string, TransformedObjectWaiter> _transformedObjectWaiters = new();

		private readonly ITransformerService _transformerService;
		private readonly ISessionKey _sessionKey;

		private IAsyncClient? _client;

		public CommunicationService(ITransformerService transformerService, ISessionKey sessionKey)
		{
			_transformerService = transformerService;
			_sessionKey = sessionKey;
		}

		public bool IsClientSet => _client != null;

		private IAsyncClient Client => _client ??
		                               throw new NullReferenceException(
			                               "Client is null. No socket for communication available.");

		public event EventHandler<string>? ConnectionLost;

		private string SessionId => _sessionKey.SessionId;

		public void SetClient(IAsyncClient client)
		{
			if (IsClientSet)
			{
				this.LogWarning("Client is already set. \n" +
				                $"Set client: {Client.Id} \n" +
				                $"New client: {client.Id} ", SessionId);
				return;
			}

			_client = client;
			this.LogDebug($"Set client. Code: <cs_setClient>" +
			              $"Set client: {Client.Id}", SessionId);
		}

		public async void Start()
		{
			Client.MessageReceived += OnMessageReceived;
			Client.ConnectionLost += ConnectionLost;

			if (Client.IsNotConnected)
			{
				var connectionResult = await Client.ConnectAsync();

				if (!connectionResult)
				{
					this.LogError("Connection to client failed. \n" +
					              "Check connection settings and try again.", SessionId);
					return;
				}
			}

			Client.StartReceivingAsync();
		}

		public Task<T> ReceiveAsync<T>() where T : IBaseMessage
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
			Client.Send(jsonString);
		}


		private async Task<T> WaitForReceive<T>() where T : IBaseMessage
		{
			var transformedObjectWaiter = TransformedObjectWaiter.Create(GetDiscriminator<T>());
			_transformedObjectWaiters.TryAdd(transformedObjectWaiter.Id, transformedObjectWaiter);

			var transformedObject = await transformedObjectWaiter.TaskCompletionSource.Task;

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

				this.LogDebug("Start transforming with Service...", SessionId);
				var transformedObject = _transformerService.Transform(jsonString);
				AddTransformedObject(transformedObject);
				this.LogDebug($"Finished and added: {transformedObject}", SessionId);

				var transformedObjectWaiters =
					_transformedObjectWaiters.Values.Where(x => x.Discriminator == transformedObject.Discriminator)
						.ToList();

				var messageAtLeastHandledOnce = false;
				foreach (var transformedObjectWaiter in transformedObjectWaiters)
				{
					transformedObjectWaiter.TaskCompletionSource.SetResult(transformedObject);

					if (transformedObjectWaiter.IsPermanent)
						continue;

					messageAtLeastHandledOnce = true;
					_transformedObjectWaiters.TryRemove(transformedObjectWaiter.Id, out _);
				}

				if (messageAtLeastHandledOnce)
					TryRemoveTransformedObject(transformedObject.Discriminator, transformedObject);
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
			foreach (var waiter in _transformedObjectWaiters)
			{
				waiter.Value.TaskCompletionSource.SetCanceled();
			}

			_transformedObjectWaiters.Clear();
			_transformedObjects.Clear();

			if (!IsClientSet)
				return;

			Client.Dispose();
			Client.MessageReceived -= OnMessageReceived;
		}
	}
}