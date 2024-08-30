using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct MapData : IComponentData
{
    public Entity block_1;
    public float3 build_position;
    public int build;
    public int block_id;
    // transition
    // public Entity[] block_list;
}

[InternalBufferCapacity(100)]
public struct BlockEntity : IBufferElementData
{
    public Entity value;
}
