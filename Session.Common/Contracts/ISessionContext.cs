using Session.Common.Implementations;

namespace Session.Common.Contracts
{
	public interface ISessionDetailsProvider
	{
		TSessionDetail? GetSessionDetail<TSessionEntryDto, TSessionDetail>()
			where TSessionEntryDto : class, IEntryDto
			where TSessionDetail : class, ISessionDetail;
	}


	public interface ISessionContext
	{
		string SessionId { get; }
		ISessionKey SessionKey { get; }
		void AddEntry(IEntryDto entry);
		bool TryGetEntry<TEntryDto>(out TEntryDto? entryDto) where TEntryDto : IEntryDto;
	}
}