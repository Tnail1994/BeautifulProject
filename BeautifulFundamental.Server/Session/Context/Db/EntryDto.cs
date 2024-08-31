using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BeautifulFundamental.Core.Identification;
using BeautifulFundamental.Server.Db;

namespace BeautifulFundamental.Server.Session.Context.Db
{
	public interface IEntryDto
	{
		string TypeName { get; }
		ISessionDetail Convert(IIdentificationKey identificationKey);
		void Update(ISessionDetail sessionDetail);
	}

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

		public abstract ISessionDetail Convert(IIdentificationKey identificationContextIdentificationKey);
		public abstract void Update(ISessionDetail sessionDetail);
	}
}