using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Physics.Authoring;
[RequiresEntityConversion]
public class BulletCollisionProxy : MonoBehaviour, IConvertGameObjectToEntity
{
    public int InteractionLayer;
    public bool BelongToPlayer;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var shape = GetComponent<PhysicsShape>();
        var data = new BulletCollisionComponent
        {
            InteractionLayer = InteractionLayer,
            BelongToPlayer = BelongToPlayer
        };
        dstManager.AddComponentData(entity, data);
    }
}