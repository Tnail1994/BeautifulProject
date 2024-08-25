using BeautifulFundamental.Core.Communication.Implementations;
using BeautifulFundamental.Core.Communication.Transformation.Implementations;
using Newtonsoft.Json;


namespace Tests.TestObjects
{
	public class TestRequestMessage : NetworkMessage<TestObject>
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