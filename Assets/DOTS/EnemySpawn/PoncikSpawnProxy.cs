using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Transforms;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class PoncikSpawnProxy : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
    public GameObject Prefab;

    public int Amount;

    public Vector3 Min;
    public Vector3 Max;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var poncikSpawner = new PoncikSpawnData
        {
            Prefab = conversionSystem.GetPrimaryEntity(Prefab),
            Amount = Amount,
            Min = Min,
            Max = Max
        };
        dstManager.AddComponentData(entity, poncikSpawner);
    }

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.Add(Prefab);
    }
}
