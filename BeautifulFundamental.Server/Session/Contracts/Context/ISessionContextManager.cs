namespace BeautifulFundamental.Server.Session.Contracts.Context
{
	public interface ISessionContextManager
	{
		bool TryFillSessionContext(ISessionContext sessionContext);
	}
}