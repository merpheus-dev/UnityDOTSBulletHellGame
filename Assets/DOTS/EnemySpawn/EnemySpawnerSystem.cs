using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

public class EnemySpawnerSystem : JobComponentSystem
{
    private EntityQuery query;

    protected override void OnCreate()
    {
        query = GetEntityQuery(typeof(Translation), ComponentType.ReadOnly<EnemySpawner>());
    }

    struct EnemySpawnJob : IJobChunk
    {
        public ArchetypeChunkComponentType<Translation> Translations;
        public ArchetypeChunkComponentType<EnemySpawner> Spawners;
        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var chunkTranslations = chunk.GetNativeArray(Translations);
            var chunkSpawners = chunk.GetNativeArray(Spawners);
            for (int i = 0; i < chunk.Count; i++)
            {
                chunkTranslations[i] = new Translation { Value = chunkSpawners[i].Position };
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var translationType = GetArchetypeChunkComponentType<Translation>(false);
        var spawnerType = GetArchetypeChunkComponentType<EnemySpawner>(false);

        var job = new EnemySpawnJob
        {
            Spawners = spawnerType,
            Translations = translationType
        };

        return job.Schedule(query, inputDeps);
    }
}