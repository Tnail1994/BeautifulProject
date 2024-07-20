﻿using DbManagement.Common.Implementations;
using Session.Common.Implementations;

namespace DbManagement.Common.Contracts
{
	public interface IDbManager
	{
		IEnumerable<T>? GetEntities<T>(ISessionKey? sessionKey = null) where T : EntityDto;
		void AddEntity<T>(T dto, ISessionKey? sessionKey = null) where T : EntityDto;
	}
}