using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
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
        public EntityCommandBuffer dotsEntityManager;

        public void Execute(
         [EntityInQueryIndex] int index,
         in Translation translation, in Entity entity, in CleanupTag destroyed)
        {
            dotsEntityManager.DestroyEntity(entity);
        }
    }

    protected override void OnUpdate()
    {
        var ecb = endInitializationEntityCommandBufferSystem.CreateCommandBuffer();
        // check if the game is over
        // default EntityManager
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        CleanupEntities CleanupEntitiesJob = new CleanupEntities
        {
            dotsEntityManager = ecb,
        };
        CleanupEntitiesJob.Run();

        /*// find all enemies that currently do not have the Lifetime ComponentData
        Entities.WithAll<CleanupTag>().ForEach((Entity entity, ref Translation pos) =>
        {
            PostUpdateCommands.DestroyEntity(entity);
        });
        */
    }
}