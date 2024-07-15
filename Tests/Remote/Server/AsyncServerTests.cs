using Remote.Server;
using Remote.Server.Common.Contracts;

namespace Tests.Remote.Server
{
	public class AsyncServerTests
	{
		private readonly IAsyncServer _asyncSocketServer;

		public AsyncServerTests()
		{
			_asyncSocketServer = new AsyncServer();
		}
	}
}