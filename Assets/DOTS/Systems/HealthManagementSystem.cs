//using Unity.Entities;
//using Unity.Burst;
//using Unity.Jobs;
//using Unity.Collections;

//public class HealthManagementSystem : JobComponentSystem
//{
//    private EntityCommandBufferSystem bufferSystem;

//    protected override void OnCreate()
//    {
//        bufferSystem = World.Active.GetOrCreateSystem<EntityCommandBufferSystem>();
//    }

//    struct HealthManagementJob : IJobForEachWithEntity<Health, ExplosionComponent>
//    {
//        [ReadOnly] public EntityCommandBuffer CommandBuffer;
//        public void Execute(Entity e,int index,ref Health health, ref ExplosionComponent explosion)
//        {
//            if (health.Value > 0f)
//                return;

//            CommandBuffer.Instantiate(explosion.Prefab);
//            CommandBuffer.RemoveComponent(e,typeof(Health));
//        }
//    }

//    protected override JobHandle OnUpdate(JobHandle inputDeps)
//    {
//        var job = new HealthManagementJob
//        {
//            CommandBuffer = bufferSystem.CreateCommandBuffer()
//        };
//        return job.Schedule(this, inputDeps);
//    }
//}