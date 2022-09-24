using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine.Rendering;

[BurstCompile]
[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial class CleanupSystem : SystemBase
{
    private EndInitializationEntityCommandBufferSystem endInitializationEntityCommandBufferSystem;

    protected override void OnCreate()
    {
        endInitializationEntityCommandBufferSystem = World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
    }
    [BurstCompile]
    private partial struct CleanupEntities : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter CommandBuffer;

        public void Execute(
         [EntityInQueryIndex] int index,
         in Translation translation, in Entity entity, in CleanupTag destroyed)
        {
            CommandBuffer.DestroyEntity(index, entity);
        }
    }

    protected override void OnUpdate()
    {
        var ecb = endInitializationEntityCommandBufferSystem.CreateCommandBuffer();
        ecb.DestroyEntitiesForEntityQuery(GetEntityQuery(typeof(CleanupTag)));
    }
}