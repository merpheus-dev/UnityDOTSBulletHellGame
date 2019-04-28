using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using System.Linq;
using Unity.Collections;
using Unity.Rendering;
using Unity.Transforms;
[DisallowMultipleComponent]
[RequiresEntityConversion]
public class EnemySpawnerProxy : MonoBehaviour, IConvertGameObjectToEntity
{
    public float3[] Positions;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var enemySpawnData = new EnemySpawner
        {
            Position = Positions[UnityEngine.Random.Range(0, Positions.Length)],
        };

        dstManager.AddComponentData(entity, enemySpawnData);
    }
}
