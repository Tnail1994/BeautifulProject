using System.Net.Sockets;
using System.Text;
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
			_receivingCancellationTokenSource = new CancellationTokenSource();

			_socket = socket;
		}

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
						Log.Warning("Client connection timed out");
						break;
					}

					var received = await receiveTask;
					if (received == 0)
					{
						Log.Warning("Client connection closed");
						break;
					}

					Log.Information($"Message received: {received} bytes");

					var json = Encoding.UTF8.GetString(buffer, 0, received);
					MessageReceived?.Invoke(json);
				}
			}
			catch (OperationCanceledException)
			{
				// Erwartete Ausnahme bei Cancellation, kann ignoriert werden
			}
			catch (SocketException ex)
			{
				Log.Error(ex.Message);
			}
			catch (Exception ex)
			{
				Log.Error($"Error processing client: {ex.Message}");
			}
			// catch all other exceptions
		}

		public void Send(string message)
		{
			var messageBytes = Encoding.UTF8.GetBytes(message);
			_socket.SendAsync(messageBytes, SocketFlags.None);
		}
	}
}