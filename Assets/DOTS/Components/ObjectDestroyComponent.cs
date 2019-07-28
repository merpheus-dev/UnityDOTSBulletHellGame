using Unity.Entities;
public struct ObjectDestroyComponent : IComponentData
{
    public float Delay;
}

public struct ObjectDestroyCounterComponent : IComponentData
{
    public float Inverval;
}