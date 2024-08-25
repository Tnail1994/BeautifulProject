namespace BeautifulFundamental.Core.Identification
{
	public interface IIdentificationKeySettings
	{
		bool GenerateId { get; init; }
	}

	public class IdentificationKeySettings : IIdentificationKeySettings
	{
		private const bool DefaultGenerateId = true;
		public bool GenerateId { get; init; } = DefaultGenerateId;

		public static IdentificationKeySettings Default => new()
		{
			GenerateId = DefaultGenerateId
		};
	}
}