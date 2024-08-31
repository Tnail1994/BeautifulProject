using BeautifulFundamental.Core.Communication.Implementations;
using Newtonsoft.Json;

namespace BeautifulFundamental.Core.Messages.Authorize
{
	public class RegistrationReplyValue
	{
		public bool Success { get; set; }
		public string? InfoText { get; set; }
	}

	public class RegistrationReply : NetworkMessage<RegistrationReplyValue>
	{
		public RegistrationReply(bool success, string? infoText = null)
		{
			RegistrationReplyValue ??= new RegistrationReplyValue();
			RegistrationReplyValue.Success = success;
			RegistrationReplyValue.InfoText = infoText;
		}


		[JsonIgnore]
		public RegistrationReplyValue? RegistrationReplyValue
		{
			get => MessageObject;
			set => MessageObject = value;
		}
	}
}