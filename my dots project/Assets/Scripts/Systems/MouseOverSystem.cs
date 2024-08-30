using Unity.Entities;
using UnityEngine;
using Unity.Physics;
using Unity.Mathematics;
using Unity.Transforms;
using System.IO;
using System;
using UnityEngine.UIElements;

//[UpdateAfter(typeof(BuildPhysicsWorld)), UpdateAfter(typeof(InitializationSystemGroup))]
public partial class MousePointerSystem : SystemBase
{
    int dim = 4;
    int seed = 1;
    protected override void OnCreate()
    {
        RequireForUpdate<Player>();
    }
    protected override void OnUpdate()
    {
        PlayerInput player_input = SystemAPI.GetSingleton<PlayerInput>();
        bool mouse_click = SystemAPI.GetSingleton<PlayerInput>().mouse_click;
        int current_item = SystemAPI.GetSingleton<Player>().current_item;

        if (mouse_click && current_item < 9)
        {
            Build(current_item);
        }
        else if (mouse_click && current_item >= 9)
        {
            Shoot();
        }

        if (mouse_click)
        {
            SystemAPI.SetSingleton<PlayerInput>(new PlayerInput
            {
                move_input = player_input.move_input,
                mouse_movement = player_input.mouse_movement,
                mouse_click = false,
                interact = player_input.interact,
            });
        }
    }

    public string createChunkPath(int3 chunk)
    {
        return "C:\\Users\\joshy\\my dots project\\Assets\\Scripts\\MapData\\" + seed + "\\chunk_" + chunk.x + "_" + chunk.y + "_" + chunk.z + ".txt";
    }
    private void WriteToChunk(float3 position, int item_id)
    {
        int3 currentChunk = new int3((int)math.floor(position.x / (float)dim), (int)math.floor(position.y / (float)dim), (int)math.floor(position.z / (float)dim));
        int x = (int)math.round(position.x) - currentChunk.x * dim;
        int y = (int)math.round(position.y) - currentChunk.y * dim;
        int z = (int)math.round(position.z) - currentChunk.z * dim;

        string chunkPath = createChunkPath(currentChunk);
        int[,,] currentChunkData = new int[dim, dim, dim];
        StreamReader sr = new StreamReader(chunkPath);

        string line;
        int count = 0;
        while ((line = sr.ReadLine()) != null)
        {
            string[] lineArray = line.Split(',');
            for (int i = 0; i < lineArray.Length; i++)
            {
                currentChunkData[count % dim, count / dim, i] = Int32.Parse(lineArray[i]);
            }
            count++;

        } 
        sr.Close();
        currentChunkData[x, y, z] = item_id;
        string newChunkData = "";
        for (int j = 0; j < dim; j++)
        {
            for(int i = 0; i < dim; i++)
            {
                for(int k = 0; k < dim; k++)
                {
                    newChunkData += currentChunkData[i, j, k].ToString() + ',';
                }
                newChunkData = newChunkData.Remove(newChunkData.Length - 1, 1) + "\n";
            }
        }
        File.WriteAllText(chunkPath, newChunkData);
    }
    private void Shoot()
    {
        var screenRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        var filter = new CollisionFilter()
        {
            BelongsTo = 64u, // 6 -> player layer
            CollidesWith = 1u, // 1 -> default layer
            GroupIndex = 0
        };
        RaycastInput input = new RaycastInput
        {
            Start = screenRay.origin,
            End = screenRay.GetPoint(100),
            Filter = filter,
        };
        PhysicsWorldSingleton World = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        if (World.CastRay(input, out var hit))
        {
            var mapData = SystemAPI.GetSingleton<MapData>();
            if (mapData.build < 0)
            {
                Entity entity = World.Bodies[hit.RigidBodyIndex].Entity;
                LocalTransform transform = SystemAPI.GetComponent<LocalTransform>(entity);
                float3 block_position = math.round(transform.Position + (float3)Camera.main.transform.forward * .1f);
                SystemAPI.GetSingletonRW<MapData>().ValueRW.build = 0;
                SystemAPI.GetSingletonRW<MapData>().ValueRW.build_position = block_position;
                WriteToChunk(block_position, 0);
                EntityManager.DestroyEntity(entity);
            }
        }
    }

    private void Build(int item_id)
    {
        var screenRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        var filter = new CollisionFilter()
        {
            BelongsTo = 64u, // 6 -> player layer
            CollidesWith = 1u, // 1 -> default layer
            GroupIndex = 0,
        };
        RaycastInput input = new RaycastInput
        {
            Start = screenRay.origin,
            End = screenRay.GetPoint(100),
            Filter = filter,
        };
        PhysicsWorldSingleton World = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

        if (World.CastRay(input, out var hit))
        {
            var mapData = SystemAPI.GetSingleton<MapData>();
            if (mapData.build < 0)
            {
                Debug.Log("hit " +  hit.Position);
                //var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
                //DynamicBuffer<BlockEntity> buffer = SystemAPI.GetSingletonBuffer<BlockEntity>(true);
                float scale = 1f;
                float block_x = math.round(hit.Position.x / scale - Camera.main.transform.forward.x * .1f);
                float block_y = math.round(hit.Position.y / scale - Camera.main.transform.forward.y * .1f);
                float block_z = math.round(hit.Position.z / scale - Camera.main.transform.forward.z * .1f);
                float3 block_position = new float3(block_x, block_y, block_z) * scale;
        
                SystemAPI.GetSingletonRW<MapData>().ValueRW.build = 1;
                SystemAPI.GetSingletonRW<MapData>().ValueRW.build_position = block_position;
                SystemAPI.GetSingletonRW<MapData>().ValueRW.block_id = item_id;
                WriteToChunk(block_position, item_id);
            }
        }
    }
}
