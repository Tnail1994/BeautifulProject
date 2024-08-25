using BeautifulFundamental.Core.Communication.Implementations;
using BeautifulFundamental.Core.Communication.Transformation.Implementations;
using Newtonsoft.Json;

namespace Tests.TestObjects
{
	public class TestReplyMessage : NetworkMessage<TestObject>
	{
		public static string CreateString()
		{
			return JsonConvert.SerializeObject(Create(), JsonConfig.Settings);
		}


		public static TestReplyMessage Create()
		{
			return new TestReplyMessage
			{
				MessageObject = TestObject.Create("MockReplyMessage")
			};
		}
	}
}