using BeautifulFundamental.Core.Helpers;

namespace BeautifulFundamental.Core.Identification
{
	public interface IIdentificationKey
	{
		string SessionId { get; }
		string InstantiatedSessionId { get; }
		void UpdateId(string sessionInfoId);
	}

	public class IdentificationKey : IIdentificationKey
	{
		public IdentificationKey(IIdentificationKeySettings identificationKeySettings)
		{
			SessionId = identificationKeySettings.GenerateId ? GuidIdCreator.CreateString() : string.Empty;
			InstantiatedSessionId = SessionId;
		}

		public string SessionId { get; private set; }
		public string InstantiatedSessionId { get; }

		public void UpdateId(string sessionInfoId)
		{
			SessionId = sessionInfoId;
		}
	}
}