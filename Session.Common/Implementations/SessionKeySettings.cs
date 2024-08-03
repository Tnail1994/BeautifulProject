using Session.Common.Contracts;

namespace Session.Common.Implementations
{
	public class SessionKeySettings : ISessionKeySettings
	{
		private const bool DefaultGenerateId = true;
		public bool GenerateId { get; init; } = DefaultGenerateId;

		public static SessionKeySettings Default => new()
		{
			GenerateId = DefaultGenerateId
		};
	}
}