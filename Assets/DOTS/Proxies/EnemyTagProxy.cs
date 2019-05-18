using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
public class EnemyTagProxy : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new EnemyTag());
    }
}
