using Session.Common.Implementations;
using Session.Core;

namespace Session.Example
{
	public class TestSessionLoop : SessionLoopBase
	{
		private readonly ITurnDetails _turnDetails;
		private readonly IRoundDetails _roundDetails;
		private readonly ICurrentPlayerDetails _currentPlayerDetails;

		public TestSessionLoop(ISessionKey sessionKey, ITurnDetails turnDetails, IRoundDetails roundDetails,
			ICurrentPlayerDetails currentPlayerDetails) : base(sessionKey)
		{
			_turnDetails = turnDetails;
			_roundDetails = roundDetails;
			_currentPlayerDetails = currentPlayerDetails;
		}

		protected override void Run()
		{
		}
	}

	public class TurnDetails : SessionDetail, ITurnDetails
	{
		public int TurnCounter { get; }

		public TurnDetails(string sessionId) : base(sessionId)
		{
		}

		public TurnDetails(string sessionId, int turnCounter) : base(sessionId)
		{
			TurnCounter = turnCounter;
		}
	}

	public interface ITurnDetails
	{
		int TurnCounter { get; }
	}

	public class RoundDetails : SessionDetail, IRoundDetails
	{
		public int RoundCounter { get; }

		public RoundDetails(string sessionId) : base(sessionId)
		{
			RoundCounter = 0;
		}

		public RoundDetails(string sessionId, int roundCounter) : base(sessionId)
		{
			RoundCounter = roundCounter;
		}
	}

	public interface IRoundDetails
	{
		int RoundCounter { get; }
	}

	public class CurrentPlayerDetails : SessionDetail, ICurrentPlayerDetails
	{
		public string PlayerName { get; }


		public CurrentPlayerDetails(string sessionId) : base(sessionId)
		{
			PlayerName = string.Empty;
		}

		public CurrentPlayerDetails(string sessionId, string playerName) : base(sessionId)
		{
			PlayerName = playerName;
		}

		public bool PlayerNameSet => !string.IsNullOrEmpty(PlayerName);
	}

	public interface ICurrentPlayerDetails
	{
		string PlayerName { get; }
		bool PlayerNameSet { get; }
	}
}