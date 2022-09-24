using Unity.Entities;

public class LifetimeSystem : ComponentSystem
{
    // runs every frame
    protected override void OnUpdate()
    {
        // loop through all entites with a Lifetime component
        Entities.WithAll<LifetimeComp>().ForEach((Entity entity, ref LifetimeComp lifetime) =>
        {
            // decrement by time elapsed for one frame
            lifetime.timeAlive -= Time.DeltaTime;

            // if we have timed out, remove the Entity safely
            if (lifetime.timeAlive <= 0)
            {
                PostUpdateCommands.DestroyEntity(entity);
            }

        });
    }
}
