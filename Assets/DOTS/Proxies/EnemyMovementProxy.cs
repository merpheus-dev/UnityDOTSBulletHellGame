using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
public class EnemyMovementProxy : MonoBehaviour, IConvertGameObjectToEntity
{
    public List<float3> Positions;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
       DynamicBuffer<TargetPointBuffer> buffer = dstManager.AddBuffer<TargetPointBuffer>(entity);
        foreach (var position in Positions)
            buffer.Add(position);

        dstManager.AddComponent(entity, typeof(EnemyMovementComponent));
    }
}
