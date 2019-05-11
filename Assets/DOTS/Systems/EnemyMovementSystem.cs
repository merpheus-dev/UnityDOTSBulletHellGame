using Unity.Entities;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;
public class EnemyMovementSystem : JobComponentSystem
{
    [BurstCompile]
    struct EnemyMovementJob : IJobForEachWithEntity<EnemyMovementComponent, Translation>
    {
        public float DeltaTime;

        [ReadOnly]
        public BufferFromEntity<TargetPointBuffer> pointBuffer;

        public void Execute(Entity entity, int index, ref EnemyMovementComponent enemyMovement, ref Translation translation)
        {
            DynamicBuffer<TargetPointBuffer> targetPointBuffer = pointBuffer[entity];
            if (targetPointBuffer.Length <= 0)
                return;

            float3 targetPositon = targetPointBuffer[enemyMovement.CurrentIndex].Value;
            if (math.distance(translation.Value, targetPositon) < .1f)
            {
                if (targetPointBuffer.Length > enemyMovement.CurrentIndex + 1)
                {
                    enemyMovement.CurrentIndex++;
                }
            }
            else
            {
                translation.Value += -GetHeading(targetPositon, translation.Value) * DeltaTime * enemyMovement.Speed;
            }
        }

        public float3 GetHeading(float3 begin,float3 destination)
        {
            return math.normalize(destination - begin);
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        EnemyMovementJob job = new EnemyMovementJob
        {
            pointBuffer = GetBufferFromEntity<TargetPointBuffer>(true),
            DeltaTime = Time.deltaTime
        };
        return job.Schedule(this, inputDeps);
    }
}