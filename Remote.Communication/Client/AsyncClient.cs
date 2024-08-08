using System.Net.Sockets;
using System.Text;
using Core.Extensions;
using Core.Helpers;
using Remote.Communication.Common.Client.Contracts;

namespace Remote.Communication.Client
{
	public class AsyncClient : IAsyncClient, IDisposable
	{
		private readonly IClient _client;

		private readonly CancellationTokenSource _receivingCancellationTokenSource;
		private readonly TimeSpan _clientTimeout;

		private readonly int _bufferSize;
		private readonly int _port;
		private readonly string _ip;

		public AsyncClient(IClient client, IAsyncClientSettings settings)
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
		public bool IsConnected => !IsNotConnected;

		public event Action<string>? MessageReceived;
		public event EventHandler<string>? ConnectionLost;

		public static IAsyncClient Create(IClient client, IAsyncClientSettings settings)
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
			}
			catch (Exception ex)
			{
				this.LogFatal($"!!! Unexpected error receiving client. Id: {Id}" +
				              $"{ex.Message}" +
				              $"Stacktrace: {ex.StackTrace}.");
			}
			finally
			{
				ConnectionLost?.Invoke(this, Id);
			}
		}

		public async void Send(string message)
		{
			//if (_client.IsNotConnected)
			//	return;
			try
			{
				var messageBytes = Encoding.UTF8.GetBytes(message);
				var sendingResult = await _client.SendAsync(messageBytes, SocketFlags.None);
				this.LogDebug($"Send {sendingResult}. Id: {Id}");
			}
			catch (SocketException ex)
			{
				switch (ex.ErrorCode)
				{
					default:
						this.LogWarning($"Cannot send {message} Id: {Id}\n" +
						                $"{ex.Message}");
						break;
				}
			}
			catch (Exception ex)
			{
				this.LogFatal($"Cannot send message {message}. Id: {Id}" +
				              $"{ex.Message}" +
				              $"Stacktrace: {ex.StackTrace}.");
			}
		}

		public void StopReceiving()
		{
			_receivingCancellationTokenSource.Cancel();
		}

		public void ResetSocket()
		{
			_client.ResetSocket();
		}

		public void Dispose()
		{
			StopReceiving();
			_receivingCancellationTokenSource.Dispose();
			_client.Dispose(); // the only special to call Dispose directly, because IClient is not registered in DI
		}
	}
}