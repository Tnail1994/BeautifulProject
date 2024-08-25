using BeautifulFundamental.Core.Identification;
using BeautifulFundamental.Server.Session.Contracts.Context.Db;

namespace BeautifulFundamental.Server.Session.Contracts.Context
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
		IIdentificationKey IdentificationKey { get; }
		void AddEntry(IEntryDto entry);
		bool TryGetEntry<TEntryDto>(out TEntryDto? entryDto) where TEntryDto : IEntryDto;
	}
}