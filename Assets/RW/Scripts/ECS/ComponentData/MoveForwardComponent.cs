using Unity.Entities;

[GenerateAuthoringComponent]
public struct MoveForwardComp : IComponentData
{
    public float speed;
}
