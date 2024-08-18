﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Core.Extensions;
using DbManagement.Common.Contracts;
using DbManagement.Common.Implementations;
using Session.Common.Contracts.Context.Db;
using Session.Common.Implementations;

namespace Session.Context.Db
{
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

		protected override Task HandleNewEntries(List<SessionInfoDto> newEntries)
		{
			foreach (var newEntry in newEntries)
			{
				if (newEntry.SessionState != (int)SessionState.Stopped)
					continue;

				AddToSet(newEntry);
			}

			return Task.CompletedTask;
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