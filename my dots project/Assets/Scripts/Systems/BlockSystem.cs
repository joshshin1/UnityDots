using Unity.Entities;
using Unity.Burst;
using Unity.Transforms;
using Unity.Mathematics;


[BurstCompile]
[UpdateInGroup(typeof(InitializationSystemGroup), OrderLast = true)]
public partial struct BlockSystem : ISystem
{
    
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<MapData>();
    }
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        /*
        //var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        var mapData = SystemAPI.GetSingleton<MapData>();
        int dims = 40;
        float scale = 1f;
        for (int i = 0; i < dims; i++)
        {
            for (int j = 0; j < dims; j++)
            {
                for (int k = 0; k < dims; k++)
                {
                    //if ((i + j + k) % 2 == 1) continue;
                    var block = ecb.Instantiate(mapData.block_1);
                    LocalTransform block_transform = new LocalTransform
                    {
                        Position = new float3(i * scale, j * scale + 1f, k * scale),
                        Rotation = quaternion.identity,
                        Scale = scale,
                    };
                    ecb.SetComponent<LocalTransform>(block, block_transform);

                }
            }
        }
        //ecb.Playback(state.EntityManager);
        state.Enabled = false;
        */
    }
}
