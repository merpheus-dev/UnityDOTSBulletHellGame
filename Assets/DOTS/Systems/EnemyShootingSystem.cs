using Unity.Entities;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using Unity.Transforms;
using Unity.Physics;
using Unity.Mathematics;
using UnityEngine;
public class EnemyShootingSystem : JobComponentSystem
{
    private EntityQuery burstPointQuery;
    private EntityCommandBufferSystem bufferSystem;

    protected override void OnCreate()
    {
        burstPointQuery = GetEntityQuery(ComponentType.ReadOnly<BurstPointComponent>());
        bufferSystem = World.Active.GetOrCreateSystem<EntityCommandBufferSystem>();
    }

    [RequireComponentTag(typeof(BurstPointComponent))]
    struct EnemyShootingJob : IJobChunk
    {
        [ReadOnly] public EntityCommandBuffer CommandBuffer;
        [ReadOnly] public ArchetypeChunkComponentType<LocalToWorld> PositionType;
        public ArchetypeChunkComponentType<FireDurationComponent> FireDurationType;
        [ReadOnly] public ArchetypeChunkComponentType<BurstPointComponent> BurstPointType;
        [ReadOnly] public float DeltaTime;
        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var positions = chunk.GetNativeArray(PositionType);
            var burstPoints = chunk.GetNativeArray(BurstPointType);
            var fireDurations = chunk.GetNativeArray(FireDurationType);
            for (var i = 0; i < burstPoints.Length; i++)
            {
                var position = positions[i];
                var burstPoint = burstPoints[i];
                var fireDuration = fireDurations[i];

                //FAILS BECAUSE, SOME OF THEM RUNS ON A DIFFERENT THREAD, SO IT WON'T BE EQUAL.
                if (fireDurations[i].FireDuration <= burstPoints[i].ShootRate)
                {
                    //Mescaline... it is the only way to fly! And.. also this is the only way to change a parameter in the chunk, it it immutable
                    fireDurations[i] = new FireDurationComponent
                    {
                        FireDuration = fireDuration.FireDuration + DeltaTime
                    };
                }
                else
                {
                    fireDurations[i] = new FireDurationComponent { };
                    var entity = CommandBuffer.Instantiate(burstPoint.Bullet);
                    Translation localToWorld = new Translation
                    {
                        Value = new float3(position.Value.c3.x, position.Value.c3.y, position.Value.c3.z)
                    };
                    CommandBuffer.SetComponent(entity, localToWorld);
                    PhysicsVelocity velocity = new PhysicsVelocity
                    {
                        Linear = new float3(0f, 0f, 20f),
                        Angular = float3.zero
                    };
                    CommandBuffer.SetComponent(entity, velocity);
                }
            }
        }
    }
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        EnemyShootingJob job = new EnemyShootingJob
        {
            PositionType = GetArchetypeChunkComponentType<LocalToWorld>(true),
            BurstPointType = GetArchetypeChunkComponentType<BurstPointComponent>(true),
            FireDurationType = GetArchetypeChunkComponentType<FireDurationComponent>(false),
            CommandBuffer = bufferSystem.CreateCommandBuffer(),
            DeltaTime = Time.deltaTime
        };
        return job.Schedule(burstPointQuery, inputDeps);
    }
}