using BeautifulFundamental.Core.Communication.Implementations;
using BeautifulFundamental.Core.Communication.Transformation;
using BeautifulFundamental.Core.Communication.Transformation.Implementations;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Tests.TestObjects;

namespace Tests.Remote.Communication.Transformation
{
	public class TransformerServiceTests
	{
		private IServiceProvider? _dummyProvider;
		private readonly IServiceCollection _dummyServiceCollection = new ServiceCollection();

		[Fact]
		public void WhenNoSuitableBaseMessagesAddedToServiceCollection_ThenTransformCall_ShouldRaiseTransformException()
		{
			//_dummyServiceCollection.AddTransient<INetworkMessage, TestMessage>();
			_dummyProvider = _dummyServiceCollection.BuildServiceProvider();
			var transformerService = new TransformerService(_dummyProvider);
			Assert.Throws<TransformException>(() =>
				transformerService.Transform(TestMessage.CreateString()));
		}

		[Fact]
		public void
			WhenSuitableBaseMessagesAddedToServiceCollection_ThenTransformCall_ShouldGiveBackTransformedObject_WithSameTestMessage()
		{
			_dummyServiceCollection.AddTransient<INetworkMessage, TestMessage>();
			_dummyProvider = _dummyServiceCollection.BuildServiceProvider();
			var transformerService = new TransformerService(_dummyProvider);
			var transformedObject = transformerService.Transform(TestMessage.CreateString());
			Assert.NotNull(transformedObject);
			Assert.NotNull((TestMessage)transformedObject.Object);
			Assert.NotNull(((TestMessage)transformedObject.Object).TestObject);
			Assert.NotNull(((TestMessage)transformedObject.Object).TestObject.MockObj);
			Assert.Equal(TestMessage.Create().TestObject.MockObj,
				((TestMessage)transformedObject.Object).TestObject.MockObj);
		}

		[Fact]
		public void WhenInputNotJsonFormat_InsideTransform_ThenShouldRaiseJsonReaderException()
		{
			_dummyServiceCollection.AddTransient<INetworkMessage, TestMessage>();
			_dummyProvider = _dummyServiceCollection.BuildServiceProvider();
			var transformerService = new TransformerService(_dummyProvider);
			Assert.Throws<JsonReaderException>(() =>
				transformerService.Transform("No JSON format"));
		}

		[Fact]
		public void
			WhenInputJsonFormat_ButWithWrongSerializerSettings_InsideTransform_ThenShouldRaiseTransformException()
		{
			_dummyServiceCollection.AddTransient<INetworkMessage, TestMessage>();
			_dummyServiceCollection.AddTransient<INetworkMessage, TestMessage>();
			_dummyProvider = _dummyServiceCollection.BuildServiceProvider();
			var transformerService = new TransformerService(_dummyProvider);
			var testMessage = TestMessage.Create();
			Assert.Throws<TransformException>(() =>
				transformerService.Transform(JsonConvert.SerializeObject(testMessage)));
		}

		[Fact]
		public void WhenWrongMessageIsAddedToServiceCollection_InsideTransform_ThenShouldRaiseTransformException()
		{
			_dummyServiceCollection.AddTransient<INetworkMessage, WrongTestMessage>();
			_dummyProvider = _dummyServiceCollection.BuildServiceProvider();
			var transformerService = new TransformerService(_dummyProvider);
			Assert.Throws<TransformException>(() =>
				transformerService.Transform(WrongTestMessage.CreateString()));
		}
	}
}