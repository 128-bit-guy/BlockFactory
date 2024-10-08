﻿using System.Collections.Concurrent;
using BlockFactory.Content.Entity_;

namespace BlockFactory.World_;

//TODO Serialization of pending entities
public class PendingEntityManager
{
    public readonly World World;
    private readonly ConcurrentDictionary<Guid, Entity> _entities = new();
    private readonly List<Entity> _toPlaceInChunk = new();
    

    public PendingEntityManager(World world)
    {
        World = world;
    }

    public void Update()
    {
        foreach (var (_, entity) in _entities)
        {
            if (World.IsBlockLoaded(entity.GetBlockPos()))
            {
                _toPlaceInChunk.Add(entity);
            }
        }

        foreach (var entity in _toPlaceInChunk)
        {
            var c = World.GetChunk(entity.GetChunkPos())!;
            c.Data!.AddEntity(entity);
            c.AddEntityInternal(entity, false);
            _entities.Remove(entity.Guid, out _);
        }
        _toPlaceInChunk.Clear();
    }

    public void AddEntity(Entity entity)
    {
        if (!_entities.TryAdd(entity.Guid, entity))
        {
            throw new ArgumentException("Entity already exists");
        }
    }

    public void RemoveEntity(Entity entity)
    {
        _entities.Remove(entity.Guid, out var _);
    }
}