using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DbManagement.Common.Contracts;
using DbManagement.Common.Implementations;

namespace Session.Contexts
{
	public class SessionsDbContext : BaseDbContext<SessionInfoDto>
	{
		public SessionsDbContext(IDbContextSettings dbContextSettings) : base(dbContextSettings)
		{
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