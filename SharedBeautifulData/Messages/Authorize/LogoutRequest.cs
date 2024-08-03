using Newtonsoft.Json;
using Remote.Communication.Common.Implementations;

namespace SharedBeautifulData.Messages.Authorize
{
	public class LogoutRequest : NetworkMessage<string>, IRequestMessage
	{
		[JsonIgnore]
		public string? Reason
		{
			get => MessageObject;
			set => MessageObject = value;
		}
	}
}