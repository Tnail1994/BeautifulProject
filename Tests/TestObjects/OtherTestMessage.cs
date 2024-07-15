using Newtonsoft.Json;
using Remote.Core.Communication;
using Remote.Core.Implementations;

namespace Tests.TestObjects
{
	public class OtherTestMessage : BaseMessage<TestObject>
	{
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