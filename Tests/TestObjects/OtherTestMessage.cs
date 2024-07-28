using Newtonsoft.Json;
using Remote.Communication.Common.Implementations;
using Remote.Communication.Common.Transformation.Implementations;

namespace Tests.TestObjects
{
	public class OtherTestMessage : BaseMessage<TestObject>
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