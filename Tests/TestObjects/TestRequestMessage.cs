using Newtonsoft.Json;
using Remote.Communication.Common.Implementations;
using Remote.Communication.Common.Transformation.Implementations;


namespace Tests.TestObjects
{
	public class TestRequestMessage : NetworkMessage<TestObject>, IRequestMessage
	{
		public static string CreateString()
		{
			return JsonConvert.SerializeObject(Create(), JsonConfig.Settings);
		}


		public static TestRequestMessage Create()
		{
			return new TestRequestMessage
			{
				MessageObject = TestObject.Create("MockRequestMessage")
			};
		}
	}
}