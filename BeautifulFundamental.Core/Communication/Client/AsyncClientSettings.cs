﻿namespace BeautifulFundamental.Core.Communication.Client
{
	public interface IAsyncClientSettings
	{
		int BufferSize { get; init; }
		int ClientTimeout { get; init; }
		string IpAddress { get; init; }
		int Port { get; init; }

		/// <summary>
		/// Should this client operate on the server or client side?
		/// </summary>
		bool IsServerClient { get; init; }
	}

	public class AsyncClientSettings : IAsyncClientSettings
	{
		private const int DefaultBufferSize = 4096;
		private const int DefaultClientTimeout = 5;
		private const string DefaultIpAddress = "localhost";
		private const int DefaultPort = 8910;

		public int BufferSize { get; init; } = DefaultBufferSize;
		public int ClientTimeout { get; init; } = DefaultClientTimeout;
		public string IpAddress { get; init; } = DefaultIpAddress;

		public int Port { get; init; } = DefaultPort;
		public bool IsServerClient { get; init; }

		public static AsyncClientSettings Default => new()
		{
			BufferSize = DefaultBufferSize,
			ClientTimeout = DefaultClientTimeout, // Minutes
			IpAddress = DefaultIpAddress,
			Port = DefaultPort,
			IsServerClient = false,
		};
	}
}