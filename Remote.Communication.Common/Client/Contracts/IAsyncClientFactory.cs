using System.Net.Security;
using System.Net.Sockets;

namespace Remote.Communication.Common.Client.Contracts
{
	public interface IAsyncClientFactory
	{
		void Init(TcpClient? client = null, SslStream? sslStream = null, bool isServerClient = true);

		IAsyncClient Create();
	}
}