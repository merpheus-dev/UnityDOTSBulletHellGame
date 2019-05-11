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
        burstPointQuery = GetEntityQuery(typeof(FireDurationComponent), ComponentType.ReadOnly<BurstPointComponent>());
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
            //Duration is seperate because otherwise entity prefab just disappear because of we need to REASSIGN to this element instead of change.
            var fireDurations = chunk.GetNativeArray(FireDurationType);
            for (var i = 0; i < chunk.Count; i++)
            {
                var position = positions[i];
                var burstPoint = burstPoints[i];
                var fireDuration = fireDurations[i];

                if (fireDurations[i].FireDuration <= burstPoints[i].ShootRate)
                {
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
                        Linear = position.Forward*40f,
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