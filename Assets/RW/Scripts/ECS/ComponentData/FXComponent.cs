using Unity.Entities;

[GenerateAuthoringComponent]
public struct FXComponent : IComponentData
{
    public Entity Value;
}
