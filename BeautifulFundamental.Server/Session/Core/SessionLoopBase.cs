﻿using BeautifulFundamental.Core.Identification;
using BeautifulFundamental.Server.Session.Contracts.Core;

namespace BeautifulFundamental.Server.Session.Core
{
	public abstract class SessionLoopBase : ISessionLoop
	{
		public SessionLoopBase(IIdentificationKey identificationKey)
		{
			IdentificationKey = identificationKey;
		}

		protected IIdentificationKey IdentificationKey { get; }
		protected string SessionId => IdentificationKey.SessionId;
		protected abstract void Run();

		public void Start()
		{
			Run();
		}
	}
}