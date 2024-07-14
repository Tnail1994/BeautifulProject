using System.Collections.Concurrent;
using Newtonsoft.Json;
using Remote.Core.Implementations;
using Remote.Core.Transformation;
using Serilog;

namespace Remote.Core.Communication
{
	public interface ICommunicationService
	{
		void SetClient(IAsyncClient client);
		void Start();

		Task<T> ReceiveAsync<T>() where T : IBaseMessage;
		void SendAsync(object messageObj);
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

		private IAsyncClient Client => _client ??
		                               throw new NullReferenceException(
			                               "Client is null. No socket for communication available.");

		public void SetClient(IAsyncClient client)
		{
			_client = client;
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

				foreach (var transformedObjectWaiter in transformedObjectWaiters)
				{
					transformedObjectWaiter.TaskCompletionSource.SetResult(transformedObject);

					if (transformedObjectWaiter.IsPermanent)
						continue;

					_transformedObjectWaiters.TryRemove(transformedObjectWaiter.Id, out _);
				}

				TryRemoveTransformedObject(transformedObject.Discriminator, transformedObject);
			}
			catch (TransformException ex)
			{
				switch (ex.ErrorCode)
				{
					case 1:
					case 2:
					case 3:
						break;
				}
			}
			catch (Exception e)
			{
				Log.Error(e.Message);
			}
		}

		private void AddTransformedObject(TransformedObject transformedObject)
		{
			_transformedObjects.TryAdd(transformedObject.Id, transformedObject);
		}
	}
}