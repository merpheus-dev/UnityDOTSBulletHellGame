using Unity.Entities;
using Unity.Mathematics;
public struct EnemyMovementComponent : IComponentData
{
    public int CurrentIndex;
    public float Speed;
}

public struct TargetPointBuffer : IBufferElementData
{
    public static implicit operator float3(TargetPointBuffer e) { return e.Value; }
    public static implicit operator TargetPointBuffer(float3 e) { return new TargetPointBuffer { Value = e }; }
    public float3 Value;
}