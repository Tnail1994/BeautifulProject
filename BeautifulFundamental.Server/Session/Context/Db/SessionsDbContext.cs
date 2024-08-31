using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BeautifulFundamental.Server.Db;
using BeautifulFundamental.Server.Session.Implementations;

namespace BeautifulFundamental.Server.Session.Context.Db
{
	public interface ISessionsDbContext;

	public interface ISessionDataProvider
	{
		bool TryGetSessionState(string sessionId, out SessionState sessionState);
	}

	public class SessionsDbContext : BaseDbContext<SessionInfoDto>, ISessionsDbContext, ISessionDataProvider
	{
		public SessionsDbContext(IDbContextSettings dbContextSettings) : base(dbContextSettings)
		{
		}

		protected override ReloadingBehavior GetReloadingBehavior()
		{
			return new ReloadingBehavior
			{
				ExceptWithEntities = true,
			};
		}

		protected override bool CustomMissingEntriesFilter(SessionInfoDto entry)
		{
			return entry.SessionState.Equals((int)SessionState.Stopped);
		}

		public bool TryGetSessionState(string sessionId, out SessionState sessionState)
		{
			var foundEntity = GetEntities().Cast<SessionInfoDto>()
				.FirstOrDefault(entity => entity.Id.Equals(sessionId));

			if (foundEntity == null)
			{
				sessionState = SessionState.None;
				return false;
			}

			sessionState = (SessionState)foundEntity.SessionState;
			return true;
		}
	}

	[Table("Sessions")]
	public class SessionInfoDto : EntityDto
	{
		public SessionInfoDto(string id, string username, int sessionState, bool authorized)
		{
			Id = id;
			Username = username;
			SessionState = sessionState;
			Authorized = authorized;
		}

		[Key] [Column("Id")] public string Id { get; set; }
		[Column("Username")] public string Username { get; set; }
		[Column("Authorized")] public bool Authorized { get; set; }
		[Column("SessionState")] public int SessionState { get; set; }

		public override bool Equals(object? obj)
		{
			return obj is SessionInfoDto dto
			       && dto.Id == Id
			       && dto.Username == Username
			       && dto.SessionState == SessionState
			       && dto.Authorized == Authorized;
		}

		public override int GetHashCode()
		{
			return Id.GetHashCode() ^ Username.GetHashCode() ^ SessionState.GetHashCode() ^ Authorized.GetHashCode();
		}
	}
}