using BeautifulFundamental.Core.Extensions;
using BeautifulFundamental.Server.Db;
using BeautifulFundamental.Server.Session.Context.Db;

namespace BeautifulFundamental.Server.Session.Context
{
	public interface ISessionDetailsManager
	{
		TSessionDetail? GetSessionDetail<TSessionEntryDto, TSessionDetail>()
			where TSessionEntryDto : class, IEntryDto
			where TSessionDetail : class, ISessionDetail;

		void Observe(ISessionDetail sessionDetail);
	}

	public class SessionDetailsManager : ISessionDetailsManager, IDisposable
	{
		private class SessionDataMap
		{
			private SessionDataMap(ISessionDetail sessionDetail, IEntryDto? entryDto)
			{
				SessionDetail = sessionDetail;
				EntryDto = entryDto;
			}

			public static SessionDataMap Create(ISessionDetail sessionDetail)
			{
				return new SessionDataMap(sessionDetail, null);
			}

			public static SessionDataMap Create(ISessionDetail sessionDetail, IEntryDto entryDto)
			{
				return new SessionDataMap(sessionDetail, entryDto);
			}

			public ISessionDetail SessionDetail { get; set; }
			public IEntryDto? EntryDto { get; set; }

			//public string SessionId => SessionDetail.SessionId;

			public bool HasEntryDto => EntryDto != null;
		}

		private readonly ISessionContext _sessionContext;
		private readonly IDbManager _dbManager;

		/// <summary>
		/// Key: Type name of detail
		/// Value: Data Object which holds all session detail with entry dto
		/// </summary>
		private readonly Dictionary<string, SessionDataMap> _sessionDataMaps = new();

		public SessionDetailsManager(ISessionContext sessionContext, IDbManager dbManager)
		{
			_sessionContext = sessionContext;
			_dbManager = dbManager;
		}

		public TSessionDetail? GetSessionDetail<TSessionEntryDto, TSessionDetail>()
			where TSessionEntryDto : class, IEntryDto
			where TSessionDetail : class, ISessionDetail
		{
			TSessionDetail? sessionDetail = null;

			if (_sessionContext.TryGetEntry(out TSessionEntryDto? sessionEntryDto))
			{
				sessionDetail = sessionEntryDto?.Convert(_sessionContext.IdentificationKey) as TSessionDetail;
			}

			if (sessionDetail != null && sessionEntryDto != null)
			{
				sessionDetail.DetailsChanged += OnDetailsChanged;
				_sessionDataMaps.Add(sessionDetail.TypeName, SessionDataMap.Create(sessionDetail, sessionEntryDto));
			}

			return sessionDetail;
		}

		// Automatically save changes when details changing
		private void OnDetailsChanged(object? sender, DetailsChangedArgs e)
		{
			if (!_sessionDataMaps.TryGetValue(e.DetailsTypeName, out var sessionDataMap))
			{
				this.LogDebug($"No details for {e.DetailsTypeName} found.", _sessionContext.SessionId);
				return;
			}

			// Late init, because db entry did not exist
			if (!sessionDataMap.HasEntryDto)
			{
				sessionDataMap.EntryDto = sessionDataMap.SessionDetail.Convert();
			}

			if (sessionDataMap.EntryDto is EntityDto entity)
			{
				sessionDataMap.EntryDto.Update(sessionDataMap.SessionDetail);
				_dbManager.SaveChanges(entity, e.IdentificationKey);
			}
		}

		// but what if the sessionDetail is not found while call GetSessionDetail
		// So we did not register successfully
		public void Observe(ISessionDetail sessionDetail)
		{
			if (_sessionDataMaps.ContainsKey(sessionDetail.TypeName))
				return;

			sessionDetail.DetailsChanged += OnDetailsChanged;
			_sessionDataMaps.Add(sessionDetail.TypeName, SessionDataMap.Create(sessionDetail));
		}


		public void Dispose()
		{
			foreach (var sessionDataMap in _sessionDataMaps.Values)
			{
				sessionDataMap.SessionDetail.DetailsChanged -= OnDetailsChanged;
			}
		}
	}
}