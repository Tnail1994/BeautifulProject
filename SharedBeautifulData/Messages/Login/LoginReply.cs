using Newtonsoft.Json;
using Remote.Communication.Common.Implementations;

namespace SharedBeautifulData.Messages.Login
{
	public class LoginReply : BaseMessage<string>
	{
		[JsonIgnore]
		public string? Token
		{
			get => MessageObject;
			set => MessageObject = value;
		}
	}
}