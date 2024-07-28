namespace DbManagement.Common.Implementations
{
	public abstract class EntityDto
	{
		public new abstract bool Equals(object? obj);
		public new abstract int GetHashCode();
	}
}