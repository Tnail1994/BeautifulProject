﻿using System.Collections.Concurrent;
using BeautifulFundamental.Core.Extensions;
using BeautifulFundamental.Core.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BeautifulFundamental.Server.Db
{
	public abstract class BaseDbContext<T> : DbContext, IDbContext where T : EntityDto
	{
		protected class ReloadingBehavior
		{
			/// <summary>
			/// Reloading local entities
			/// </summary>
			public bool ReloadLocals { get; init; }

			/// <summary>
			/// Checking if elements are removed or added
			/// </summary>
			public bool ExceptWithEntities { get; init; }

			public bool Enabled => ReloadLocals || ExceptWithEntities;
		}

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

			public T EntityDto { get; }

			public UpdateType Type { get; }

			public override bool Equals(object? obj)
			{
				return obj is UpdateEntityElement other &&
				       EntityDto.Equals(other.EntityDto) &&
				       Type.Equals(other.Type);
			}

			public override int GetHashCode()
			{
				return EntityDto.GetHashCode() + Type.GetHashCode();
			}
		}

		private readonly IDbContextSettings _dbContextSettings;

		private readonly ConcurrentQueue<UpdateEntityElement> _updateQueue = new();
		private readonly CancellationTokenSource _updateLoopCts = new();
		private readonly ConcurrentDictionary<int, T> _set = new();
		private Task? _updateLoopTask;
		private ReloadingBehavior? _reloadingBehavior;

		protected BaseDbContext(IDbContextSettings dbContextSettings)
		{
			Id = GuidIdCreator.CreateString();

			_dbContextSettings = dbContextSettings;
			TypeNameOfCollectionEntries = typeof(T).Name;

			RunUpdateLoop();
		}

		protected abstract ReloadingBehavior GetReloadingBehavior();

		private void RunUpdateLoop()
		{
			_updateLoopTask = Task.Factory.StartNew(async () =>
			{
				var getChangesFromDbCounter = 0;

				while (!_updateLoopCts.IsCancellationRequested)
				{
					await Task.Delay(_dbContextSettings.UpdateDbDelayInMs);

					_reloadingBehavior ??= GetReloadingBehavior();

					await SaveChangesFromUpdateLoop();

					if (_reloadingBehavior is { Enabled: true } &&
					    getChangesFromDbCounter >= _dbContextSettings.UpdateChangesFromDbThreshold)
					{
						await GetChangesFromDb();
						getChangesFromDbCounter = 0;
					}

					getChangesFromDbCounter++;
				}
			}, _updateLoopCts.Token);
		}

		private async Task GetChangesFromDb()
		{
			if (_reloadingBehavior is not { Enabled: true })
				return;

			var dbEntitiesTask = Entities?.ToListAsync();

			if (dbEntitiesTask == null)
			{
				this.LogWarning($"dbEntities null\n" +
				                $"Cannot update changes from db for collection entry types: {TypeNameOfCollectionEntries}");
				return;
			}

			var dbEntities = await dbEntitiesTask;

			if (_reloadingBehavior.ExceptWithEntities)
			{
				var missingEntries = dbEntities.Where(CustomMissingEntriesFilter).Except(_set.Values).ToList();
				var removedEntries = _set.Values.Except(dbEntities.Where(CustomRemovedEntriesFilter)).ToList();

				if (missingEntries.Any())
				{
					await HandleNewEntries(missingEntries);
				}

				if (removedEntries.Any())
				{
					await HandleRemovedEntries(removedEntries);
				}
			}

			if (_reloadingBehavior.ReloadLocals)
			{
				await HandleLocalReloading();
			}
		}

		protected virtual bool CustomRemovedEntriesFilter(T entry)
		{
			return true;
		}

		protected virtual bool CustomMissingEntriesFilter(T entry)
		{
			return true;
		}


		protected virtual Task HandleNewEntries(List<T> newEntries)
		{
			foreach (var newEntry in newEntries)
			{
				var addingResult = _set.TryAdd(newEntry.GetHashCode(), newEntry);
				this.LogDebug($"{TypeNameOfCollectionEntries}: Adding was {addingResult}");
			}

			return Task.CompletedTask;
		}

		protected virtual Task HandleRemovedEntries(List<T> removedEntries)
		{
			foreach (var removedEntry in removedEntries)
			{
				var removeResult = _set.Remove(removedEntry.GetHashCode(), out _);
				this.LogDebug($"{TypeNameOfCollectionEntries}: Removing was {removeResult}");
			}

			return Task.CompletedTask;
		}

		protected virtual async Task HandleLocalReloading()
		{
			if (Entities == null)
				return;

			foreach (var entity in _set.Values)
			{
				await Entry(entity).ReloadAsync();
			}
		}

		protected void AddToSet(T entity)
		{
			_set.TryAdd(entity.GetHashCode(), entity);
		}


		private async Task SaveChangesFromUpdateLoop(bool skipCts = false)
		{
			var hasChanges = false;

			// todo: how to add analyzing of the updateQueue? 
			if (_dbContextSettings.AnalyzeUpdateSet)
			{
			}

			while (!_updateQueue.IsEmpty && (!_updateLoopCts.IsCancellationRequested || skipCts))
			{
				if (!_updateQueue.TryDequeue(out var updateEntityElement))
					continue;

				hasChanges = await ExecuteChange(updateEntityElement);
			}

			if (hasChanges)
				await SaveChangesAsync();
		}

		private async Task<bool> ExecuteChange(UpdateEntityElement updateEntityElement)
		{
			if (Entities == null)
			{
				this.LogWarning($"DbSet of {TypeNameOfCollectionEntries} is null");
				return false;
			}

			switch (updateEntityElement.Type)
			{
				case UpdateType.Add:
					this.LogDebug($"Adding type {typeof(T)}, with {updateEntityElement.EntityDto}");
					await Entities.AddAsync(updateEntityElement.EntityDto);
					return true;
				case UpdateType.Delete:
					this.LogDebug($"Removing type {typeof(T)}, with {updateEntityElement.EntityDto}");
					Entities.Remove(updateEntityElement.EntityDto);
					return true;
				case UpdateType.Update:
					return true;
			}

			return false;
		}


		protected DbSet<T>? Entities { get; set; }


		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder
			.UseNpgsql(
				$@"Server={_dbContextSettings.ServerAdresse};Port={_dbContextSettings.Port};UserId={_dbContextSettings.UserId};Password={_dbContextSettings.Password};Database={_dbContextSettings.DatabaseName};",
				options => options.EnableRetryOnFailure())
			.LogTo(Console.WriteLine, LogLevel.Information);

		public string Id { get; }
		public string TypeNameOfCollectionEntries { get; }

		public void AddEntity<TDto>(TDto dto) where TDto : EntityDto
		{
			var entityDto = dto as T;

			if (entityDto == null)
			{
				this.LogWarning($"Cannot add because dto is not {typeof(T)}");
				return;
			}

			if (!_set.Values.Contains(entityDto))
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
			_set.TryAdd(entityDto.GetHashCode(), entityDto);
			var updateEntityElement = UpdateEntityElement.Create(entityDto, UpdateType.Add);
			AddToUpdateQueue(updateEntityElement);
		}

		private void AddToUpdateQueue(UpdateEntityElement updateEntityElement)
		{
			if (_updateQueue.Contains(updateEntityElement))
			{
				return;
			}

			_updateQueue.Enqueue(updateEntityElement);
		}

		private void Update(T entityDto)
		{
			AddToUpdateQueue(UpdateEntityElement.Create(entityDto, UpdateType.Update));
		}

		public IEnumerable<object> GetEntities()
		{
			if (_set.IsEmpty && Entities != null)
			{
				foreach (var entity in Entities)
				{
					AddToSet(entity);
				}
			}

			return _set.Values;
		}

		public override async ValueTask DisposeAsync()
		{
			_updateLoopCts.Cancel();

			if (_updateLoopTask is { IsCompleted: true, IsCanceled: true, IsFaulted: true })
				_updateLoopTask?.Dispose();

			await SaveChangesFromUpdateLoop(true);
			await base.DisposeAsync();
		}
	}
}