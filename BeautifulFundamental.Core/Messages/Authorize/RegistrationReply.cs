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
		public static RegistrationReply Create(bool success, string? infoText = null)
		{
			var registrationReply = new RegistrationReplyValue
			{
				Success = success,
				InfoText = infoText
			};
			return new RegistrationReply
			{
				RegistrationReplyValue = registrationReply
			};
		}


		[JsonIgnore]
		public RegistrationReplyValue? RegistrationReplyValue
		{
			get => MessageObject;
			set => MessageObject = value;
		}
	}
}