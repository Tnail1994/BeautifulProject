using System.Text.Json.Serialization;
using Remote.Communication.Common.Implementations;

namespace SharedBeautifulData.Messages.Authorize
{
	public enum LoginRequestType
	{
		Username,
	}

	public class LoginRequest : NetworkMessage<LoginRequestType>, IRequestMessage
	{
		[JsonIgnore]
		public LoginRequestType Type
		{
			get => MessageObject;
			set => MessageObject = value;
		}
	}
}