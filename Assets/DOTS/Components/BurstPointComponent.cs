using Unity.Entities;
public struct BurstPointComponent : IComponentData
{
    public Entity Bullet;
    public float ShootRate;
}

