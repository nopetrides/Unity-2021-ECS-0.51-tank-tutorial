using Unity.Entities;

[GenerateAuthoringComponent]
public struct LifetimeComp : IComponentData
{
    public float timeAlive;
}
