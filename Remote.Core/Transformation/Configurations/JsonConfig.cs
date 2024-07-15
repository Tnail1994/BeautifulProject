using Newtonsoft.Json;

namespace Remote.Core.Transformation.Configurations
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