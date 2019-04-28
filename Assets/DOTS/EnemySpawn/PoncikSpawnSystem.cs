using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

public class PoncikSpawnSystem : JobComponentSystem
{
    private Random random;

    private EntityCommandBufferSystem bufferSystem;

    protected override void OnCreate()
    {
        Random random = new Random();

        bufferSystem = World.GetOrCreateSystem<EntityCommandBufferSystem>();
    }

    struct SpawnerJob : IJobForEachWithEntity<PoncikSpawnData>
    {
        [ReadOnly] public EntityCommandBuffer CommandBuffer;

        public Random RandomObject;

        public void Execute(Entity entity, int index, ref PoncikSpawnData poncik)
        {
            for (int i = 0; i < poncik.Amount; i++)
            {
                var instance = CommandBuffer.Instantiate(poncik.Prefab);
                var pos = RandomObject.NextFloat3(poncik.Min, poncik.Max);
                CommandBuffer.SetComponent(instance, new Translation { Value = pos });
            }
            CommandBuffer.DestroyEntity(entity);
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        random.InitState();
        var spawnJob = new SpawnerJob
        {
            CommandBuffer = bufferSystem.CreateCommandBuffer(),
            RandomObject = random
        };
        return spawnJob.Schedule(this, inputDeps);
    }
}