namespace AutoSynchronizedMessageHandling.Common.Implementations
{
	public enum AutoSyncType
	{
		/// <summary>
		/// Is the application (like ui) context.
		/// </summary>
		Main = 0,

		/// <summary>
		/// Is the async context, where it comes from the async receive handling.
		/// Be careful using thread unsafe collections.
		/// </summary>
		This = 1,

		Custom = 2,
	}
}