using Remote.Communication.Common.Contracts;
using Session.Common.Contracts.Context.Db;
using Session.Common.Implementations;
using Session.Context;
using Session.Core;
using SharedBeautifulData.Messages.CheckAlive;

namespace Session.Example
{
    public class TestSessionLoop : SessionLoopBase
	{
		private readonly ITurnDetails _turnDetails;
		private readonly IRoundDetails _roundDetails;
		private readonly ICurrentPlayerDetails _currentPlayerDetails;
		private readonly ICommunicationService _communicationService;

		public TestSessionLoop(ISessionKey sessionKey, ITurnDetails turnDetails, IRoundDetails roundDetails,
			ICurrentPlayerDetails currentPlayerDetails, ICommunicationService communicationService) : base(sessionKey)
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

		public TurnDetails(ISessionKey sessionKey) : base(sessionKey)
		{
		}

		public TurnDetails(ISessionKey sessionKey, int turnCounter) : base(sessionKey)
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

		public RoundDetails(ISessionKey sessionKey) : base(sessionKey)
		{
			RoundCounter = 0;
		}

		public RoundDetails(ISessionKey sessionKey, int roundCounter) : base(sessionKey)
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


		public CurrentPlayerDetails(ISessionKey sessionKey) : base(sessionKey)
		{
			PlayerName = string.Empty;
		}

		public CurrentPlayerDetails(ISessionKey sessionKey, string playerName) : base(sessionKey)
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