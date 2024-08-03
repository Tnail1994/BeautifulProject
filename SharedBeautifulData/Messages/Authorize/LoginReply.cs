using Newtonsoft.Json;
using Remote.Communication.Common.Implementations;

namespace SharedBeautifulData.Messages.Authorize
{
	public class LoginReply : NetworkMessage<string>
	{
		[JsonIgnore]
		public string? Token
		{
			get => MessageObject;
			set => MessageObject = value;
		}
	}
}