using System.Text.Json.Serialization;
using Remote.Communication.Common.Implementations;

namespace SharedBeautifulData.Messages.Authorize
{
	public enum LoginRequestType
	{
		DeviceIdent,
		Username,
	}

	public class LoginRequestValue
	{
		public LoginRequestType Type { get; set; }
		public string? Value { get; set; }
		public bool StayActive { get; set; }
	}

	public class LoginRequest : NetworkMessage<LoginRequestValue>
	{
		[JsonIgnore]
		public LoginRequestValue? RequestValue
		{
			get => MessageObject;
			set => MessageObject = value;
		}
	}
}