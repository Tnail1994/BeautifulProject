using Remote.Server.Common.Contracts;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Core.Exceptions;
using Core.Extensions;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Remote.Server
{
	public class AsyncServer : IAsyncServer, IDisposable
	{
		private static int _errorCount;
		private readonly int _maxErrorCount;

		private readonly TcpListener _listener;
		private readonly CancellationTokenSource _cts = new CancellationTokenSource();

		private readonly ConcurrentDictionary<string, TcpClient> _connectedClients = new();

		public event Action<ConnectionOccurObject>? NewConnectionOccured;

		public AsyncServer(IAsyncServerSettings settings)
		{
			var asyncServerSettings = settings;
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

		public void Remove(string clientId)
		{
			if (_connectedClients.TryRemove(clientId, out var tcpClient))
				tcpClient.Dispose();
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
				var serverCertificate = new X509Certificate2(@"C:\Users\Stefan\Desktop\Beautiful_Server_Cert.pfx");

				while (!_cts.Token.IsCancellationRequested)
				{
					this.LogInfo("Listening...");
					var client = await _listener.AcceptTcpClientAsync();
					var sslStream = new SslStream(client.GetStream(), false, App_CertificateValidation);

					// todo: make configure values
					await sslStream.AuthenticateAsServerAsync(serverCertificate, false, false);

					var clientId = Guid.NewGuid().ToString();
					var addingResult = _connectedClients.TryAdd(clientId, client);

					if (!addingResult)
						this.LogWarning($"Cannot add Id {clientId} to dictionary.");

					this.LogInfo($"New Connection: Id = {clientId}");
					NewConnectionOccured?.Invoke(ConnectionOccurObject.Create(clientId, client, sslStream));
				}
			}
			catch (OperationCanceledException oce)
			{
				this.LogDebug($"{oce.Message}");
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

		public X509Certificate ServerCertificateFile { get; set; }

		private bool App_CertificateValidation(object sender, X509Certificate? certificate, X509Chain? chain,
			SslPolicyErrors sslpolicyerrors)
		{
			return true;
		}

		public void Dispose()
		{
			_listener.Stop();
			_cts.Dispose();
		}
	}
}