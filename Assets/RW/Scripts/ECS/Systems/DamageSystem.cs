using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class DamageSystem : ComponentSystem
{
    float impactDistance = 2f;

    protected override void OnUpdate()
    {
        if (!GameManager.IsGameOver())
        {
            float3 playerPosition = (float3)GameManager.GetPlayerPosition();

            Entities.WithAll<EnemyTag>().ForEach((Entity enemy, ref Translation enemyPos) =>
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

                Entities.WithAll<BulletTag>().ForEach((Entity bullet, ref Translation bulletPos, ref MaxDistanceComp lifetime) => 
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
                });
            });

        }
    }
}
