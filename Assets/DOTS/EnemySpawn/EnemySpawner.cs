using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
[Serializable]
public struct EnemySpawner : IComponentData
{
    public Entity Prefab;
    public float3 Position;
}
