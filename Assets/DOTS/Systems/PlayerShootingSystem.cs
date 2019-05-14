using Unity.Entities;
using Unity.Burst;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;
public class PlayerShootingSystem: JobComponentSystem
{
    private EntityCommandBufferSystem bufferSystem;

    protected override void OnCreate()
    {
        bufferSystem = World.Active.GetOrCreateSystem<EntityCommandBufferSystem>();
    }

    struct PlayerShootingJob : IJobForEach<BurstPointComponent, LocalToWorld, FireDurationComponent,PlayerInputComponent>
    {
        [ReadOnly] public EntityCommandBuffer CommandBuffer;
        [ReadOnly] public float DeltaTime;
        public void Execute(ref BurstPointComponent burstPoint, ref LocalToWorld position, ref FireDurationComponent fireDuration,ref PlayerInputComponent playerInput)
        {
            if (!playerInput.IsFiring)
                return;
            if (fireDuration.FireDuration <= burstPoint.ShootRate)
            {
                fireDuration.FireDuration += DeltaTime;
            }
            else
            {
                fireDuration = new FireDurationComponent { };
                var entity = CommandBuffer.Instantiate(burstPoint.Bullet);
                Translation localToWorld = new Translation
                {
                    Value = new float3(position.Value.c3.x, position.Value.c3.y, position.Value.c3.z)
                };
                //SetComponent works faster than AddComponent
                CommandBuffer.SetComponent(entity, localToWorld);
                PhysicsVelocity velocity = new PhysicsVelocity
                {
                    Linear = position.Forward * burstPoint.Speed,
                    Angular = float3.zero
                };
                CommandBuffer.SetComponent(entity, velocity);
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new PlayerShootingJob
        {
            CommandBuffer = bufferSystem.CreateCommandBuffer(),
            DeltaTime = Time.deltaTime
        };
        return job.Schedule(this,inputDeps);
    }
}