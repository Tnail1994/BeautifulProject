using DbManagement.Common.Contracts;
using DbManagement.Common.Implementations;
using Session.Common.Contracts.Context.Db;

namespace Session.Context.Db
{
    public abstract class ContextCollection<TEntryDto> : BaseDbContext<TEntryDto>, IContextCollection
		where TEntryDto : EntryDto
	{
		protected ContextCollection(IDbContextSettings dbContextSettings) : base(dbContextSettings)
		{
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
	}
}