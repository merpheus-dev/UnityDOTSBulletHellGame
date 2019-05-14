using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
[RequiresEntityConversion]
public class BulletCollisionProxy : MonoBehaviour, IConvertGameObjectToEntity,IDeclareReferencedPrefabs
{
    public GameObject Prefab;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var data = new BulletCollisionComponent
        {
            ExplosionPrefab = conversionSystem.GetPrimaryEntity(Prefab)
        };
        dstManager.AddComponentData(entity, data);
    }

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.Add(Prefab);
    }
}