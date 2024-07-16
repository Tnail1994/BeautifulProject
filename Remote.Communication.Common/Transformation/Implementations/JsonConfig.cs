using Newtonsoft.Json;

namespace Remote.Communication.Common.Transformation.Implementations
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