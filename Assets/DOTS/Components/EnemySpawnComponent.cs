using Unity.Entities;
public struct EnemySpawnComponent : IComponentData
{
    public Entity Prefab;
    public int Count;
}