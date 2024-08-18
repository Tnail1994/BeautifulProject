using System.Collections.Concurrent;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using Core.Extensions;
using Remote.Communication.Common.Client.Contracts;
using Remote.Communication.Common.Helpers;

namespace Remote.Communication.Client
{
	public class TlsClient : IClient
	{
		private class SendingBuffer
		{
			private SendingBuffer(byte[] bytes)
			{
				SendingCompletedTcs = new TaskCompletionSource();
				Bytes = bytes;
			}

			public static SendingBuffer Create(byte[] buffer)
			{
				return new SendingBuffer(buffer);
			}

			public byte[] Bytes { get; set; }
			public TaskCompletionSource SendingCompletedTcs { get; set; }
		}

		private TcpClient _client;
		private readonly ITlsSettings _tlsSettings;
		private readonly bool _isServerClient;

		private SslStream? _sslStream;

		private readonly ConcurrentQueue<SendingBuffer> _sendingQueue = new();
		private CancellationTokenSource _sendingLoopCts = new();
		private TaskCompletionSource _connectedTcs = new();
		private readonly string? _host;
		private readonly int _port;

		private Task? _sendingLoopTask;

		private TlsClient(string host, int port, ITlsSettings tlsSettings)
		{
			_host = host;
			_port = port;
			_client = CreateTcpClient();
			_tlsSettings = tlsSettings;
		}

		public static IClient CreateClient(string host, int port, ITlsSettings tlsSettings)
		{
			return new TlsClient(host, port, tlsSettings);
		}


		private TlsClient(TcpClient client, SslStream sslStream, ITlsSettings tlsSettings)
		{
			_isServerClient = true;
			_client = client;
			_tlsSettings = tlsSettings;
			_sslStream = sslStream;

			TryStart();
		}

		/// <summary>
		/// Creating server client, using this ctor.
		/// No need of host and port, because due listener it is already connected.
		/// </summary>
		/// <param name="client">From listeners client</param>
		/// <param name="sslStream">Clients stream</param>
		/// <param name="tlsSettings">Settings</param>
		public static IClient CreateServerClient(TcpClient client, SslStream sslStream, ITlsSettings tlsSettings)
		{
			return new TlsClient(client, sslStream, tlsSettings);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <exception cref="InvalidOperationException">WHen sslStream not init</exception>
		private void StartSendingLoop()
		{
			if (_sslStream == null)
				throw new InvalidOperationException("Cannot start sending loop, because sslStream is null.");

			_sendingLoopTask = Task.Factory.StartNew(async () =>
			{
				try
				{
					while (!_sendingLoopCts.IsCancellationRequested)
					{
						while (_sendingQueue.TryDequeue(out var sendingBuffer))
						{
							await _sslStream.WriteAsync(sendingBuffer.Bytes);
							sendingBuffer.SendingCompletedTcs.SetResult();
						}
					}
				}
				catch (Exception ex)
				{
					this.LogFatal($"[TlsClient] Unexpected error in sending loop:\n" +
					              $"{ex.Message}\n" +
					              $"Stacktrace: {ex.StackTrace}");
				}
			}, _sendingLoopCts.Token);
		}


		public bool Connected => _client.Connected && _sslStream is { CanRead: true, CanWrite: true };

		public bool IsNotConnected => !Connected;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="buffer"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException">WHen sslStream not init</exception>
		public async Task<int> ReceiveAsync(byte[] buffer)
		{
			if (_sslStream == null)
			{
				await _connectedTcs.Task;

				if (_sslStream == null)
					throw new InvalidOperationException("Cannot receive, because sslStream is null.");
			}

			return await _sslStream.ReadAsync(buffer);
		}

		public async Task<int> SendAsync(byte[] buffer)
		{
			var sendingBuffer = SendingBuffer.Create(buffer);
			_sendingQueue.Enqueue(sendingBuffer);
			await sendingBuffer.SendingCompletedTcs.Task;
			return buffer.Length;
		}

		public async Task<bool> ConnectAsync(string ip, int port)
		{
			if (!_client.Connected && _host != null && port != 0)
			{
				await _client.ConnectAsync(_host, port);
			}

			if (!_isServerClient)
			{
				var clientCertificateCollection =
					CertificateCreator.CreateAsCollection(_tlsSettings.CertificatePath);

				_sslStream = new SslStream(_client.GetStream(), _tlsSettings.LeaveInnerStreamOpen,
					ValidateAsClient);

				await _sslStream.AuthenticateAsClientAsync(_tlsSettings.TargetHost, clientCertificateCollection,
					_tlsSettings.CheckCertificateRevocation);
			}

			TryStart();

			return Connected;
		}

		private void TryStart()
		{
			if (Connected)
			{
				_connectedTcs.SetResult();
				_connectedTcs = new TaskCompletionSource();
				StartSendingLoop();
			}
		}

		private bool ValidateAsClient(object sender, X509Certificate? certificate, X509Chain? chain,
			SslPolicyErrors sslPolicyErrors)
		{
			return sslPolicyErrors == SslPolicyErrors.None ||
			       (_tlsSettings.AllowRemoteCertificateChainErrors &&
			        sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors);
		}

		public void ResetSocket()
		{
			Close();
			_client = CreateTcpClient();
		}

		public NetworkStream GetStream()
		{
			return _client.GetStream();
		}

		public void Close()
		{
			_sendingLoopCts.Cancel();
			_sendingLoopCts = new CancellationTokenSource();

			if (_sslStream != null)
				_sslStream.Close();

			_client.Close();


			while (_sendingQueue.TryDequeue(out var sendingBuffer))
			{
				sendingBuffer.SendingCompletedTcs.SetCanceled();
			}

			if (_sendingLoopTask is { IsCompleted: true, IsCanceled: true, IsFaulted: true })
				_sendingLoopTask?.Dispose();

			_sendingLoopTask = null;
		}

		public void Dispose()
		{
			Close();

			if (_sslStream != null)
				_sslStream.Dispose();

			_client.Dispose();
		}

		private TcpClient CreateTcpClient()
		{
			try
			{
				if (_host == null || _port == 0)
					throw new SocketException();

				return new TcpClient(_host, _port);
			}
			catch (SocketException)
			{
				this.LogDebug($"Cannot create and connect with TcpClient({_host}´,{_port}\n" +
				              $"Creating TcpClient without connection settings, which will lead into a needed connect call");
				return new TcpClient();
			}
		}
	}
}