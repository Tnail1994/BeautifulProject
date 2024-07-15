using Newtonsoft.Json;
using Remote.Core.Communication;
using Remote.Core.Implementations;

namespace Tests.TestObjects
{
	public class WrongTestMessage : IBaseMessage
	{
		public WrongTestMessage()
		{
		}

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