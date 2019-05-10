using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class PlayerProxy : MonoBehaviour, IConvertGameObjectToEntity
{
    public float MovementSpeed = 5f;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var data = new PlayerMovementComponent
        {
            Speed = MovementSpeed
        };
        dstManager.AddComponent(entity, typeof(PlayerInputComponent));
        dstManager.AddComponentData(entity, data);
    }
}
