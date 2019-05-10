using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Unity.Rendering
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [ExecuteAlways]
    [WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.EntitySceneOptimizations)]
    public class CreateMissingRenderBoundsFromMeshRenderer : ComponentSystem
    {
        EntityQuery m_MissingRenderBounds;

        protected override void OnCreate()
        {
            m_MissingRenderBounds = GetEntityQuery(
                ComponentType.Exclude<Frozen>(), 
                ComponentType.Exclude<RenderBounds>(), 
                ComponentType.ReadWrite<RenderMesh>());
        }

        protected override void OnUpdate()
        {
            var chunks = m_MissingRenderBounds.CreateArchetypeChunkArray(Allocator.TempJob);
            var archetypeChunkRenderMeshType = GetArchetypeChunkSharedComponentType<RenderMesh>();
            for (int i = 0; i < chunks.Length; ++i)
            {
                var chunk = chunks[i];
                var sharedComponent = chunk.GetSharedComponentData(archetypeChunkRenderMeshType, EntityManager);
                if (sharedComponent.mesh != null)
                {
                    var entities = chunk.GetNativeArray(GetArchetypeChunkEntityType());
                    for (int j = 0; j < chunk.Count; ++j)
                    {
                        PostUpdateCommands.AddComponent(entities[j], new RenderBounds { Value = sharedComponent.mesh.bounds.ToAABB() });
                    }
                }
            }
            chunks.Dispose();
        }
        
    }

    /// <summary>
    /// Updates WorldRenderBounds for anything that has LocalToWorld and RenderBounds (and ensures WorldRenderBounds exists)
    /// </summary>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(CreateMissingRenderBoundsFromMeshRenderer))]
    [WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.EntitySceneOptimizations)]
    [ExecuteAlways]
    public class RenderBoundsUpdateSystem : JobComponentSystem
    {
        EntityQuery m_MissingWorldRenderBounds;
        EntityQuery m_WorldRenderBounds;
        EntityQuery m_MissingWorldChunkRenderBounds;
        
        [BurstCompile]
        struct BoundsJob : IJobChunk
        {
            [ReadOnly] public ArchetypeChunkComponentType<RenderBounds> RendererBounds;
            [ReadOnly] public ArchetypeChunkComponentType<LocalToWorld> LocalToWorld;
            public ArchetypeChunkComponentType<WorldRenderBounds> WorldRenderBounds;
            public ArchetypeChunkComponentType<ChunkWorldRenderBounds> ChunkWorldRenderBounds;

            
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                //@TODO: Delta change...
                var worldBounds = chunk.GetNativeArray(WorldRenderBounds);
                var localBounds = chunk.GetNativeArray(RendererBounds);
                var localToWorld = chunk.GetNativeArray(LocalToWorld);
                MinMaxAABB combined = MinMaxAABB.Empty;
                for (int i = 0; i != localBounds.Length; i++)
                {
                    var transformed = AABB.Transform(localToWorld[i].Value, localBounds[i].Value);

                    worldBounds[i] = new WorldRenderBounds { Value = transformed };
                    combined.Encapsulate(transformed);
                }
                
                chunk.SetChunkComponentData(ChunkWorldRenderBounds, new ChunkWorldRenderBounds { Value = combined });
            }
        }

        protected override void OnCreate()
        {
            m_MissingWorldRenderBounds = GetEntityQuery
            (
                new EntityQueryDesc
                {
                    All = new[] {ComponentType.ReadOnly<RenderBounds>(), ComponentType.ReadOnly<LocalToWorld>()},
                    None = new[] {ComponentType.ReadOnly<WorldRenderBounds>(), ComponentType.ReadOnly<Frozen>()}
                }
            );
            
            m_MissingWorldChunkRenderBounds = GetEntityQuery
            (
                new EntityQueryDesc
                {
                    All = new[] { ComponentType.ReadOnly<RenderBounds>(), ComponentType.ReadOnly<LocalToWorld>() },
                    None = new[] { ComponentType.ChunkComponentReadOnly<ChunkWorldRenderBounds>(), ComponentType.ReadOnly<Frozen>() }
                }
            );

            m_WorldRenderBounds = GetEntityQuery
            (
                new EntityQueryDesc
                {
                    All = new[] { ComponentType.ChunkComponent<ChunkWorldRenderBounds>(), ComponentType.ReadWrite<WorldRenderBounds>(), ComponentType.ReadOnly<RenderBounds>(), ComponentType.ReadOnly<LocalToWorld>() },
                    None = new[] { ComponentType.ReadOnly<Frozen>() }
                }
            );
        }

        protected override JobHandle OnUpdate(JobHandle dependency)
        {
            EntityManager.AddComponent(m_MissingWorldRenderBounds, typeof(WorldRenderBounds));
            EntityManager.AddComponent(m_MissingWorldChunkRenderBounds, ComponentType.ChunkComponent<ChunkWorldRenderBounds>());

            var boundsJob = new BoundsJob
            {
                RendererBounds = GetArchetypeChunkComponentType<RenderBounds>(true),
                LocalToWorld = GetArchetypeChunkComponentType<LocalToWorld>(true),
                WorldRenderBounds = GetArchetypeChunkComponentType<WorldRenderBounds>(),
                ChunkWorldRenderBounds = GetArchetypeChunkComponentType<ChunkWorldRenderBounds>(),
            };
            return boundsJob.Schedule(m_WorldRenderBounds, dependency);
        }

#if false
        public void DrawGizmos()
        {
            var boundsGroup = GetEntityQuery(typeof(LocalToWorld), typeof(WorldMeshRenderBounds), typeof(MeshRenderBounds));
            var localToWorlds = boundsGroup.GetComponentDataArray<LocalToWorld>();
            var worldBounds = boundsGroup.GetComponentDataArray<WorldMeshRenderBounds>();
            var localBounds = boundsGroup.GetComponentDataArray<MeshRenderBounds>();
            boundsGroup.CompleteDependency();

            Gizmos.matrix =Matrix4x4.identity;
            Gizmos.color = Color.green;
            for (int i = 0; i != worldBounds.Length; i++)
            {
                Gizmos.DrawWireCube(worldBounds[i].Value.Center, worldBounds[i].Value.Size);
            }

            Gizmos.color = Color.blue;
            for (int i = 0; i != localToWorlds.Length; i++)
            {
                Gizmos.matrix = new Matrix4x4(localToWorlds[i].Value.c0, localToWorlds[i].Value.c1, localToWorlds[i].Value.c2, localToWorlds[i].Value.c3);
                Gizmos.DrawWireCube(localBounds[i].Value.Center, localBounds[i].Value.Size);
            }
        }

        //@TODO: We really need a system level gizmo callback.
        [UnityEditor.DrawGizmo(UnityEditor.GizmoType.NonSelected)]
        public static void DrawGizmos(Light light, UnityEditor.GizmoType type)
        {
            if (light.type == LightType.Directional && light.isActiveAndEnabled)
            {
                var renderer = Entities.World.Active.GetExistingSystem<MeshRenderBoundsUpdateSystem>();
                renderer.DrawGizmos();
            }
        }
#endif
    }
}
