﻿using System.Net.Sockets;

namespace Remote.Core.Communication
{
	public interface IAsyncClientFactory
	{
		IAsyncClient Create(Socket socket);
	}

	public class AsyncClientFactory : IAsyncClientFactory
	{
		public IAsyncClient Create(Socket socket)
		{
			return AsyncClient.Create(socket);
		}
	}
}