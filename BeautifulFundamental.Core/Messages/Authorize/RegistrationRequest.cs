using BeautifulFundamental.Core.Communication.Implementations;
using Newtonsoft.Json;

namespace BeautifulFundamental.Core.Messages.Authorize
{
	public class RegistrationRequestValue
	{
		public RegistrationRequestValue(string name)
		{
			Name = name;
		}

		public string Name { get; set; }
	}

	public class RegistrationRequest : NetworkMessage<RegistrationRequestValue>
	{
		[JsonIgnore]
		public RegistrationRequestValue? RegistrationRequestValue
		{
			get => MessageObject;
			set => MessageObject = value;
		}
	}
}