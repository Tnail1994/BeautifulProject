using SharedBeautifulServices.Common;

namespace SharedBeautifulServices
{
	public class CheckAliveService
	{
		private readonly ICheckAliveSettings _settings;

		public CheckAliveService(ICheckAliveSettings settings)
		{
			_settings = settings;
		}
	}
}