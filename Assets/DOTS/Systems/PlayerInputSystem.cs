using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Unity.Burst;
using Unity.Transforms;

public class PlayerInputSystem : JobComponentSystem
{
    [BurstCompile]
    struct PlayerInputJob : IJobForEach<PlayerInputComponent>
    {
        public float2 MovementVector;
        public bool IsFiring;
        public void Execute(ref PlayerInputComponent playerInput)
        {
            playerInput.MovementVector = MovementVector;
            playerInput.IsFiring = IsFiring;
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        PlayerInputJob job = new PlayerInputJob
        {
            MovementVector = new float2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")),
            IsFiring = Input.GetKey(KeyCode.Space)
        };
        return job.Schedule(this, inputDeps);
    }
}