using BeautifulFundamental.Core.Communication.Implementations;
using BeautifulFundamental.Core.Communication.Transformation.Implementations;
using Newtonsoft.Json;

namespace Tests.TestObjects
{
	public class OtherTestMessage : NetworkMessage<TestObject>
	{
		[JsonIgnore]
		public TestObject TestObject
		{
			get => MessageObject ?? throw new InvalidOperationException("[OtherTestMessage] MessageObject is not set.");
			set => MessageObject = value;
		}

		public static string CreateString()
		{
			return JsonConvert.SerializeObject(Create(), JsonConfig.Settings);
		}

		public static OtherTestMessage Create()
		{
			return new OtherTestMessage
			{
				MessageObject = TestObject.Create("MockMessage2")
			};
		}
	}
}