using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class PlayerProxy : MonoBehaviour, IConvertGameObjectToEntity
{
    public bool Parent = false;
    public float MovementSpeed = 5f;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        if (Parent)
        {
            var data = new PlayerMovementComponent
            {
                Speed = MovementSpeed
            };
            dstManager.AddComponentData(entity, data);
        }
        dstManager.AddComponent(entity, typeof(PlayerInputComponent));
    }
}
