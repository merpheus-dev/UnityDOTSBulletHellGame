using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct PlayerMovementComponent : IComponentData
{
    public float Speed;
    public float2 CameraBounds;
}
