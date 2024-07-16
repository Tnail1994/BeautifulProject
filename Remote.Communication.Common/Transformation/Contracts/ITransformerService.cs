using Remote.Communication.Common.Transformation.Implementations;

namespace Remote.Communication.Common.Transformation.Contracts
{
	public interface ITransformerService : IDisposable
	{
		TransformedObject Transform(string jsonString);
	}
}