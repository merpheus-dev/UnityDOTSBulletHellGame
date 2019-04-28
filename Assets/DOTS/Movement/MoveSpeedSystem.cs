using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;
using UnityEngine;
public class MoveSpeedSystem : JobComponentSystem
{
    [BurstCompile]
    struct MoveSpeedSystemJob : IJobForEach<Translation, MoveSpeed>
    {
        public float DeltaTime;
        public float3 HeadingVector;

        public void Execute(ref Translation translation, [ReadOnly] ref MoveSpeed movementSpeed)
        {
            translation.Value += HeadingVector * movementSpeed.Speed * DeltaTime;
        }
    }
    
    //Runs On Main Thread
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new MoveSpeedSystemJob()
        {
            DeltaTime = Time.deltaTime,
            HeadingVector = new float3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"))
        };

        return job.Schedule(this, inputDependencies);
    }
}