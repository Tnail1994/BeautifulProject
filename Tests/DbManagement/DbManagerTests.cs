﻿using BeautifulFundamental.Server.Db;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;
using Tests.TestObjects;

namespace Tests.DbManagement
{
	public class DbManagerTests
	{
		private IDbManager? Manager { get; set; }
		private readonly IDbContextResolver _dbContextResolverMock = Substitute.For<IDbContextResolver>();
		private readonly IMemoryCache _dbCacheMock = Substitute.For<IMemoryCache>();
		private readonly IDbSettings _dbSettingsMock = DbSettings.Default;

		[Fact]
		public void WhenDbManagerGetsInstantiated_ThenDbContextResolvers_GetMethod_ShouldCall()
		{
			Manager = new DbManager(_dbContextResolverMock, _dbSettingsMock, _dbCacheMock);

			_dbContextResolverMock.Received(1).Get();
		}

		[Fact]
		public void WhenDbResolverContainsData_ThenThisGetsCache_AndShouldCallSetOfCache()
		{
			var dbContextMock = Substitute.For<IDbContext>();
			var testEntities = new List<TestEntityDto>
			{
				new TestEntityDto()
			};
			dbContextMock.GetEntities().Returns(testEntities);

			_dbContextResolverMock.Get().Returns(new List<IDbContext> { dbContextMock });

			Manager = new DbManager(_dbContextResolverMock, _dbSettingsMock, _dbCacheMock);

			_dbCacheMock.Received().Set(Arg.Any<string>(), testEntities, new MemoryCacheEntryOptions()
				.SetSlidingExpiration(TimeSpan.FromSeconds(_dbSettingsMock.CachingTimeInSeconds)));
		}

		[Fact]
		public void WhenDbResolverContainsData_AndKeyIsCorrect_ThenGetEntities_ShouldNotBeEmpty()
		{
			var dbContextMock = Substitute.For<IDbContext>();
			var testEntities = new List<TestEntityDto>
			{
				new TestEntityDto()
			};
			dbContextMock.GetEntities().Returns(testEntities);
			dbContextMock.TypeNameOfCollectionEntries.Returns(nameof(TestEntityDto));
			_dbContextResolverMock.Get().Returns(new List<IDbContext> { dbContextMock });

			var cache = new MemoryCache(new MemoryCacheOptions());
			Manager = new DbManager(_dbContextResolverMock, _dbSettingsMock, cache);

			var getEntities = Manager.GetEntities<TestEntityDto>();

			Assert.NotNull(getEntities);
			Assert.NotEmpty(getEntities);
		}

		[Fact]
		public void WhenDbResolverDoesNotContainData_ThenGetEntities_ShouldBeEmpty()
		{
			var dbContextMock = Substitute.For<IDbContext>();

			dbContextMock.GetEntities().Returns((IEnumerable<object>?)null);

			_dbContextResolverMock.Get().Returns(new List<IDbContext> { dbContextMock });

			Manager = new DbManager(_dbContextResolverMock, _dbSettingsMock, _dbCacheMock);

			var getEntities = Manager.GetEntities<TestEntityDto>();
			_dbCacheMock.Received(1).TryGetValue<List<TestEntityDto>>(Arg.Any<string>(), out _);
			Assert.NotNull(getEntities);
			Assert.Empty(getEntities);
		}
	}
}