using Session.Common.Implementations;

namespace Session.Common.Contracts
{
	public interface ISessionDetailsManager
	{
		TSessionDetail? GetSessionDetail<TSessionEntryDto, TSessionDetail>()
			where TSessionEntryDto : class, IEntryDto
			where TSessionDetail : class, ISessionDetail;

		void Observe(ISessionDetail sessionDetail);
	}


	public interface ISessionContext
	{
		string SessionId { get; }
		ISessionKey SessionKey { get; }
		void AddEntry(IEntryDto entry);
		bool TryGetEntry<TEntryDto>(out TEntryDto? entryDto) where TEntryDto : IEntryDto;
	}
}