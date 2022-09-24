using Unity.Entities;
[GenerateAuthoringComponent]
public struct QuadrantTag : IComponentData
{
    public QuadrantUnitType unitType;
    public enum QuadrantUnitType
    {
        Enemy,
        Bullet,
    }
}
