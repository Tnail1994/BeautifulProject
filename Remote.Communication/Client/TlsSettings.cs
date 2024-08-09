using Remote.Communication.Common.Client.Contracts;

namespace Remote.Communication.Client
{
	public class TlsSettings : ITlsSettings
	{
		private const string DefaultCertificatePath = "anyPath";
		private const bool DefaultCertificateRequired = true;
		private const bool DefaultCheckCertificateRevocation = true;
		private const string DefaultTargetHost = "localhost";

		public bool CertificateRequired { get; init; } = DefaultCertificateRequired;
		public bool CheckCertificateRevocation { get; init; } = DefaultCheckCertificateRevocation;
		public string CertificatePath { get; init; } = DefaultCertificatePath;
		public bool LeaveInnerStreamOpen { get; init; }
		public bool AllowRemoteCertificateChainErrors { get; init; };
		public string TargetHost { get; init; } = DefaultTargetHost;
		public static ITlsSettings Default => new TlsSettings();
	}
}