using CoreHelpers;
using Serilog;
using System.Net.Sockets;
using System.Text;

namespace Remote.Core.Communication.Client
{
	public interface IAsyncClient : IDisposable
	{
		string Id { get; }
		event Action<string> MessageReceived;
		event EventHandler<string> ConnectionLost;
		void StartReceivingAsync();
		void Send(string message);
	}

	internal class AsyncClient : IAsyncClient
	{
		private readonly IClient _client;

		private readonly CancellationTokenSource _receivingCancellationTokenSource;
		private readonly TimeSpan _clientTimeout;

		private readonly int _bufferSize;

		private AsyncClient(IClient client, IAsyncClientSettings settings)
		{
			Id = GuidIdCreator.CreateString();

			_receivingCancellationTokenSource = new CancellationTokenSource();

			_clientTimeout = TimeSpan.FromMinutes(settings.ClientTimeout);
			_bufferSize = settings.BufferSize;

			_client = client;
		}

		public string Id { get; }

		public event Action<string>? MessageReceived;
		public event EventHandler<string>? ConnectionLost;

		public static IAsyncClient Create(IClient client, IAsyncClientSettings settings)
		{
			return new AsyncClient(client, settings);
		}

		public async void StartReceivingAsync()
		{
			try
			{
				var buffer = new byte[_bufferSize];
				while (!_receivingCancellationTokenSource.Token.IsCancellationRequested)
				{
					var receiveTask = _client.ReceiveAsync(buffer, SocketFlags.None);
					var timeoutTask = Task.Delay(_clientTimeout, _receivingCancellationTokenSource.Token);

					var completedTask = await Task.WhenAny(receiveTask, timeoutTask);
					if (completedTask == timeoutTask)
					{
						Log.Warning($"Client connection timed out. Id: {Id}");
						break;
					}

					var received = await receiveTask;
					if (received == 0)
					{
						Log.Warning($"Client connection closed. Id: {Id}");
						break;
					}

					Log.Information($"Message received: {received} bytes. Id: {Id}");

					var json = Encoding.UTF8.GetString(buffer, 0, received);
					MessageReceived?.Invoke(json);
				}
			}
			catch (OperationCanceledException)
			{
				Log.Debug("Receiving cancelled");
			}
			catch (SocketException ex)
			{
				switch (ex.ErrorCode)
				{
					default:
						Log.Error("SocketException");
						break;
				}

				ConnectionLost?.Invoke(this, Id);
			}
			catch (Exception ex)
			{
				Log.Fatal($"!!! Unexpected error receiving client. Id: {Id}" +
				          $"{ex.Message}" +
				          $"Stacktrace: {ex.StackTrace}.");

				ConnectionLost?.Invoke(this, ex.Message);
			}
		}

		public async void Send(string message)
		{
			//if (_client.IsNotConnected)
			//	return;

			var messageBytes = Encoding.UTF8.GetBytes(message);
			var sendingResult = await _client.SendAsync(messageBytes, SocketFlags.None);
			Log.Debug($"Send {sendingResult}. Id: {Id}");
		}

		public void Dispose()
		{
			_receivingCancellationTokenSource.Dispose();
			_client.Dispose();
		}
	}
}