using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class MapMono : MonoBehaviour
{
    public GameObject[] block_list;
    public GameObject block_1;
    public float3 build_position = 0;
    public int build = -1;
    public int block_id = 0;
}

public class MapBaker : Baker<MapMono>
{
    public override void Bake(MapMono authoring)
    {
        AddComponent(GetEntity(authoring, TransformUsageFlags.Dynamic), new MapData
        {
            block_1 = GetEntity(authoring.block_1, TransformUsageFlags.Dynamic),
            build_position = authoring.build_position,
            build = authoring.build,
            block_id = authoring.block_id,
        });
        DynamicBuffer<BlockEntity> buffer = AddBuffer<BlockEntity>(GetEntity(authoring, TransformUsageFlags.Renderable));
        for (int i = 0; i < authoring.block_list.Length; i++)
        {
            buffer.Add(new BlockEntity { value = GetEntity(authoring.block_list[i], TransformUsageFlags.Renderable) });
        }
    }
}
