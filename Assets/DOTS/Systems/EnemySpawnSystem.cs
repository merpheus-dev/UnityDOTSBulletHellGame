using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
public class EnemySpawnSystem : JobComponentSystem
{
    private EntityCommandBufferSystem bufferSystem;
    protected override void OnCreate()
    {
        bufferSystem = World.Active.GetOrCreateSystem<EntityCommandBufferSystem>();
    }

    struct SpawnJob : IJobForEachWithEntity<EnemySpawnComponent>
    {
        [ReadOnly] public EntityCommandBuffer CommandBuffer;
        public void Execute(Entity e, int index, ref EnemySpawnComponent spawner)
        {
            for (int i = 0; i < spawner.Count; i++)
            {
                CommandBuffer.Instantiate(spawner.Prefab);
            }
            CommandBuffer.DestroyEntity(e);
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new SpawnJob
        {
            CommandBuffer = bufferSystem.CreateCommandBuffer()
        };
        return job.Schedule(this, inputDeps);
    }
}