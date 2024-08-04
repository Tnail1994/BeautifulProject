using System.Text.Json.Serialization;
using Remote.Communication.Common.Implementations;

namespace SharedBeautifulData.Messages.Authorize
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