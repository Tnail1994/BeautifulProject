using Newtonsoft.Json;
using Remote.Communication.Common.Implementations;

namespace SharedBeautifulData.Messages.CheckAlive
{
	public class CheckAliveReply : NetworkMessage<bool>
	{
		[JsonIgnore]
		public bool Success
		{
			get => MessageObject;
			set => MessageObject = value;
		}
	}
}