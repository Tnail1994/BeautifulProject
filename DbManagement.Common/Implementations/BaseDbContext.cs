using System.Collections.Concurrent;
using Core.Extensions;
using Core.Helpers;
using DbManagement.Common.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DbManagement.Common.Implementations
{
	public abstract class BaseDbContext<T> : DbContext, IDbContext where T : EntityDto
	{
		enum UpdateType
		{
			Add,
			Update,
			Delete
		}

		private class UpdateEntityElement
		{
			private UpdateEntityElement(T entityDto, UpdateType type)
			{
				EntityDto = entityDto;
				Type = type;
			}

			public static UpdateEntityElement Create(T entityDto, UpdateType updateType) => new(entityDto, updateType);

			public T EntityDto { get; set; }

			public UpdateType Type { get; set; }
		}

		private readonly IDbContextSettings _dbContextSettings;

		private readonly ConcurrentQueue<UpdateEntityElement> _updateQueue = new();
		private readonly CancellationTokenSource _updateLoopCts = new();

		private List<T>? _set;

		protected BaseDbContext(IDbContextSettings dbContextSettings)
		{
			Id = GuidIdCreator.CreateString();

			_dbContextSettings = dbContextSettings;
			TypeNameOfCollectionEntries = typeof(T).Name;

			RunUpdateLoop();
		}

		private void RunUpdateLoop()
		{
			Task.Factory.StartNew(async () =>
			{
				while (!_updateLoopCts.IsCancellationRequested)
				{
					await Task.Delay(_dbContextSettings.UpdateDbDelayInMs);
					await SaveChangesFromUpdateLoop();
				}
			}, _updateLoopCts.Token);
		}

		private async Task SaveChangesFromUpdateLoop(bool skipCts = false)
		{
			if (Entities == null)
			{
				this.LogWarning($"DbSet of {TypeNameOfCollectionEntries} is null");
				return;
			}

			var hasChanges = true;

			// todo: how to add analyzing of the updateQueue? 
			if (_dbContextSettings.AnalyzeUpdateSet)
			{
			}

			while (!_updateQueue.IsEmpty && (!_updateLoopCts.IsCancellationRequested || skipCts))
			{
				if (!_updateQueue.TryDequeue(out var updateEntityElement))
					continue;

				switch (updateEntityElement.Type)
				{
					case UpdateType.Add:
						this.LogDebug($"Adding type {typeof(T)}, with {updateEntityElement.EntityDto}");
						await Entities.AddAsync(updateEntityElement.EntityDto);
						hasChanges = true;
						break;
					case UpdateType.Delete:
						this.LogDebug($"Removing type {typeof(T)}, with {updateEntityElement.EntityDto}");
						Entities.Remove(updateEntityElement.EntityDto);
						hasChanges = true;
						break;
					case UpdateType.Update:
						hasChanges = true;
						break;
				}
			}

			if (hasChanges)
				await SaveChangesAsync();
		}


		protected DbSet<T>? Entities { get; set; }


		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder
			.UseNpgsql(
				$@"Server={_dbContextSettings.ServerAdresse};Port={_dbContextSettings.Port};UserId={_dbContextSettings.UserId};Password={_dbContextSettings.Password};Database={_dbContextSettings.DatabaseName};",
				options => options.EnableRetryOnFailure())
			.LogTo(Console.WriteLine, LogLevel.Information);

		public string Id { get; }
		public string TypeNameOfCollectionEntries { get; }

		public IEnumerable<TDto> GetEntities<TDto>() where TDto : EntityDto
		{
			if (_set == null)
			{
				this.LogError($"Cannot get entities because Set is not initialized ");
				return Enumerable.Empty<TDto>();
			}

			return _set.Cast<TDto>();
		}

		public void AddEntity<TDto>(TDto dto) where TDto : EntityDto
		{
			var entityDto = dto as T;

			if (entityDto == null)
			{
				this.LogWarning($"Cannot add because dto is not {typeof(T)}");
				return;
			}

			if (_set == null)
			{
				this.LogError($"(_set) Cannot add because Set is not initialized ");
				return;
			}

			if (!_set.Contains(entityDto))
			{
				this.LogDebug($"(_set) Adding type {typeof(T)}, with {entityDto}");
				Add(entityDto);
			}
			else
			{
				this.LogDebug($"(_set) Updating type {typeof(T)}, with {entityDto}");
				Update(entityDto);
			}
		}


		private void Add(T entityDto)
		{
			if (_set == null)
				return;

			_set.Add(entityDto);
			_updateQueue.Enqueue(UpdateEntityElement.Create(entityDto, UpdateType.Add));
		}

		private void Update(T entityDto)
		{
			if (_set == null)
				return;

			_updateQueue.Enqueue(UpdateEntityElement.Create(entityDto, UpdateType.Update));
		}

		public IEnumerable<object>? GetEntities()
		{
			_set ??= Entities?.ToList();
			return _set;
		}

		public override async ValueTask DisposeAsync()
		{
			_updateLoopCts.Cancel();
			await SaveChangesFromUpdateLoop(true);
			await base.DisposeAsync();
		}
	}
}