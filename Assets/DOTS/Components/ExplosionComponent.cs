using Unity.Entities;
using Unity.Mathematics;
public struct ExplosionComponent : IComponentData
{
    public Entity Prefab;
    public float3 Position;
}