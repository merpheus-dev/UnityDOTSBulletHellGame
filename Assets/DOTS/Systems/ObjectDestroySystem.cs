using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
public class ObjectDestroySystem : JobComponentSystem
{
    private EntityCommandBufferSystem bufferSystem;
    protected override void OnCreate()
    {
        bufferSystem = World.Active.GetOrCreateSystem<EntityCommandBufferSystem>();
    }

    struct ObjectDestroyJob : IJobForEachWithEntity<ObjectDestroyComponent,ObjectDestroyCounterComponent>
    {
        [ReadOnly] public EntityCommandBuffer commandBuffer;
        [ReadOnly] public float deltaTime;
        public void Execute(Entity entity, int index, 
                            ref ObjectDestroyComponent destroyComponent,
                            ref ObjectDestroyCounterComponent counterComponent)
        {
            if(counterComponent.Inverval<destroyComponent.Delay)
            {
                counterComponent.Inverval += deltaTime;
                return;
            }

            commandBuffer.DestroyEntity(entity);
        }
    }
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new ObjectDestroyJob
        {
            commandBuffer = bufferSystem.CreateCommandBuffer(),
            deltaTime = Time.deltaTime
        }.Schedule(this, inputDeps);
        //bufferSystem.AddJobHandleForProducer(job); //Apparently this doesn't do a thing
        job.Complete(); //This is required for other inputdeps
        return job;
    }
}