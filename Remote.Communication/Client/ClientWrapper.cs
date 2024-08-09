﻿using System.Net.Security;
using System.Net.Sockets;
using Remote.Communication.Common.Client.Contracts;

namespace Remote.Communication.Client
{
	public class ClientWrapper : IClient
	{
		private readonly TcpClient _client;
		private readonly SslStream _sslStream;

		private ClientWrapper(TcpClient client, SslStream sslStream)
		{
			_client = client;
			_sslStream = sslStream;
		}

		public static IClient Create(TcpClient client, SslStream sslStream)
		{
			return new ClientWrapper(client, sslStream);
		}

		public bool Connected => _client.Connected;

		public bool IsNotConnected => !Connected;

		public async Task<int> ReceiveAsync(byte[] buffer)
		{
			return await _sslStream.ReadAsync(buffer);
		}

		public async Task<int> SendAsync(byte[] buffer)
		{
			await _sslStream.WriteAsync(buffer);
			return buffer.Length;
		}

		//public async Task<int> SendAsync(byte[] buffer)
		//{
		//	await AwaitCurrentWriteTask();

		//	_writeTask = _sslStream.WriteAsync(buffer);
		//	await _writeTask.Value;
		//	_writeTask = null;

		//	return buffer.Length;
		//}

		//private async Task AwaitCurrentWriteTask()
		//{
		//	if (_writeTask == null)
		//		return;

		//	await _writeTask.Value;
		//}

		public async Task<bool> ConnectAsync(string ip, int port)
		{
			await _client.ConnectAsync(ip, port);
			return _client.Connected;
		}

		public void ResetSocket()
		{
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