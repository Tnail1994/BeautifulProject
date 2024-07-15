using Configurations.General.Settings;
using Microsoft.Extensions.Options;
using NSubstitute;
using Remote.Server;
using Remote.Server.Common.Contracts;

namespace Tests.Remote.Server
{
	public class AsyncServerTests
	{
		private readonly IAsyncServer _asyncSocketServer;

		public AsyncServerTests()
		{
			var optionsMock = Substitute.For<IOptions<AsyncServerSettings>>();
			optionsMock.Value.Returns(AsyncServerSettings.Default);
			_asyncSocketServer = new AsyncServer(optionsMock);
		}
	}
}