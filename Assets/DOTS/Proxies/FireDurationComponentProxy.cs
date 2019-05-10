using UnityEngine;
using Unity.Entities;
public class FireDurationComponentProxy : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponent(entity, typeof(FireDurationComponent));
    }
}