using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata.Ecma335;
using Remote.Server.Common.Contracts;
using Serilog;

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
			Log.Information("Server starting...");

			_listener.Bind(new IPEndPoint(IPAddress.Any, port));
			_listener.Listen(maxListener);

			Log.Information("Server started.");

			return Task.Run(ListenForClientsAsync, _cts.Token);
		}

		public void Stop()
		{
			Log.Information("Server stopping...");

			_cts.Cancel();
			_listener.Close();

			Log.Information("Server stopped.");
		}

		private async Task ListenForClientsAsync()
		{
			Log.Information("Server start listening for clients...");

			try
			{
				while (!_cts.Token.IsCancellationRequested)
				{
					Log.Information("Listening...");
					var client = await _listener.AcceptAsync();
					var clientId = Guid.NewGuid().ToString();
					_connectedClients.TryAdd(clientId, client);
					Log.Information($"New Connection: Id = {clientId}");
					NewConnectionOccured?.Invoke(client);
				}
			}
			catch (OperationCanceledException)
			{
				// Erwartete Ausnahme bei Cancellation, kann ignoriert werden
			}
			catch (Exception ex) when (!_cts.Token.IsCancellationRequested)
			{
				Log.Error($"Error in listener loop: {ex.Message}");
			}
		}

		public void Dispose()
		{
			_listener.Dispose();
			_cts.Dispose();
		}
	}
}