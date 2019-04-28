using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct PoncikSpawnData : IComponentData
{
    public Entity Prefab;
    public int Amount;
    public float3 Min;
    public float3 Max;
}
