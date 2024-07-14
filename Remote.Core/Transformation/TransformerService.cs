using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Remote.Core.Communication;

namespace Remote.Core.Transformation
{
	public interface ITransformerService
	{
		TransformedObject Transform(string jsonString);
	}

	public class TransformerService : ITransformerService
	{
		private readonly Dictionary<string, Type> _typeMap = new Dictionary<string, Type>();
		private readonly Dictionary<string, MethodInfo> _methodCache = new Dictionary<string, MethodInfo>();

		public TransformerService(IServiceProvider serviceProvider)
		{
			RegisterAllBaseMessages(serviceProvider);
		}

		private void RegisterAllBaseMessages(IServiceProvider serviceProvider)
		{
			var baseMessageTypes = serviceProvider.GetServices<IBaseMessage>()
				.Select(m => m.GetType())
				.Where(t => t.BaseType?.IsGenericType == true &&
				            t.BaseType.GetGenericTypeDefinition() == typeof(BaseMessage<>));

			foreach (var type in baseMessageTypes)
			{
				var typeName = type.Name;
				_typeMap[typeName] = type;
				var methodInfo = type.GetMethod("Transform",
					BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

				if (methodInfo == null)
				{
					Console.WriteLine($"No methodInfo found for {typeName}");
					continue;
				}

				_methodCache[typeName] = methodInfo;
			}
		}

		public TransformedObject Transform(string json)
		{
			Console.WriteLine("Start transforming object...");

			var jObject = JObject.Parse(json);
			var discriminator = jObject["$type"]?.ToString().Split(',')[0].Split('.').Last();

			if (string.IsNullOrEmpty(discriminator) || !_typeMap.TryGetValue(discriminator, out var type))
			{
				throw new InvalidOperationException($"No type registered for discriminator: {discriminator}");
			}

			if (!_methodCache.TryGetValue(discriminator, out var method))
			{
				throw new InvalidOperationException($"Transform method not found for type: {discriminator}");
			}

			var invokeResult = method.Invoke(null, new object[] { json });

			if (invokeResult == null)
				throw new InvalidOperationException($"Invoke result is null for type: {discriminator}");

			return TransformedObject.Create(invokeResult, discriminator);
		}
	}
}