using Newtonsoft.Json;
using Remote.Communication.Common.Implementations;
using SharedBeautifulData.Objects;

namespace SharedBeautifulData.Messages
{
	public class UserMessage : BaseMessage<User>
	{
		[JsonIgnore]
		public User User
		{
			get => MessageObject ?? throw new InvalidOperationException("User is not set.");
			set => MessageObject = value;
		}
	}
}