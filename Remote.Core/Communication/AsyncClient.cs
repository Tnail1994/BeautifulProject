using System.Net.Sockets;
using System.Text;
using CoreHelpers;
using Serilog;

namespace Remote.Core.Communication
{
	public interface IAsyncClient
	{
		event Action<string> MessageReceived;
		void StartReceivingAsync();
		void Send(string message);
	}

	internal class AsyncClient : IAsyncClient
	{
		private readonly Socket _socket;

		private readonly CancellationTokenSource _receivingCancellationTokenSource;
		private readonly TimeSpan _clientTimeout = TimeSpan.FromMinutes(5);

		private AsyncClient(Socket socket)
		{
			Id = GuidIdCreator.CreateString();

			_receivingCancellationTokenSource = new CancellationTokenSource();

			_socket = socket;
		}

		public string Id { get; }

		public event Action<string>? MessageReceived;

		public static IAsyncClient Create(Socket socket)
		{
			return new AsyncClient(socket);
		}

		public async void StartReceivingAsync()
		{
			try
			{
				var buffer = new byte[4096];
				while (!_receivingCancellationTokenSource.Token.IsCancellationRequested)
				{
					var receiveTask = _socket.ReceiveAsync(buffer, SocketFlags.None);
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
						Log.Error(ex.Message);
						break;
				}
			}
			catch (Exception ex)
			{
				Log.Fatal($"!!! Unexpected error receiving client. Id: {Id}" +
				          $"{ex.Message}" +
				          $"Stacktrace: {ex.StackTrace}.");
			}
		}

		public async void Send(string message)
		{
			var messageBytes = Encoding.UTF8.GetBytes(message);
			var sendingResult = await _socket.SendAsync(messageBytes, SocketFlags.None);
			Log.Debug($"Send {sendingResult}. Id: {Id}");
		}
	}
}