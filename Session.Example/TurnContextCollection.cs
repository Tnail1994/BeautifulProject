using System.ComponentModel.DataAnnotations.Schema;
using BeautifulFundamental.Core.Identification;
using BeautifulFundamental.Server.Db;
using BeautifulFundamental.Server.Session.Context;
using BeautifulFundamental.Server.Session.Context.Db;

namespace Session.Example
{
	public class TurnContextCollection : ContextCollection<TurnContextEntryDto>, ITurnContextCollection
	{
		public TurnContextCollection(IDbContextSettings dbContextSettings, SessionsDbContext sessionDataProvider) :
			base(
				dbContextSettings, sessionDataProvider)
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

	public interface ITurnContextCollection;

	public class RoundContextCollection : ContextCollection<RoundContextEntryDto>, IRoundContextCollection
	{
		public RoundContextCollection(IDbContextSettings dbContextSettings, SessionsDbContext sessionDataProvider) :
			base(
				dbContextSettings, sessionDataProvider)
		{
		}

		protected override ReloadingBehavior GetReloadingBehavior()
		{
			return new ReloadingBehavior
			{
				ExceptWithEntities = true,
			};
		}
	}

	public interface IRoundContextCollection;

	public class CurrentPlayerContextCollection : ContextCollection<CurrentPlayerContextEntryDto>,
		ICurrentPlayerContextCollection
	{
		public CurrentPlayerContextCollection(IDbContextSettings dbContextSettings,
			SessionsDbContext sessionDataProvider) :
			base(
				dbContextSettings, sessionDataProvider)
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

	public interface ICurrentPlayerContextCollection;

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

		public override ISessionDetail Convert(IIdentificationKey identificationKey)
		{
			return new CurrentPlayerDetails(identificationKey, PlayerName);
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

		public override ISessionDetail Convert(IIdentificationKey identificationKey)
		{
			return new TurnDetails(identificationKey, TurnCounter);
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

		public override ISessionDetail Convert(IIdentificationKey identificationKey)
		{
			return new RoundDetails(identificationKey, RoundCounter);
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