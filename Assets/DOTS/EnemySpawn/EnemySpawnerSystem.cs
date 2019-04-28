using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

public class EnemySpawnerSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((ref EnemySpawner enemySpawner) =>
        {
           var instance = PostUpdateCommands.Instantiate(enemySpawner.Prefab);
            PostUpdateCommands.SetComponent(instance, new Translation { Value = enemySpawner.Position });
        });
    }
}