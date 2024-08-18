using Session.Common.Contracts.Context.Db;
using Session.Common.Implementations;

namespace Session.Common.Contracts.Context
{
    public interface ISessionDetail
    {
        event EventHandler<DetailsChangedArgs> DetailsChanged;
        string SessionId { get; }
        string TypeName { get; }
        IEntryDto Convert();
    }

    public class DetailsChangedArgs
    {
        public DetailsChangedArgs(ISessionKey sessionKey, string detailsTypeName)
        {
            SessionKey = sessionKey;
            DetailsTypeName = detailsTypeName;
        }

        public string SessionId => SessionKey.SessionId;
        public ISessionKey SessionKey { get; }
        public string DetailsTypeName { get; }
    }
}