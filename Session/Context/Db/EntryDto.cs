using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DbManagement.Common.Implementations;
using Session.Common.Contracts.Context;
using Session.Common.Contracts.Context.Db;
using Session.Common.Implementations;

namespace Session.Context.Db
{
    public abstract class EntryDto : EntityDto, IEntryDto
	{
		public EntryDto(string sessionId)
		{
			SessionId = sessionId;
		}

		[Key] [Column("SessionId")] public string SessionId { get; set; }

		public override bool Equals(object? obj)
		{
			return obj is EntryDto dto
			       && dto.SessionId == SessionId;
		}

		public override int GetHashCode()
		{
			return SessionId.GetHashCode();
		}

		public abstract ISessionDetail Convert(ISessionKey sessionContextSessionKey);
		public abstract void Update(ISessionDetail sessionDetail);
	}
}