using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Remote.Server;

namespace Tests.Remote.Server
{
	public class AsyncServerTests
	{
		private readonly AsyncServer _asyncSocketServer;

		public AsyncServerTests()
		{
			_asyncSocketServer = new AsyncServer();
		}
	}
}