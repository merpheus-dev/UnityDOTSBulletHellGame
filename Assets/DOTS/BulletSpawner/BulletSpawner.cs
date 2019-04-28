using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct BulletSpawner : IComponentData
{
    public Entity Prefab;
    public float ShotRate;
    public float Duration;
    public float Speed;
}
