namespace Session.Common.Contracts.Context
{
    public interface ISessionContextManager
    {
        bool TryFillSessionContext(ISessionContext sessionContext);
    }
}