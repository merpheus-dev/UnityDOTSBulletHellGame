using Unity.Entities;
using UnityEngine;
[RequiresEntityConversion]
public class ObjectDestroyProxy : MonoBehaviour, IConvertGameObjectToEntity
{
    public float Delay;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var objectDestroyComponent = new ObjectDestroyComponent
        {
            Delay = Delay
        };
        dstManager.AddComponentData(entity, objectDestroyComponent);
        dstManager.AddComponent(entity, typeof(ObjectDestroyCounterComponent));
    }
}