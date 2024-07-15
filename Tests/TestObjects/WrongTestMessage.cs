using Newtonsoft.Json;
using Remote.Core.Communication;
using Remote.Core.Transformation.Configurations;

namespace Tests.TestObjects
{
	public class WrongTestMessage : IBaseMessage
	{
		private static WrongTestMessage Create()
		{
			return new WrongTestMessage();
		}

		public static string CreateString()
		{
			return JsonConvert.SerializeObject(Create(), JsonConfig.Settings);
		}
	}
}