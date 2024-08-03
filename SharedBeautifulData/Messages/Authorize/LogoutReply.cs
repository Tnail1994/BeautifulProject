using Newtonsoft.Json;
using Remote.Communication.Common.Implementations;

namespace SharedBeautifulData.Messages.Authorize
{
	public class LogoutReply : NetworkMessage<bool>
	{
		[JsonIgnore]
		public bool IsOk
		{
			get => MessageObject;
			set => MessageObject = value;
		}
	}
}