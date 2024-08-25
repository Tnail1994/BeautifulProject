using System.Security.Cryptography.X509Certificates;

namespace BeautifulFundamental.Core.Communication.Helpers
{
	public static class CertificateCreator
	{
		public static X509Certificate2 Create(string certificatePath)
		{
			if (!Directory.Exists(certificatePath))
			{
				var currentDirectory = Directory.GetCurrentDirectory();
				certificatePath = Path.Combine(currentDirectory, certificatePath);
			}

			var serverCertificate = new X509Certificate2(certificatePath);
			return serverCertificate;
		}

		public static X509CertificateCollection CreateAsCollection(string certificatePath)
		{
			return new X509CertificateCollection()
			{
				Create(certificatePath)
			};
		}
	}
}