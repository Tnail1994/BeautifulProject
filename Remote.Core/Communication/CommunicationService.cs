using System.Collections.Concurrent;
using Newtonsoft.Json;
using Remote.Core.Communication.Client;
using Remote.Core.Implementations;
using Remote.Core.Transformation;
using Serilog;

namespace Remote.Core.Communication
{
	public interface ICommunicationService : IDisposable
	{
		void SetClient(IAsyncClient client);
		void Start();

		Task<T> ReceiveAsync<T>() where T : IBaseMessage;
		void SendAsync(object messageObj);
		bool IsClientSet { get; }
	}

	public class CommunicationService : ICommunicationService
	{
		private readonly ConcurrentDictionary<string, TransformedObject> _transformedObjects = new();
		private readonly ConcurrentDictionary<string, TransformedObjectWaiter> _transformedObjectWaiters = new();

		private readonly ITransformerService _transformerService;

		private IAsyncClient? _client;

		public CommunicationService(ITransformerService transformerService)
		{
			_transformerService = transformerService;
		}

		public bool IsClientSet => _client != null;

		private IAsyncClient Client => _client ??
		                               throw new NullReferenceException(
			                               "Client is null. No socket for communication available.");

		public void SetClient(IAsyncClient client)
		{
			if (IsClientSet)
			{
				Log.Warning("Client is already set. \n" +
				            $"Set client: {Client.Id} \n" +
				            $"New client: {client.Id} ");
				return;
			}

			_client = client;
			Log.Debug($"Set client. Code: <cs_setClient>" +
			          $"Set client: {Client.Id}");
		}

		public void Start()
		{
			Client.StartReceivingAsync();
			Client.MessageReceived += OnMessageReceived;
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

			_transformedObjects.TryRemove(transformedObject.Id, out _);
		}

		public void SendAsync(object messageObj)
		{
			var jsonString = JsonConvert.SerializeObject(messageObj, JsonConfig.Settings);
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
				var transformedObject = _transformerService.Transform(jsonString);
				AddTransformedObject(transformedObject);

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
				Log.Error($"[CommunicationService] \n Json reader error for jsonString: {jsonString}.\n" +
				          $"{jsonReaderException.Message}");

				// Todo check, why jsonString is not a valid json format
			}
			catch (JsonException ex)
			{
				Log.Error($"[CommunicationService] \n Json error for jsonString: {jsonString}.\n" +
				          $"{ex.Message}");
			}
			catch (TransformException ex)
			{
				switch (ex.ErrorCode)
				{
					// Todo, depending on case do error handling

					case 1:
					case 2:
					case 3:
						Log.Error($"[CommunicationService] \n Transform error for jsonString: {jsonString}.\n" +
						          $"{ex.Message}");
						break;
				}
			}
			catch (Exception ex)
			{
				Log.Fatal($"[CommunicationService] \n !!! Unexpected error while OnMessageReceived event\n" +
				          $"Message: {ex.Message}\n" +
				          $"Stacktrace: {ex.StackTrace}\n");
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