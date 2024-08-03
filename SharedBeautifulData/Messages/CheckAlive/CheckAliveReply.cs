using Newtonsoft.Json;
using Remote.Communication.Common.Implementations;

namespace SharedBeautifulData.Messages.CheckAlive
{
	public class CheckAliveReply : NetworkMessage<bool>, IReplyMessage
	{
		[JsonIgnore]
		public bool Success
		{
			get => MessageObject;
			set => MessageObject = value;
		}
	}
}