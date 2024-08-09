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

		private readonly TcpClient _client;
		private readonly ITlsSettings _tlsSettings;
		private readonly bool _isServerClient;

		private SslStream? _sslStream;

		private readonly ConcurrentQueue<SendingBuffer> _sendingQueue = new();
		private readonly CancellationTokenSource _sendingLoopCts = new();
		private readonly TaskCompletionSource _connectedTcs = new();

		private TlsClient(string host, int port, ITlsSettings tlsSettings)
		{
			_client = new TcpClient(host, port);
			_tlsSettings = tlsSettings;
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
		/// 
		/// </summary>
		/// <exception cref="InvalidOperationException">WHen sslStream not init</exception>
		private void StartSendingLoop()
		{
			if (_sslStream == null)
				throw new InvalidOperationException("Cannot start sending loop, because sslStream is null.");

			_ = Task.Factory.StartNew(async () =>
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
			});
		}

		public static IClient CreateServerClient(TcpClient client, SslStream sslStream, ITlsSettings tlsSettings)
		{
			return new TlsClient(client, sslStream, tlsSettings);
		}

		public static IClient CreateClient(string host, int port, ITlsSettings tlsSettings)
		{
			return new TlsClient(host, port, tlsSettings);
		}

		public bool Connected => _client.Connected && _sslStream != null;

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

		// todo: due change of tls, how to reset correctly
		public void ResetSocket()
		{
			Close();
			_client.Client = new TcpClient().Client;
		}

		public NetworkStream GetStream()
		{
			return _client.GetStream();
		}

		public void Close()
		{
			if (_sslStream != null)
				_sslStream.Close();

			_client.Close();
		}

		public void Dispose()
		{
			Close();

			if (_sslStream != null)
				_sslStream.Dispose();

			_client.Dispose();
		}
	}
}