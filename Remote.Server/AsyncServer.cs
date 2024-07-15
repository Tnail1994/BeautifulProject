using Remote.Server.Common.Contracts;
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
			this.LogInfo("Server starting...");

			_listener.Start();

			this.LogInfo("Server started.");

			return Task.Factory.StartNew(ListenForClientsAsync, _cts.Token);
		}

		public void Stop()
		{
			this.LogInfo("Server stopping...");

			Dispose();

			this.LogInfo("Server stopped.");
		}

		private async Task ListenForClientsAsync()
		{
			this.LogInfo("Server start listening for clients...");

			try
			{
				while (!_cts.Token.IsCancellationRequested)
				{
					this.LogInfo("Listening...");
					var client = await _listener.AcceptTcpClientAsync();
					var clientId = Guid.NewGuid().ToString();
					var addingResult = _connectedClients.TryAdd(clientId, client);

					if (!addingResult)
						this.LogWarning($"Cannot add Id {clientId} to dictionary.");

					this.LogInfo($"New Connection: Id = {clientId}");
					NewConnectionOccured?.Invoke(client);
				}
			}
			catch (OperationCanceledException oce)
			{
				this.Log($"{oce.Message}");
			}
			catch (BaseException baseException)
			{
				this.LogError($"{baseException.Message}");

				// todo: we know that the exception is out of our project. We have to figure out, what should happen if the project (like Session) fails
				// todo: processing the NewConnectionOccured event. We maybe have to retry the event?
			}
			catch (Exception ex) when (!_cts.Token.IsCancellationRequested)
			{
				this.LogFatal($"!!! Unexpected error in listener loop: {ex.Message}+" +
				              $"Stacktrace: {ex.StackTrace}");
			}
			finally
			{
				if (!_cts.Token.IsCancellationRequested)
				{
					_errorCount++;

					if (_errorCount > _maxErrorCount)
					{
						this.LogFatal(
							$"Error count is greater than {_maxErrorCount}. Stopping server.");
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