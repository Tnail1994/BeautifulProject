using System.Net.Sockets;
using System.Text;
using Core.Extensions;
using Core.Helpers;
using Remote.Communication.Common.Client.Contracts;
using Remote.Communication.Common.Client.Implementations;

namespace Remote.Communication.Client
{
	internal class AsyncClient : IAsyncClient
	{
		private readonly IClient _client;

		private readonly CancellationTokenSource _receivingCancellationTokenSource;
		private readonly TimeSpan _clientTimeout;

		private readonly int _bufferSize;
		private readonly int _port;
		private readonly string _ip;

		private AsyncClient(IClient client, AsyncClientSettings settings)
		{
			Id = GuidIdCreator.CreateString();

			_receivingCancellationTokenSource = new CancellationTokenSource();

			_clientTimeout = TimeSpan.FromMinutes(settings.ClientTimeout);
			_bufferSize = settings.BufferSize;
			_ip = settings.IpAddress;
			_port = settings.Port;

			_client = client;
		}

		public string Id { get; }

		public bool IsNotConnected => _client.IsNotConnected;

		public event Action<string>? MessageReceived;
		public event EventHandler<string>? ConnectionLost;

		public static IAsyncClient Create(IClient client, AsyncClientSettings settings)
		{
			return new AsyncClient(client, settings);
		}

		public Task<bool> ConnectAsync()
		{
			return _client.ConnectAsync(_ip, _port);
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
						this.LogWarning($"Client connection timed out. Id: {Id}");
						ConnectionLost?.Invoke(this, Id);
						break;
					}

					var received = await receiveTask;
					if (received == 0)
					{
						this.LogWarning($"Client connection closed. Id: {Id}");
						break;
					}

					this.LogInfo($"Message received: {received} bytes. Id: {Id}");

					var json = Encoding.UTF8.GetString(buffer, 0, received);
					MessageReceived?.Invoke(json);
				}
			}
			catch (OperationCanceledException)
			{
				this.LogDebug("Receiving cancelled");
			}
			catch (SocketException ex)
			{
				switch (ex.ErrorCode)
				{
					default:
						this.LogError(ex.Message);
						break;
				}

				ConnectionLost?.Invoke(this, Id);
			}
			catch (Exception ex)
			{
				this.LogFatal($"!!! Unexpected error receiving client. Id: {Id}" +
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
			this.LogDebug($"Send {sendingResult}. Id: {Id}");
		}

		public void Dispose()
		{
			_receivingCancellationTokenSource.Dispose();
			_client.Dispose();
		}
	}
}