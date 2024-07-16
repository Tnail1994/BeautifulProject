using Newtonsoft.Json;
using Remote.Communication.Common.Implementations;
using Remote.Communication.Common.Transformation.Implementations;

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