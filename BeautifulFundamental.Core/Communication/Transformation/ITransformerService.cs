using BeautifulFundamental.Core.Communication.Transformation.Implementations;

namespace BeautifulFundamental.Core.Communication.Transformation
{
	public interface ITransformerService : IDisposable
	{
		TransformedObject Transform(string jsonString);
	}
}