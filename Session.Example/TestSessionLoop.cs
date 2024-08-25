using BeautifulFundamental.Core.Communication;
using BeautifulFundamental.Core.Identification;
using BeautifulFundamental.Core.Messages.CheckAlive;
using BeautifulFundamental.Server.Session.Context;
using BeautifulFundamental.Server.Session.Contracts.Context.Db;
using BeautifulFundamental.Server.Session.Core;

namespace Session.Example
{
	public class TestSessionLoop : SessionLoopBase
	{
		private readonly ITurnDetails _turnDetails;
		private readonly IRoundDetails _roundDetails;
		private readonly ICurrentPlayerDetails _currentPlayerDetails;
		private readonly ICommunicationService _communicationService;

		public TestSessionLoop(IIdentificationKey identificationKey, ITurnDetails turnDetails,
			IRoundDetails roundDetails,
			ICurrentPlayerDetails currentPlayerDetails,
			ICommunicationService communicationService) : base(identificationKey)
		{
			_turnDetails = turnDetails;
			_roundDetails = roundDetails;
			_currentPlayerDetails = currentPlayerDetails;
			_communicationService = communicationService;
		}

		protected override async void Run()
		{
			try
			{
				var x = await _communicationService.ReceiveAsync<CheckAliveRequest>();
				_turnDetails.UpdateTurnCounter(9);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}
	}

	public class TurnDetails : SessionDetail, ITurnDetails
	{
		public int TurnCounter { get; private set; }

		public void UpdateTurnCounter(int i)
		{
			TurnCounter = i;
			TriggerUpdate();
		}

		public TurnDetails(IIdentificationKey identificationKey) : base(identificationKey)
		{
		}

		public TurnDetails(IIdentificationKey identificationKey, int turnCounter) : base(identificationKey)
		{
			TurnCounter = turnCounter;
		}

		public override IEntryDto Convert()
		{
			return new TurnContextEntryDto(SessionId, TurnCounter);
		}
	}

	public interface ITurnDetails
	{
		int TurnCounter { get; }
		void UpdateTurnCounter(int i);
	}

	public class RoundDetails : SessionDetail, IRoundDetails
	{
		public int RoundCounter { get; }

		public RoundDetails(IIdentificationKey identificationKey) : base(identificationKey)
		{
			RoundCounter = 0;
		}

		public RoundDetails(IIdentificationKey identificationKey, int roundCounter) : base(identificationKey)
		{
			RoundCounter = roundCounter;
		}

		public override IEntryDto Convert()
		{
			return new RoundContextEntryDto(SessionId, RoundCounter);
		}
	}

	public interface IRoundDetails
	{
		int RoundCounter { get; }
	}

	public class CurrentPlayerDetails : SessionDetail, ICurrentPlayerDetails
	{
		public string PlayerName { get; }


		public CurrentPlayerDetails(IIdentificationKey identificationKey) : base(identificationKey)
		{
			PlayerName = string.Empty;
		}

		public CurrentPlayerDetails(IIdentificationKey identificationKey, string playerName) : base(identificationKey)
		{
			PlayerName = playerName;
		}

		public bool PlayerNameSet => !string.IsNullOrEmpty(PlayerName);

		public override IEntryDto Convert()
		{
			return new CurrentPlayerContextEntryDto(SessionId, PlayerName);
		}
	}

	public interface ICurrentPlayerDetails
	{
		string PlayerName { get; }
		bool PlayerNameSet { get; }
	}
}