using BeautifulFundamental.Core.Communication.Implementations;
using Newtonsoft.Json;

namespace BeautifulFundamental.Core.Messages.Authorize
{
	public class LogoutRequest : NetworkMessage<string>
	{
		[JsonIgnore]
		public string? Reason
		{
			get => MessageObject;
			set => MessageObject = value;
		}
	}
}