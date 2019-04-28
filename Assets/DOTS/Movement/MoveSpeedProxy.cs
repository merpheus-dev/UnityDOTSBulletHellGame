using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class MoveSpeedProxy : MonoBehaviour, IConvertGameObjectToEntity
{
    public float Speed;
    
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var data = new MoveSpeed { Speed = Speed };
        dstManager.AddComponentData(entity, data);
    }
}
