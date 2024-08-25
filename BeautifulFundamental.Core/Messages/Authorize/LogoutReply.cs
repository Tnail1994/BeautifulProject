using BeautifulFundamental.Core.Communication.Implementations;
using Newtonsoft.Json;

namespace BeautifulFundamental.Core.Messages.Authorize
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