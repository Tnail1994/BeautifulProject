namespace Remote.Communication.Common.Client.Contracts
{
	public interface ITlsSettings
	{
		bool CertificateRequired { get; init; }
		bool CheckCertificateRevocation { get; init; }
		string CertificatePath { get; init; }
		bool LeaveInnerStreamOpen { get; init; }
		string TargetHost { get; init; }
	}
}