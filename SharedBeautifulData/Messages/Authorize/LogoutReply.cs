using Newtonsoft.Json;
using Remote.Communication.Common.Implementations;

namespace SharedBeautifulData.Messages.Authorize
{
	public class LogoutReply : BaseMessage<bool>
	{
		[JsonIgnore]
		public bool IsOk
		{
			get => MessageObject;
			set => MessageObject = value;
		}
	}
}