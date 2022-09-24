
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial class DamageSystem : SystemBase
{
    private float impactDistance = 2f;
    private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBufferSystem;

    [BurstCompile]
    [WithNone(typeof(CleanupTag), typeof(DestroyedTag))]
    private partial struct CheckForCollisionsJob : IJobEntity
    {
        public NativeParallelMultiHashMap<int, QuadrantData> quadrantMultiHashMap;
        public float3 playerPosition;
        public float collisionDistance;
        public EntityCommandBuffer.ParallelWriter dotsEntityManager;

        public void Execute(
         [EntityInQueryIndex] int index,
         in Translation translation, in Entity entity, in QuadrantTag tag, in MaxDistanceComp maxDistance)
        {
            int hashMapKey = QuadrantSystem.GetPositionHashMapKey(translation.Value);

            // entity, translation, tag = enemy
            if (tag.unitType == QuadrantTag.QuadrantUnitType.Enemy)
            {
                if (math.distance(translation.Value, playerPosition) <= collisionDistance)
                {
                    GameManager.EndGame();

                    dotsEntityManager.AddComponent(index, entity, typeof(DestroyedTag));
                }
                else if (quadrantMultiHashMap.TryGetFirstValue(hashMapKey, out QuadrantData quadrant, out NativeParallelMultiHashMapIterator<int> iterator))
                {
                    do
                    {
                        // entity, translation, tag = enemy
                        // quadrant = bullet (ignore other enemies)
                        if (quadrant.quadrantTag.unitType == QuadrantTag.QuadrantUnitType.Bullet)
                        {
                            // Did the bullet collide with this enemy?
                            if (math.distance(translation.Value, quadrant.position) <= collisionDistance)
                            {
                                // TODO change to OnDamaged
                                GameManager.AddScore(1);

                                dotsEntityManager.AddComponent(index, entity, typeof(DestroyedTag));
                                dotsEntityManager.AddComponent(index, quadrant.entity, typeof(CleanupTag));
                                break;
                            }
                        }

                    } while (quadrantMultiHashMap.TryGetNextValue(out quadrant, ref iterator));
                }

            }
            else
            {
                if (math.distance(playerPosition, translation.Value) >= maxDistance.allowedDistance)
                {
                    dotsEntityManager.AddComponent(index, entity, typeof(CleanupTag));
                }
            }
        }
    }

    protected override void OnCreate()
    {
        endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var ecb = endSimulationEntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        CheckForCollisionsJob setQuadrantDataHashMapJob = new CheckForCollisionsJob
        {
            quadrantMultiHashMap = QuadrantSystem.quadrantMultiHashMap,
            playerPosition = (float3)GameManager.GetPlayerPosition(),
            collisionDistance = impactDistance,
            dotsEntityManager = ecb,
        };
        setQuadrantDataHashMapJob.Run();
        endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(this.Dependency);

        /*
    EntityManager dotsEntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    if (!GameManager.IsGameOver())
    {
        float3 playerPosition = (float3)GameManager.GetPlayerPosition();
        float dist = impactDistance;
        Entities.ForEach((ref Translation enemyPos, ref Entity enemy) =>
        {
            playerPosition.y = enemyPos.Value.y;

            if (math.distance(enemyPos.Value, playerPosition) <= dist)
            {
                FXSpawner.Instance.SpawnFX(enemyPos.Value);
                FXSpawner.Instance.SpawnFX(playerPosition);

                GameManager.EndGame();

                dotsEntityManager.SetComponentData(enemy, new DestroyedTag { });
                //PostUpdateCommands.DestroyEntity(enemy);
            }

        }).Run();
        */
        /*
        Entities.WithAll<BulletTag>()
                    .ForEach((Entity bullet, ref Translation bulletPos, ref MaxDistanceComp lifetime) =>
                    {
                        if (math.distance(bulletPos.Value, enemy) >= lifetime.allowedDistance)
                        {
                            dotsEntityManager.SetComponentData(bullet, new DestroyedTag { });
                            //PostUpdateCommands.DestroyEntity(bullet);
                        }
                        else if (math.distance(enemy, bulletPos.Value) <= dist)
                        {
                            FXSpawner.Instance.SpawnFX(playerPosition);
                            GameManager.AddScore(1);

                            dotsEntityManager.SetComponentData(bullet, new DestroyedTag { });
                            dotsEntityManager.SetComponentData(enemy, new DestroyedTag { });
                        }
                    }).ScheduleParallel();
        */

        /*
        Entities
            .WithAll<EnemyTag>()
            .ForEach(
                (ref Translation enemyPos, in Entity enemy) =>
                {
                    // ignore the y value since the game is on the x and z plane
                    playerPosition.y = enemyPos.Value.y;

                    if (math.distance(enemyPos.Value, playerPosition) <= impactDistance)
                    {
                        FXSpawner.Instance.SpawnFX(enemyPos.Value);
                        FXSpawner.Instance.SpawnFX(playerPosition);

                        GameManager.EndGame();
                        PostUpdateCommands.DestroyEntity(enemy);
                    }

                    float3 enemyPosition = enemyPos.Value;

                    Entities.WithAll<BulletTag>()
                    .ForEach((Entity bullet, ref Translation bulletPos, ref MaxDistanceComp lifetime) =>
                    {
                        if (math.distance(bulletPos.Value, playerPosition) >= lifetime.allowedDistance)
                        {
                            PostUpdateCommands.DestroyEntity(bullet);
                        }
                        else if (math.distance(enemyPosition, bulletPos.Value) <= impactDistance)
                        {
                            FXSpawner.Instance.SpawnFX(enemyPosition);
                            GameManager.AddScore(1);

                            PostUpdateCommands.DestroyEntity(enemy);
                            PostUpdateCommands.DestroyEntity(bullet);
                        }
                    }).Schedule();
                }
            )
            .Run();*/

    }
}
