using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;
[UpdateBefore(typeof(BuildPhysicsWorld))]
public class BulletSpawnerSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        if (!Input.GetButton("Fire1"))
            return;

        Entities.ForEach((ref LocalToWorld burstPoint, ref Rotation rotation, ref BulletSpawner bulletSpawner) =>
        {
            bulletSpawner.Duration += Time.fixedDeltaTime;

            if (bulletSpawner.Duration <= bulletSpawner.ShotRate)
                return;

            Entity instance = PostUpdateCommands.Instantiate(bulletSpawner.Prefab);
            PhysicsVelocity velocity = new PhysicsVelocity()
            {
                Linear = burstPoint.Forward * bulletSpawner.Speed,
                Angular = new float3(0f, 0f, 0f)
            };
            Translation translation = new Translation() { Value = burstPoint.Position + burstPoint.Forward };
            Rotation rot = new Rotation() { Value = rotation.Value };

           // PostUpdateCommands.SetComponent(instance, velocity);
            PostUpdateCommands.SetComponent(instance, rot);
            PostUpdateCommands.SetComponent(instance, translation);
            bulletSpawner.Duration = 0f;
        });
    }
}