namespace Session.Common.Implementations
{
	public interface ISessionKeySettings
	{
		bool GenerateId { get; init; }
	}

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