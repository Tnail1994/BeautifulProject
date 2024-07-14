using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Remote.Server.Common.Contracts;

namespace Remote.Server
{
	// Extract options like:
	// - Port
	// - IP Address
	// - Max Listeners

	public class AsyncSocketServer : IAsyncSocketServer, IDisposable
	{
		private readonly Socket _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		private readonly CancellationTokenSource _cts = new CancellationTokenSource();

		private readonly ConcurrentDictionary<string, Socket> _connectedClients = new();

		public event Action<Socket>? NewConnectionOccured;

		public Task StartAsync(int port = 8910, int maxListener = 100)
		{
			Console.WriteLine("Server starting...");

			_listener.Bind(new IPEndPoint(IPAddress.Any, port));
			_listener.Listen(maxListener);

			Console.WriteLine("Server started.");

			return Task.Run(ListenForClientsAsync, _cts.Token);
		}

		public void Stop()
		{
			Console.WriteLine("Server stopping...");

			_cts.Cancel();
			_listener.Close();

			Console.WriteLine("Server stopped.");
		}

		private async Task ListenForClientsAsync()
		{
			Console.WriteLine("Server start listening for clients...");

			try
			{
				while (!_cts.Token.IsCancellationRequested)
				{
					Console.WriteLine("Listening...");
					var client = await _listener.AcceptAsync();
					var clientId = Guid.NewGuid().ToString();
					_connectedClients.TryAdd(clientId, client);
					Console.WriteLine($"New Connection: Id = {clientId}");
					NewConnectionOccured?.Invoke(client);
				}
			}
			catch (OperationCanceledException)
			{
				// Erwartete Ausnahme bei Cancellation, kann ignoriert werden
			}
			catch (Exception ex) when (!_cts.Token.IsCancellationRequested)
			{
				Console.WriteLine($"Error in listener loop: {ex.Message}");
			}
		}

		public void Dispose()
		{
			_listener.Dispose();
			_cts.Dispose();
		}
	}
}