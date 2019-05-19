using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
public class EnemySpawnerProxy : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
    public GameObject Prefab;
    public int Count;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var data = new EnemySpawnComponent
        {
            Prefab = conversionSystem.GetPrimaryEntity(Prefab),
            Count = Count
        };

        dstManager.AddComponentData(entity, data);
    }

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.Add(Prefab);
    }
}
