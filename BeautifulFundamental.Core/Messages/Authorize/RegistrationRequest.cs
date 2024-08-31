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
		public static RegistrationRequest Create(string name)
		{
			var registrationRequest = new RegistrationRequestValue(name);
			return new RegistrationRequest
			{
				RegistrationRequestValue = registrationRequest
			};
		}

		[JsonIgnore]
		public RegistrationRequestValue? RegistrationRequestValue
		{
			get => MessageObject;
			set => MessageObject = value;
		}
	}
}