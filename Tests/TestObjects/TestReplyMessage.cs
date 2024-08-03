using Newtonsoft.Json;
using Remote.Communication.Common.Implementations;
using Remote.Communication.Common.Transformation.Implementations;

namespace Tests.TestObjects
{
	public class TestReplyMessage : NetworkMessage<TestObject>, IReplyMessage
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