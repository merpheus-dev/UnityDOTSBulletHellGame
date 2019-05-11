using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[RequireComponent(typeof(FireDurationComponentProxy))]
public class BurstPointProxy : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
    public GameObject Prefab;
    public float ShootRate = .2f;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var data = new BurstPointComponent
        {
            Bullet = conversionSystem.GetPrimaryEntity(Prefab),
            ShootRate = ShootRate
        };
        dstManager.AddComponentData(entity, data);
        //dstManager.AddComponent(entity, typeof(FireDurationComponent));
    }

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.Add(Prefab);
    }
}
