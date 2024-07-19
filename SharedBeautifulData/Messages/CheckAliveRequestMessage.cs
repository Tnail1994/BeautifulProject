using Newtonsoft.Json;
using Remote.Communication.Common.Implementations;

namespace SharedBeautifulData.Messages
{
	public class CheckAliveRequestMessage : BaseMessage<bool>
	{
		[JsonIgnore]
		public bool Success
		{
			get => MessageObject;
			set => MessageObject = value;
		}
	}
}