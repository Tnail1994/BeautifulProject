using Newtonsoft.Json;
using Remote.Core.Communication;
using Remote.Core.Implementations;

namespace Tests.TestObjects
{
	public class TestMessage : BaseMessage<Test>
	{
		public static string CreateString()
		{
			return JsonConvert.SerializeObject(Create(), JsonConfig.Settings);
		}


		public static TestMessage Create()
		{
			return new TestMessage
			{
				MessageObject = Test.Create("MockMessage")
			};
		}
	}
}