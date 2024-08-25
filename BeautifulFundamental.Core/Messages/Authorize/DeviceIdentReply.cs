using System.Text.Json.Serialization;
using BeautifulFundamental.Core.Communication.Implementations;

namespace BeautifulFundamental.Core.Messages.Authorize
{
	public class DeviceIdentReply : NetworkMessage<string>
	{
		[JsonIgnore]
		public string? Ident
		{
			get => MessageObject;
			set => MessageObject = value;
		}
	}
}