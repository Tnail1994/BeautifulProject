namespace Session.Common.Contracts
{
	public interface IEntryDto
	{
		string TypeName { get; }

		ISessionDetail Convert();
	}
}