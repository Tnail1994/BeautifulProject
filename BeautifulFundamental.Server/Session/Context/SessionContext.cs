using BeautifulFundamental.Core.Identification;
using BeautifulFundamental.Server.Session.Contracts.Context;
using BeautifulFundamental.Server.Session.Contracts.Context.Db;

namespace BeautifulFundamental.Server.Session.Context
{
	public class SessionContext : ISessionContext
	{
		/// <summary>
		/// Key: The type name of the entry
		/// Value: EntryDto
		/// </summary>
		private readonly Dictionary<string, IEntryDto> _typeEntriesDto = new();

		public SessionContext(IIdentificationKey identificationKey)
		{
			IdentificationKey = identificationKey;
		}

		public string SessionId => IdentificationKey.SessionId;
		public IIdentificationKey IdentificationKey { get; }

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