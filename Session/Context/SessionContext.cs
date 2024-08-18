using Session.Common.Contracts.Context;
using Session.Common.Contracts.Context.Db;
using Session.Common.Implementations;

namespace Session.Context
{
    public class SessionContext : ISessionContext
	{
		/// <summary>
		/// Key: The type name of the entry
		/// Value: EntryDto
		/// </summary>
		private readonly Dictionary<string, IEntryDto> _typeEntriesDto = new();

		public SessionContext(ISessionKey sessionKey)
		{
			SessionKey = sessionKey;
		}

		public string SessionId => SessionKey.SessionId;
		public ISessionKey SessionKey { get; }

		public void AddEntry(IEntryDto entry)
		{
			_typeEntriesDto.TryAdd(entry.TypeName, entry);
		}

		public bool TryGetEntry<TEntryDto>(out TEntryDto? entryDto) where TEntryDto : IEntryDto
		{
			if (_typeEntriesDto.TryGetValue(typeof(TEntryDto).Name, out var foundEntryDto) &&
			    foundEntryDto is TEntryDto tEntryDto)
			{
				entryDto = tEntryDto;
				return true;
			}

			entryDto = default;
			return false;
		}
	}
}