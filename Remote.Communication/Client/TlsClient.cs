using System.Collections.Concurrent;
using System.Net.Security;
using System.Net.Sockets;
using Core.Extensions;
using Remote.Communication.Common.Client.Contracts;

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
		private readonly SslStream _sslStream;

		private readonly ConcurrentQueue<SendingBuffer> _sendingQueue = new();
		private readonly CancellationTokenSource _sendingLoopCts = new();

		private TlsClient(TcpClient client, SslStream sslStream)
		{
			_client = client;
			_sslStream = sslStream;

			StartSendingLoop();
		}

		private void StartSendingLoop()
		{
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

		public static IClient Create(TcpClient client, SslStream sslStream)
		{
			return new TlsClient(client, sslStream);
		}

		public bool Connected => _client.Connected;

		public bool IsNotConnected => !Connected;

		public async Task<int> ReceiveAsync(byte[] buffer)
		{
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
			await _client.ConnectAsync(ip, port);
			return _client.Connected;
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
			_client.Close();
		}

		public void Dispose()
		{
			_client.Dispose();
		}
	}
}