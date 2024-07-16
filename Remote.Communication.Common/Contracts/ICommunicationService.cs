﻿using Remote.Communication.Common.Client.Contracts;
using Remote.Communication.Common.Implementations;

namespace Remote.Communication.Common.Contracts
{
	public interface ICommunicationService : IDisposable
	{
		event EventHandler<string>? ConnectionLost;
		void SetClient(IAsyncClient client);
		void Start();

		Task<T> ReceiveAsync<T>() where T : IBaseMessage;
		void SendAsync(object messageObj);
		bool IsClientSet { get; }
	}
}