using Session.Common.Implementations;

namespace Session.Common.Contracts
{
	public interface IEntryDto
	{
		string TypeName { get; }
		ISessionDetail Convert(ISessionKey sessionKey);
		void Update(ISessionDetail sessionDetail);
	}
}