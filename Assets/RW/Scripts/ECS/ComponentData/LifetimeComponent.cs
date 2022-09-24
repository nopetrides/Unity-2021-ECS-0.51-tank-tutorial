using Unity.Entities;

[GenerateAuthoringComponent]
public struct LifetimeComponent : IComponentData
{
    public float allowedDistance;
}
