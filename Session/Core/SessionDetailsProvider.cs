using Session.Common.Contracts;

namespace Session.Core
{
	public class SessionDetailsProvider : ISessionDetailsProvider
	{
		private readonly ISessionContext _sessionContext;

		public SessionDetailsProvider(ISessionContext sessionContext)
		{
			_sessionContext = sessionContext;
		}

		public TSessionDetail? GetSessionDetail<TSessionEntryDto, TSessionDetail>()
			where TSessionEntryDto : class, IEntryDto
			where TSessionDetail : class, ISessionDetail
		{
			TSessionDetail? sessionDetail = null;

			if (_sessionContext.TryGetEntry(out TSessionEntryDto? sessionEntryDto))
			{
				sessionDetail = sessionEntryDto?.Convert() as TSessionDetail;
			}

			return sessionDetail;
		}
	}
}