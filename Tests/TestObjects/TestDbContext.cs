using DbManagement.Common.Contracts;
using DbManagement.Common.Implementations;

namespace Tests.TestObjects
{
	public class TestDbContext : BaseDbContext<TestEntityDto>
	{
		public TestDbContext(IDbContextSettings dbContextSettings) : base(dbContextSettings)
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
}