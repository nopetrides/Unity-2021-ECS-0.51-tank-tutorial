using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial class FXSpawnerSystem : SystemBase
{
    private Entity explosionEntityFromPrefab;
    public static GameObject prefab;

    // DOTS
    private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBufferSystem;

    public static void SetPrefab(GameObject p)
    {
        prefab = p;
    }

    protected void CheckReady()
    {
        if (prefab != null)
        {
            endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

            var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);
            explosionEntityFromPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(prefab, settings);
        }
    }

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
        if (endSimulationEntityCommandBufferSystem == null)
        {
            CheckReady();
        }
        else
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
}