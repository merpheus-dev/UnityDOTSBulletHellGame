using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
[RequiresEntityConversion]
public class ExplosionComponentProxy : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
    public GameObject PrefabGO;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var data = new ExplosionComponent
        {
            Prefab = conversionSystem.GetPrimaryEntity(PrefabGO)
        };
        dstManager.AddComponentData(entity, data);
    }

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.Add(PrefabGO);
    }
}
