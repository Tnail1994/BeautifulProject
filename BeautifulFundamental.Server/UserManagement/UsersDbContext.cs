using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BeautifulFundamental.Server.Db;

namespace BeautifulFundamental.Server.UserManagement
{
	public class UsersDbContext : BaseDbContext<UserDto>, IUsersDbContext
	{
		public UsersDbContext(IDbContextSettings dbContextSettings) : base(dbContextSettings)
		{
		}

		protected override ReloadingBehavior GetReloadingBehavior()
		{
			return new ReloadingBehavior
			{
				ExceptWithEntities = true,
				ReloadLocals = true
			};
		}
	}

	[Table("Users")]
	public class UserDto : EntityDto
	{
		public UserDto(string name)
		{
			Name = name;
		}

		[Key] [Column("Name")] public string Name { get; set; }
		[Column("Active")] public bool IsActive { get; set; }
		[Column("StayActive")] public bool StayActive { get; set; }
		[Column("LastLoggedInDeviceIdent")] public string? LastLoggedInDeviceIdent { get; set; }
		[Column("ReactivateCounter")] public int ReactivateCounter { get; set; }

		public override bool Equals(object? obj)
		{
			if (obj is UserDto dto)
			{
				return dto.Name == Name;
			}

			return false;
		}

		public override int GetHashCode()
		{
			return Name.GetHashCode();
		}
	}
}