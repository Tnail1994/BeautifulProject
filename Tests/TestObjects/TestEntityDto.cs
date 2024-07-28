using DbManagement.Common.Implementations;

namespace Tests.TestObjects
{
	public class TestEntityDto : EntityDto
	{
		// ReSharper disable once EmptyConstructor
		public TestEntityDto()
		{
		}

		public override bool Equals(object? obj)
		{
			return false;
		}

		public override int GetHashCode()
		{
			return 0;
		}
	}
}