using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;

namespace Unity.Rendering
{
    /// <summary>
    /// Render Mesh with Material (must be instanced material) by object to world matrix.
    /// Specified by the LocalToWorld associated with Entity.
    /// </summary>
    [Serializable]
    // Culling system requires a maximum of 128 entities per chunk (See ChunkInstanceLodEnabled)
    [MaximumChunkCapacity(128)]
    public struct RenderMesh : ISharedComponentData
    {
        public Mesh                 mesh;
        public Material             material;
        public int                  subMesh;

        [LayerField]
        public int                  layer;

        public ShadowCastingMode    castShadows;
        public bool                 receiveShadows;
    }

    [AddComponentMenu("DOTS/Deprecated/RenderMeshProxy-Deprecated")]
    public class RenderMeshProxy : SharedComponentDataProxy<RenderMesh>
    {
        internal override void UpdateComponentData(EntityManager manager, Entity entity)
        {
            // Hack to make rendering not break if there is no local to world
            if (!manager.HasComponent<LocalToWorld>(entity))
                manager.AddComponentData(entity, new LocalToWorld {Value = float4x4.identity});

            base.UpdateComponentData(manager, entity);
        }
    }
}
