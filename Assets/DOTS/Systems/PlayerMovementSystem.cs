using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;
using UnityEngine;

public class PlayerMovementSystem : JobComponentSystem
{
    [BurstCompile]
    struct PlayerMovementJob : IJobForEach<Translation, PlayerInputComponent, PlayerMovementComponent>
    {
        public float3 CameraBounds;
        public float DeltaTime;

        public void Execute(ref Translation translation,[ReadOnly]ref PlayerInputComponent playerInput, ref PlayerMovementComponent playerMovement)
        {
            translation.Value.x = math.clamp(translation.Value.x, CameraBounds.x*-1, CameraBounds.x);
            translation.Value.z = math.clamp(translation.Value.z, CameraBounds.z, CameraBounds.z * -1);
            translation.Value += new float3(playerInput.MovementVector.x,0f, playerInput.MovementVector.y) * DeltaTime * playerMovement.Speed;
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        Camera camera = Camera.main;
        PlayerMovementJob job = new PlayerMovementJob
        {
            CameraBounds = camera.ScreenToWorldPoint(new Vector3(Screen.width, camera.transform.position.y, Screen.height)),
            DeltaTime = Time.deltaTime
        };
        return job.Schedule(this, inputDeps);
    }
}