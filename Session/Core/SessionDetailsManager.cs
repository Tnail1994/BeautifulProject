using System.Security.AccessControl;
using DbManagement.Common.Contracts;
using DbManagement.Common.Implementations;
using Session.Common.Contracts;
using Session.Contexts;

namespace Session.Core
{
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

			public string SessionId => SessionDetail.SessionId;

			public bool HasEntryDto => EntryDto != null;
		}

		private class DetailsEntryDtoLink
		{
			private DetailsEntryDtoLink(string detailsType, string entryDtoType)
			{
				DetailsType = detailsType;
				EntryDtoType = entryDtoType;
			}

			public static DetailsEntryDtoLink Create(string detailsType, string entryDtoType)
			{
				return new DetailsEntryDtoLink(detailsType, entryDtoType);
			}

			public string DetailsType { get; set; }
			public string EntryDtoType { get; set; }
		}

		private readonly ISessionContext _sessionContext;
		private readonly IDbManager _dbManager;

		/// <summary>
		/// Key: Type name of detail
		/// Value: Data Object which holds all session detail with entry dto
		/// </summary>
		private readonly Dictionary<string, SessionDataMap> _sessionDataMaps = new();

		private readonly List<DetailsEntryDtoLink> _links = new();


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
				sessionDetail = sessionEntryDto?.Convert(_sessionContext.SessionKey) as TSessionDetail;
			}

			if (sessionDetail != null && sessionEntryDto != null)
			{
				sessionDetail.DetailsChanged += OnDetailsChanged;
				_sessionDataMaps.Add(sessionDetail.TypeName, SessionDataMap.Create(sessionDetail, sessionEntryDto));
				_links.Add(DetailsEntryDtoLink.Create(sessionDetail.TypeName, sessionEntryDto.TypeName));
			}

			return sessionDetail;
		}

		// Automatically save changes when details changing
		private void OnDetailsChanged(object? sender, DetailsChangedArgs e)
		{
			if (_sessionDataMaps.TryGetValue(e.DetailsTypeName, out var sessionDataMap))
			{
				// todo check if are the same
				var savedSessionDetail = sessionDataMap.SessionDetail;
				var newSessionDetail = sender as ISessionDetail;

				if (!sessionDataMap.HasEntryDto)
				{
					sessionDataMap.EntryDto = sessionDataMap.SessionDetail.Convert();
					_links.Add(DetailsEntryDtoLink.Create(sessionDataMap.SessionDetail.TypeName,
						sessionDataMap.EntryDto.TypeName));
				}

				if (sessionDataMap.EntryDto is EntityDto entity)
				{
					sessionDataMap.EntryDto.Update(savedSessionDetail);
					_dbManager.SaveChanges(entity, e.SessionKey);
				}
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