using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
[RequiresEntityConversion]
public class HealthComponentProxy : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var data = new Health
        {
            Value = 100f
        };
        dstManager.AddComponentData(entity, data);
    }
}
