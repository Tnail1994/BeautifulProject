using BeautifulFundamental.Core.Communication.Implementations;
using BeautifulFundamental.Core.Communication.Transformation.Implementations;
using Newtonsoft.Json;

namespace Tests.TestObjects
{
	public class TestMessage : NetworkMessage<TestObject>
	{
		[JsonIgnore]
		public TestObject TestObject
		{
			get => MessageObject ?? throw new InvalidOperationException("[TestMessage] TestObject is not set.");
			set => MessageObject = value;
		}

		public static string CreateString()
		{
			return JsonConvert.SerializeObject(Create(), JsonConfig.Settings);
		}


		public static TestMessage Create()
		{
			return new TestMessage
			{
				MessageObject = TestObject.Create("MockMessage")
			};
		}
	}
}