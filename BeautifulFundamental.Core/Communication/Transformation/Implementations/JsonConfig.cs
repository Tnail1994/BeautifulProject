using Newtonsoft.Json;

namespace BeautifulFundamental.Core.Communication.Transformation.Implementations
{
	public static class JsonConfig
	{
		public static JsonSerializerSettings Settings => new()
		{
			TypeNameHandling = TypeNameHandling.Objects,
			TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple
		};
	}
}