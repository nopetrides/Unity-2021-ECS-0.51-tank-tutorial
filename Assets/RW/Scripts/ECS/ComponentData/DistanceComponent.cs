using Unity.Entities;

[GenerateAuthoringComponent]
public struct MaxDistanceComp : IComponentData
{
    public float allowedDistance;
}
