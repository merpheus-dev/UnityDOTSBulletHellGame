using Unity.Entities;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using Unity.Transforms;
public class EnemyMovementSystem: JobComponentSystem
{
    [BurstCompile]
    struct EnemyMovementJob : IJobForEachWithEntity<EnemyMovementComponent,Translation>
    {
        [NativeDisableParallelForRestriction]
        public BufferFromEntity<TargetPointBuffer> pointBuffer;

        public void Execute(Entity entity, int index,[ReadOnly] ref EnemyMovementComponent enemyMovement,ref Translation translation)
        {
            DynamicBuffer<TargetPointBuffer> targetPointBuffer = pointBuffer[entity];
            for(var i = 0; i < targetPointBuffer.Length; i++)
            {
                translation.Value = targetPointBuffer[i].Value;
                //Debug.Log("Position:"+targetPointBuffer[i].Value);
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        EnemyMovementJob job = new EnemyMovementJob
        {
            pointBuffer = GetBufferFromEntity<TargetPointBuffer>(true)
        };
        return job.Schedule(this, inputDeps);
    }
}