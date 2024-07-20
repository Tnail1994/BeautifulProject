﻿using DbManagement.Common.Contracts;
using DbManagement.Common.Implementations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DbManagement.Contexts
{
	public class UsersDbContext : BaseDbContext<UserDto>
	{
		public UsersDbContext(IDbContextSettings dbContextSettings) : base(dbContextSettings)
		{
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