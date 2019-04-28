using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct MoveSpeed : IComponentData
{
    public float Speed;
}
