using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Rendering;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class BulletSpawnerProxy : MonoBehaviour, IConvertGameObjectToEntity ,IDeclareReferencedPrefabs
{
    public GameObject BulletPrefab;
    public float BulletSpeed;
    public float ShootRate = 0.1f;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var spawnData = new BulletSpawner
        {
            Prefab = conversionSystem.GetPrimaryEntity(BulletPrefab),
            ShotRate = ShootRate,
            Speed = BulletSpeed
        };

        dstManager.AddComponentData(entity, spawnData);
    }

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.Add(BulletPrefab);
    }
}
