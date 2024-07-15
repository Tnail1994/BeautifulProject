﻿using Newtonsoft.Json;
using Remote.Core.Communication;
using Remote.Core.Transformation.Configurations;

namespace Tests.TestObjects
{
	public class TestMessage : BaseMessage<TestObject>
	{
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