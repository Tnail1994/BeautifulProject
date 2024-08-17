namespace Session.Common.Contracts
{
	public interface ISessionContextManager
	{
		bool TryFillSessionContext(ISessionContext sessionContext);
	}
}