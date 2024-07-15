using Remote.Server.Common.Contracts;
using Serilog;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Configurations.General.Settings;
using CoreImplementations;
using Microsoft.Extensions.Options;

namespace Remote.Server
{
	public class AsyncServer : IAsyncServer, IDisposable
	{
		private static int _errorCount;
		private readonly int _maxErrorCount;

		private readonly TcpListener _listener;
		private readonly CancellationTokenSource _cts = new CancellationTokenSource();

		private readonly ConcurrentDictionary<string, TcpClient> _connectedClients = new();

		public event Action<TcpClient>? NewConnectionOccured;

		public AsyncServer(IOptions<AsyncServerSettings> options)
		{
			var asyncServerSettings = options.Value;
			var ipAddress = IPAddress.Parse(asyncServerSettings.IpAddress);
			_maxErrorCount = asyncServerSettings.MaxErrorCount;
			_listener = new TcpListener(ipAddress, asyncServerSettings.Port);
		}

		public Task StartAsync()
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
				Log.Debug($"[AsyncSocketServer]\n {oce.Message}");
			}
			catch (BaseException baseException)
			{
				Log.Error($"[AsyncSocketServer]\n {baseException.Message}");

				// todo: we know that the exception is out of our project. We have to figure out, what should happen if the project (like Session) fails
				// todo: processing the NewConnectionOccured event. We maybe have to retry the event?
			}
			catch (Exception ex) when (!_cts.Token.IsCancellationRequested)
			{
				Log.Fatal($"[AsyncSocketServer]\n !!! Unexpected error in listener loop: {ex.Message}+" +
				          $"Stacktrace: {ex.StackTrace}");
			}
			finally
			{
				if (!_cts.Token.IsCancellationRequested)
				{
					_errorCount++;

					if (_errorCount > _maxErrorCount)
					{
						Log.Fatal(
							$"[AsyncSocketServer]\n Error count is greater than {_maxErrorCount}. Stopping server.");
						Stop();
					}
					else
					{
						// We definitely have to restart listening
						await ListenForClientsAsync();
					}
				}
			}
		}

		public void Dispose()
		{
			_listener.Stop();
			_cts.Dispose();
		}
	}
}