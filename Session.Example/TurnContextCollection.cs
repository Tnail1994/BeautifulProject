using System.ComponentModel.DataAnnotations.Schema;
using DbManagement.Common.Contracts;
using Session.Common.Contracts.Context;
using Session.Common.Implementations;
using Session.Context.Db;

namespace Session.Example
{
	public class TurnContextCollection : ContextCollection<TurnContextEntryDto>
	{
		public TurnContextCollection(IDbContextSettings dbContextSettings) : base(dbContextSettings)
		{
		}

		protected override ReloadingBehavior GetReloadingBehavior()
		{
			return new ReloadingBehavior
			{
				ExceptWithEntities = true
			};
		}
	}

	public class RoundContextCollection : ContextCollection<RoundContextEntryDto>
	{
		public RoundContextCollection(IDbContextSettings dbContextSettings) : base(dbContextSettings)
		{
		}

		protected override ReloadingBehavior GetReloadingBehavior()
		{
			return new ReloadingBehavior
			{
				ExceptWithEntities = true,
				ReloadLocals = true,
			};
		}
	}

	public class CurrentPlayerContextCollection : ContextCollection<CurrentPlayerContextEntryDto>
	{
		public CurrentPlayerContextCollection(IDbContextSettings dbContextSettings) : base(dbContextSettings)
		{
		}

		protected override ReloadingBehavior GetReloadingBehavior()
		{
			return new ReloadingBehavior
			{
				ExceptWithEntities = true
			};
		}
	}

	[Table("CurrentPlayerContexts")]
	public class CurrentPlayerContextEntryDto : EntryDto
	{
		public CurrentPlayerContextEntryDto(string sessionId, string playerName) : base(sessionId)
		{
			PlayerName = playerName;
		}

		[Column("Playername")] public string PlayerName { get; set; }

		public override bool Equals(object? obj)
		{
			return base.Equals(obj) &&
			       obj is CurrentPlayerContextEntryDto dto
			       && dto.PlayerName == PlayerName;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode() ^ PlayerName.GetHashCode();
		}

		public override ISessionDetail Convert(ISessionKey sessionKey)
		{
			return new CurrentPlayerDetails(sessionKey, PlayerName);
		}

		public override void Update(ISessionDetail sessionDetail)
		{
			if (sessionDetail is ICurrentPlayerDetails currentPlayerDetails)
			{
				PlayerName = currentPlayerDetails.PlayerName;
			}
		}
	}


	[Table("TurnContexts")]
	public class TurnContextEntryDto : EntryDto
	{
		public TurnContextEntryDto(string sessionId, int turnCounter) : base(sessionId)
		{
			TurnCounter = turnCounter;
		}

		[Column("TurnCounter")] public int TurnCounter { get; set; }

		public override bool Equals(object? obj)
		{
			return base.Equals(obj) &&
			       obj is TurnContextEntryDto dto
			       && dto.TurnCounter == TurnCounter;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode() ^ TurnCounter.GetHashCode();
		}

		public override ISessionDetail Convert(ISessionKey sessionKey)
		{
			return new TurnDetails(sessionKey, TurnCounter);
		}

		public override void Update(ISessionDetail sessionDetail)
		{
			if (sessionDetail is ITurnDetails turnDetails)
			{
				TurnCounter = turnDetails.TurnCounter;
			}
		}
	}


	[Table("RoundContexts")]
	public class RoundContextEntryDto : EntryDto
	{
		public RoundContextEntryDto(string sessionId, int roundCounter) : base(sessionId)
		{
			RoundCounter = roundCounter;
		}

		[Column("RoundCounter")] public int RoundCounter { get; set; }

		public override bool Equals(object? obj)
		{
			return base.Equals(obj) && obj is RoundContextEntryDto dto
			                        && dto.RoundCounter == RoundCounter;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode() ^ RoundCounter.GetHashCode();
		}

		public override ISessionDetail Convert(ISessionKey sessionKey)
		{
			return new RoundDetails(sessionKey, RoundCounter);
		}

		public override void Update(ISessionDetail sessionDetail)
		{
			if (sessionDetail is IRoundDetails roundDetails)
			{
				RoundCounter = roundDetails.RoundCounter;
			}
		}
	}
}