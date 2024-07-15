using Remote.Server.Common.Contracts;
using Serilog;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace Remote.Server
{
	// Extract options like:
	// - Port
	// - IP Address
	// - Max Listeners

	public class AsyncServer : IAsyncSocketServer, IDisposable
	{
		private readonly TcpListener _listener = new(IPAddress.Any, 8910);
		private readonly CancellationTokenSource _cts = new CancellationTokenSource();

		private readonly ConcurrentDictionary<string, TcpClient> _connectedClients = new();

		public event Action<TcpClient>? NewConnectionOccured;

		public Task StartAsync(int port = 8910, int maxListener = 100)
		{
			Log.Information("[AsyncSocketServer]\n Server starting...");

			_listener.Start();

			Log.Information("[AsyncSocketServer]\n Server started.");

			return Task.Factory.StartNew(ListenForClientsAsync, _cts.Token);
		}

		public void Stop()
		{
			Log.Information("[AsyncSocketServer]\n Server stopping...");

			Dispose();

			Log.Information("[AsyncSocketServer]\n Server stopped.");
		}

		private async Task ListenForClientsAsync()
		{
			Log.Information("[AsyncSocketServer]\n Server start listening for clients...");

			try
			{
				while (!_cts.Token.IsCancellationRequested)
				{
					Log.Information("[AsyncSocketServer]\n Listening...");
					var client = await _listener.AcceptTcpClientAsync();
					var clientId = Guid.NewGuid().ToString();
					var addingResult = _connectedClients.TryAdd(clientId, client);

					if (!addingResult)
						Log.Warning($"[AsyncSocketServer]\n Cannot add Id {clientId} to dictionary.");

					Log.Information($"[AsyncSocketServer]\n New Connection: Id = {clientId}");
					NewConnectionOccured?.Invoke(client);
				}
			}
			catch (OperationCanceledException oce)
			{
				Log.Debug(oce.Message);
			}
			catch (Exception ex) when (!_cts.Token.IsCancellationRequested)
			{
				Log.Fatal($"[AsyncSocketServer]\n !!! Unexpected error in listener loop: {ex.Message}+" +
				          $"Stacktrace: {ex.StackTrace}");
			}
		}

		public void Dispose()
		{
			_listener.Stop();
			_cts.Dispose();
		}
	}
}