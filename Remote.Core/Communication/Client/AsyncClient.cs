using System.Net.Sockets;
using System.Text;
using Configurations.General.Settings;
using CoreHelpers;
using Microsoft.Extensions.Options;
using Serilog;

namespace Remote.Core.Communication.Client
{
	public interface IAsyncClient : IDisposable
	{
		string Id { get; }
		event Action<string> MessageReceived;
		void StartReceivingAsync();
		void Send(string message);
	}

	internal class AsyncClient : IAsyncClient
	{
		private readonly IClient _client;

		private readonly CancellationTokenSource _receivingCancellationTokenSource;
		private readonly TimeSpan _clientTimeout;

		private readonly int _bufferSize;

		private AsyncClient(IClient client, IOptions<AsyncClientSettings> options)
		{
			Id = GuidIdCreator.CreateString();

			_receivingCancellationTokenSource = new CancellationTokenSource();

			var settings = options.Value;

			_clientTimeout = TimeSpan.FromMinutes(settings.ClientTimeout);
			_bufferSize = settings.BufferSize;

			_client = client;
		}

		public string Id { get; }

		public event Action<string>? MessageReceived;

		public static IAsyncClient Create(IClient client, IOptions<AsyncClientSettings> options)
		{
			return new AsyncClient(client, options);
		}

		public async void StartReceivingAsync()
		{
			try
			{
				var buffer = new byte[4096];
				while (!_receivingCancellationTokenSource.Token.IsCancellationRequested)
				{
					var receiveTask = _client.ReceiveAsync(buffer, SocketFlags.None);
					var timeoutTask = Task.Delay(_clientTimeout, _receivingCancellationTokenSource.Token);

					var completedTask = await Task.WhenAny(receiveTask, timeoutTask);
					if (completedTask == timeoutTask)
					{
						Log.Warning($"Client connection timed out. Id: {Id}");
						break;
					}

					var received = await receiveTask;
					if (received == 0)
					{
						Log.Warning($"Client connection closed. Id: {Id}");
						break;
					}

					Log.Information($"Message received: {received} bytes. Id: {Id}");

					var json = Encoding.UTF8.GetString(buffer, 0, received);
					MessageReceived?.Invoke(json);
				}
			}
			catch (OperationCanceledException)
			{
				Log.Debug("Receiving cancelled");
			}
			catch (SocketException ex)
			{
				switch (ex.ErrorCode)
				{
					default:
						Log.Error(ex.Message);
						break;
				}
			}
			catch (Exception ex)
			{
				Log.Fatal($"!!! Unexpected error receiving client. Id: {Id}" +
				          $"{ex.Message}" +
				          $"Stacktrace: {ex.StackTrace}.");
			}
		}

		public async void Send(string message)
		{
			var messageBytes = Encoding.UTF8.GetBytes(message);
			var sendingResult = await _client.SendAsync(messageBytes, SocketFlags.None);
			Log.Debug($"Send {sendingResult}. Id: {Id}");
		}

		public void Dispose()
		{
			_receivingCancellationTokenSource.Dispose();
			_client.Dispose();
		}
	}
}