using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public partial class FXSpawnerSystem : SystemBase
{
    private Entity explosionEntityFromPrefab;
    private Entity explosionEntitySpawner;

    protected override void OnStartRunning()
    {
        explosionEntityFromPrefab = GetSingleton<FXComponent>().Value;
    }

    protected override void OnCreate()
    {
        endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    // DOTS
    private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBufferSystem;

    [BurstCompile]
    private partial struct SpawnDestructionFX : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter dotsEntityManager;
        public Entity prefabEntity;

        public void Execute(
         [EntityInQueryIndex] int index,
         in Translation translation, in Entity entity, in DestroyedTag destroyed)
        {
            var newEntity = dotsEntityManager.Instantiate(index, prefabEntity);
            dotsEntityManager.SetComponent(index, newEntity, new Translation { Value = translation.Value });
            dotsEntityManager.RemoveComponent(index, entity, typeof(DestroyedTag));
            dotsEntityManager.AddComponent(index, entity, typeof(CleanupTag));
        }
    }

    protected override void OnUpdate()
    {
        var ecb = endSimulationEntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();

        SpawnDestructionFX setQuadrantDataHashMapJob = new SpawnDestructionFX
        {
            dotsEntityManager = ecb,
            prefabEntity = explosionEntityFromPrefab
        };
        setQuadrantDataHashMapJob.Run();

        endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(this.Dependency);
    }
}