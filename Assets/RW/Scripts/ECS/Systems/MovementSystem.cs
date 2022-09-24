using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class MovementSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.WithAll<MoveForwardComp>().ForEach((ref Translation trans, ref Rotation rot, ref MoveForwardComp moveForward) =>
        {
            trans.Value += moveForward.speed * Time.DeltaTime * math.forward(rot.Value);
        });
    }
}
