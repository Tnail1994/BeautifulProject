using DbManagement.Common.Contracts;
using DbManagement.Common.Implementations;
using Session.Common.Contracts.Context.Db;
using Session.Common.Implementations;

namespace Session.Context.Db
{
	public abstract class ContextCollection<TEntryDto> : BaseDbContext<TEntryDto>, IContextCollection
		where TEntryDto : EntryDto
	{
		private readonly ISessionDataProvider _sessionDataProvider;

		protected ContextCollection(IDbContextSettings dbContextSettings, ISessionDataProvider sessionDataProvider) :
			base(
				dbContextSettings)
		{
			_sessionDataProvider = sessionDataProvider;
		}

		private TEntryDto? GetEntryBySession(string sessionId)
		{
			if (GetEntities() is IEnumerable<TEntryDto> entriesDto)
			{
				return entriesDto.FirstOrDefault(entryDto => entryDto.SessionId.Equals(sessionId));
			}

			return null;
		}

		public IEntryDto? GetEntry(string sessionId)
		{
			return GetEntryBySession(sessionId);
		}

		protected override bool CustomMissingEntriesFilter(TEntryDto entry)
		{
			return GetSessionState(entry.SessionId).Equals((int)SessionState.Stopped);
		}

		private SessionState GetSessionState(string sessionId)
		{
			if (_sessionDataProvider.TryGetSessionState(sessionId, out SessionState sessionState))
				return sessionState;

			return SessionState.None;
		}
	}
}