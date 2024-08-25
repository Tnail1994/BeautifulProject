using BeautifulFundamental.Core.Communication.Implementations;
using Newtonsoft.Json;

namespace BeautifulFundamental.Core.Messages.CheckAlive
{
	public class CheckAliveRequest : NetworkMessage<bool>
	{
		[JsonIgnore]
		public bool Success
		{
			get => MessageObject;
			set => MessageObject = value;
		}
	}
}