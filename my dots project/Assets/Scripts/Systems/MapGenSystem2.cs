using System.IO;
using Unity.Entities;
using System;
using UnityEngine;
using Unity.Mathematics;
using Unity.Transforms;
using System.Collections.Generic;
using Unity.Rendering;
using UnityEngine.Rendering;
using Unity.Physics;

public partial class MapGenerationSystem : SystemBase
{
    ChunkGenerationSystem cgs = new ChunkGenerationSystem();
    HashSet<int3> generatedChunks = new HashSet<int3>();
    
    bool init = true;
    int seed;
    int dim;
    Dictionary<int3, Dictionary<int4, Entity>> chunkMeta = new Dictionary<int3, Dictionary<int4, Entity>>();
    Queue<int3> chunksToGenerate = new Queue<int3>();
    Queue<int3> chunksToDelete = new Queue<int3>();
    HashSet<int3> queuedChunks = new HashSet<int3>();
    HashSet<int3> queuedDeleteChunks = new HashSet<int3>();


    protected override void OnCreate()
    {
        seed = UnityEngine.Random.Range(0, int.MaxValue);
        seed = cgs.seed;
        dim = cgs.dim;
        Directory.CreateDirectory("C:\\Users\\joshy\\my dots project\\Assets\\Scripts\\MapData\\" + seed);
    }
 
    public void generateChunk(int3 chunk)
    {
        int[,,] currentChunk = new int[dim, dim, dim];
        StreamReader sr = new StreamReader(cgs.createChunkPath(chunk));

        string line;
        int count = 0;
        while ((line = sr.ReadLine()) != null)
        {
            string[] lineArray = line.Split(',');
            for (int i = 0; i < lineArray.Length; i++)
            {
                currentChunk[count % dim, count / dim, i] = Int32.Parse(lineArray[i]);
            }
            count++;

        }
        sr.Close();

        //var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        //var ecb = ecbSingleton.CreateCommandBuffer(World.Unmanaged);
        var em = World.EntityManager;
        var mapData = SystemAPI.GetSingleton<MapData>();

        Dictionary<int4, Entity> chunkEntities = new Dictionary<int4, Entity>();

        for (int i = 0; i < currentChunk.GetLength(0); i++)
        {
            for (int j = 0; j < currentChunk.GetLength(1); j++)
            {
                for (int k = 0; k < currentChunk.GetLength(2); k++)
                {
                    if (currentChunk[i, j, k] > 0)
                    {
                        chunkEntities.Add(new int4(i, j, k, 0), Entity.Null);
                        if (i == 0 && !cgs.isBlock(new int3(chunk.x * dim + i - 1, chunk.y * dim + j, chunk.z * dim + k))
                            || i > 0 && currentChunk[i - 1, j, k] == 0)
                        {
                            var entity = em.Instantiate(mapData.block_1);
                            em.SetComponentData(entity, new LocalTransform
                            {
                                Position = new float3(chunk.x * dim + i - .5f, chunk.y * dim + j, chunk.z * dim + k),
                                Rotation = quaternion.RotateY(90 * math.PI / 180),
                                Scale = 1
                            });
                            chunkEntities.Add(new int4(i, j, k, 1), entity);
                        }
                        if (i == currentChunk.GetLength(0) - 1 && !cgs.isBlock(new int3(chunk.x * dim + i + 1, chunk.y * dim + j, chunk.z * dim + k))
                            || i < currentChunk.GetLength(0) - 1 && currentChunk[i + 1, j, k] == 0)
                        {
                            var entity = em.Instantiate(mapData.block_1);
                            em.SetComponentData(entity, new LocalTransform
                            {
                                Position = new float3(chunk.x * dim + i + .5f, chunk.y * dim + j, chunk.z * dim + k),
                                Rotation = quaternion.RotateY(-90 * math.PI / 180),
                                Scale = 1
                            });
                            chunkEntities.Add(new int4(i, j, k, 2), entity);
                        }
                        if (j == 0 && !cgs.isBlock(new int3(chunk.x * dim + i, chunk.y * dim + j - 1, chunk.z * dim + k))
                            || j > 0 && currentChunk[i, j - 1, k] == 0)
                        {
                            var entity = em.Instantiate(mapData.block_1);
                            em.SetComponentData(entity, new LocalTransform
                            {
                                Position = new float3(chunk.x * dim + i, chunk.y * dim + j -.5f, chunk.z * dim + k),
                                Rotation = quaternion.RotateX(-90 * math.PI / 180),
                                Scale = 1
                            });
                            chunkEntities.Add(new int4(i, j, k, 3), entity);
                        }
                        if (j == currentChunk.GetLength(1) - 1 && (chunk.y == -1 || !cgs.isBlock(new int3(chunk.x * dim + i, chunk.y * dim + j + 1, chunk.z * dim + k)))
                            || j < currentChunk.GetLength(1) - 1 && currentChunk[i, j + 1, k] == 0)
                        {
                            var entity = em.Instantiate(mapData.block_1);
                            em.SetComponentData(entity, new LocalTransform
                            {
                                Position = new float3(chunk.x * dim + i, chunk.y * dim + j + .5f, chunk.z * dim + k),
                                Rotation = quaternion.RotateX(90 * math.PI / 180),
                                Scale = 1
                            });
                            chunkEntities.Add(new int4(i, j, k, 4), entity);
                        }
                        if (k == 0 && !cgs.isBlock(new int3(chunk.x * dim + i, chunk.y * dim + j, chunk.z * dim + k - 1))
                            || k > 0 && currentChunk[i, j, k - 1] == 0)
                        {
                            var entity = em.Instantiate(mapData.block_1);
                            em.SetComponentData(entity, new LocalTransform
                            {
                                Position = new float3(chunk.x * dim + i, chunk.y * dim + j, chunk.z * dim + k - .5f),
                                Rotation = quaternion.identity,
                                Scale = 1
                            });
                            chunkEntities.Add(new int4(i, j, k, 5), entity);
                        }
                        if (k == currentChunk.GetLength(2) - 1 && !cgs.isBlock(new int3(chunk.x * dim + i, chunk.y * dim + j, chunk.z * dim + k + 1)) 
                            || k < currentChunk.GetLength(2) - 1 && currentChunk[i, j, k + 1] == 0)
                        {
                            var entity = em.Instantiate(mapData.block_1);
                            em.SetComponentData(entity, new LocalTransform
                            {
                                Position = new float3(chunk.x * dim + i, chunk.y * dim + j, chunk.z * dim + k + .5f),
                                Rotation = quaternion.RotateX(180 * math.PI / 180),
                                Scale = 1
                            });
                            chunkEntities.Add(new int4(i, j, k, 6), entity);
                        }
                    }
                }
            }
        }
        chunkMeta.Add(chunk, chunkEntities);
        generatedChunks.Add(chunk);
    }

    public void destroyChunk(int3 chunk)
    {
        foreach (KeyValuePair<int4, Entity> pair in chunkMeta[chunk])
        {
            EntityManager.DestroyEntity(pair.Value);
        }
        generatedChunks.Remove(chunk);
        chunkMeta.Remove(chunk);
    }

    public void createBlock(float3 position)
    {
        int3 currentChunk = new int3((int)math.floor(position.x / (float)dim), (int)math.floor(position.y / (float)dim), (int)math.floor(position.z / (float)dim));
        int x = (int)math.round(position.x);
        int y = (int)math.round(position.y);
        int z = (int)math.round(position.z);

        int xKey = x - currentChunk.x * dim;
        int yKey = y - currentChunk.y * dim;
        int zKey = z - currentChunk.z * dim;

        Debug.Log(xKey + " " + yKey + " " + zKey);
        var mapData = SystemAPI.GetSingleton<MapData>();
        var em = World.EntityManager;
        Dictionary<int4, Entity> chunkEntities = chunkMeta[currentChunk];

        // solve logic between chunks
        chunkEntities.Add(new int4(xKey, yKey, zKey, 0), Entity.Null);
        if (!chunkEntities.ContainsKey(new int4(xKey - 1, yKey, zKey, 2)))
        {
            var entity = em.Instantiate(mapData.block_1);
            em.SetComponentData(entity, new LocalTransform
            {
                Position = new float3(x - .5f, y, z),
                Rotation = quaternion.RotateY(90 * math.PI / 180),
                Scale = 1
            });
            chunkEntities.Add(new int4(xKey, yKey, zKey, 1), entity);
        }
        if (!chunkEntities.ContainsKey(new int4(xKey + 1, yKey, zKey, 1)))
        {
            var entity = em.Instantiate(mapData.block_1);
            em.SetComponentData(entity, new LocalTransform
            {
                Position = new float3(x + .5f, y, z),
                Rotation = quaternion.RotateY(-90 * math.PI / 180),
                Scale = 1
            });
            chunkEntities.Add(new int4(xKey, yKey, zKey, 2), entity);
        }
        if (!chunkEntities.ContainsKey(new int4(xKey, yKey - 1, zKey, 4)))
        {
            var entity = em.Instantiate(mapData.block_1);
            em.SetComponentData(entity, new LocalTransform
            {
                Position = new float3(x, y - .5f, z),
                Rotation = quaternion.RotateX(-90 * math.PI / 180),
                Scale = 1
            });
            chunkEntities.Add(new int4(xKey, yKey, zKey, 3), entity);
        }
        if (!chunkEntities.ContainsKey(new int4(xKey, yKey + 1, zKey, 3)))
        {
            var entity = em.Instantiate(mapData.block_1);
            em.SetComponentData(entity, new LocalTransform
            {
                Position = new float3(x, y + .5f, z),
                Rotation = quaternion.RotateX(90 * math.PI / 180),
                Scale = 1
            });
            chunkEntities.Add(new int4(xKey, yKey, zKey, 4), entity);
        }
        if (!chunkEntities.ContainsKey(new int4(xKey, yKey, zKey - 1, 6)))
        {
            var entity = em.Instantiate(mapData.block_1);
            em.SetComponentData(entity, new LocalTransform
            {
                Position = new float3(x, y, z - .5f),
                Rotation = quaternion.identity,
                Scale = 1
            });
            chunkEntities.Add(new int4(xKey, yKey, zKey, 5), entity);
        }
        if (!chunkEntities.ContainsKey(new int4(xKey, yKey, zKey + 1, 5)))
        {
            var entity = em.Instantiate(mapData.block_1);
            em.SetComponentData(entity, new LocalTransform
            {
                Position = new float3(x, y, z + .5f),
                Rotation = quaternion.RotateX(180 * math.PI / 180),
                Scale = 1
            });
            chunkEntities.Add(new int4(xKey, yKey, zKey, 6), entity);
        }

        if (chunkEntities.ContainsKey(new int4(xKey - 1, yKey, zKey, 2)))
        {
            EntityManager.DestroyEntity(chunkEntities[new int4(xKey - 1, yKey, zKey, 2)]);
            chunkEntities.Remove(new int4(xKey - 1, yKey, zKey, 2));
        }
        if (chunkEntities.ContainsKey(new int4(xKey + 1, yKey, zKey, 1)))
        {
            EntityManager.DestroyEntity(chunkEntities[new int4(xKey + 1, yKey, zKey, 1)]);
            chunkEntities.Remove(new int4(xKey + 1, yKey, zKey, 1));
        }
        if (chunkEntities.ContainsKey(new int4(xKey, yKey - 1, zKey, 4)))
        {
            EntityManager.DestroyEntity(chunkEntities[new int4(xKey, yKey - 1, zKey, 4)]);
            chunkEntities.Remove(new int4(xKey, yKey - 1, zKey, 4));
        }
        if (chunkEntities.ContainsKey(new int4(xKey, yKey + 1, zKey, 3)))
        {
            EntityManager.DestroyEntity(chunkEntities[new int4(xKey, yKey + 1, zKey, 3)]);
            chunkEntities.Remove(new int4(xKey, yKey + 1, zKey, 3));
        }
        if (chunkEntities.ContainsKey(new int4(xKey, yKey, zKey - 1, 6)))
        {
            EntityManager.DestroyEntity(chunkEntities[new int4(xKey, yKey, zKey - 1, 6)]);
            chunkEntities.Remove(new int4(xKey, yKey, zKey - 1, 6));
        }
        if (chunkEntities.ContainsKey(new int4(xKey, yKey, zKey + 1, 5)))
        {
            EntityManager.DestroyEntity(chunkEntities[new int4(xKey, yKey, zKey + 1, 5)]);
            chunkEntities.Remove(new int4(xKey, yKey, zKey + 1, 5));
        }
    }

    public void destroyBlock(float3 position)
    {
        // math.round returns nearest even number on .5
        int3 currentChunk = new int3((int)math.floor(position.x / (float)dim), (int)math.floor(position.y / (float)dim), (int)math.floor(position.z / (float)dim));
        int x = (int)math.round(position.x);
        int y = (int)math.round(position.y);
        int z = (int)math.round(position.z);
        int xKey = x - currentChunk.x * dim;
        int yKey = y - currentChunk.y * dim;
        int zKey = z - currentChunk.z * dim;

        Debug.Log(xKey + " " + yKey + " " + zKey);

        var mapData = SystemAPI.GetSingleton<MapData>();
        var em = World.EntityManager;
        Dictionary<int4, Entity> chunkEntities = chunkMeta[currentChunk];

        // solve logic between chunks
        if (chunkEntities.ContainsKey(new int4(xKey - 1, yKey, zKey, 0)))
        {
            var entity = em.Instantiate(mapData.block_1);
            em.SetComponentData(entity, new LocalTransform
            {
                Position = new float3(x - .5f, y, z),
                Rotation = quaternion.RotateY(-90 * math.PI / 180),
                Scale = 1
            });
            chunkEntities.Add(new int4(xKey - 1, yKey, zKey, 2), entity);
        }
        if (chunkEntities.ContainsKey(new int4(xKey + 1, yKey, zKey, 0)))
        {
            var entity = em.Instantiate(mapData.block_1);
            em.SetComponentData(entity, new LocalTransform
            {
                Position = new float3(x + .5f, y, z),
                Rotation = quaternion.RotateY(90 * math.PI / 180),
                Scale = 1
            });
            chunkEntities.Add(new int4(xKey + 1, yKey, zKey, 1), entity);
        }
        if (chunkEntities.ContainsKey(new int4(xKey, yKey - 1, zKey, 0)))
        {
            var entity = em.Instantiate(mapData.block_1);
            em.SetComponentData(entity, new LocalTransform
            {
                Position = new float3(x, y - .5f, z),
                Rotation = quaternion.RotateX(90 * math.PI / 180),
                Scale = 1
            });
            chunkEntities.Add(new int4(xKey, yKey - 1, zKey, 4), entity);
        }
        if (chunkEntities.ContainsKey(new int4(xKey, yKey + 1, zKey, 0)))
        {
            var entity = em.Instantiate(mapData.block_1);
            em.SetComponentData(entity, new LocalTransform
            {
                Position = new float3(x, y + .5f, z),
                Rotation = quaternion.RotateX(-90 * math.PI / 180),
                Scale = 1
            });
            chunkEntities.Add(new int4(xKey, yKey + 1, zKey, 3), entity);
        }
        if (chunkEntities.ContainsKey(new int4(xKey, yKey, zKey - 1, 0)))
        {
            var entity = em.Instantiate(mapData.block_1);
            em.SetComponentData(entity, new LocalTransform
            {
                Position = new float3(x, y, z - .5f),
                Rotation = quaternion.RotateX(180 * math.PI / 180),
                Scale = 1
            });
            chunkEntities.Add(new int4(xKey, yKey, zKey - 1, 6), entity);
        }
        if (chunkEntities.ContainsKey(new int4(xKey, yKey, zKey + 1, 0)))
        {
            var entity = em.Instantiate(mapData.block_1);
            em.SetComponentData(entity, new LocalTransform
            {
                Position = new float3(x, y, z + .5f),
                Rotation = quaternion.identity,
                Scale = 1
            });
            chunkEntities.Add(new int4(xKey, yKey, zKey + 1, 5), entity);
        }

        for (int i = 1; i <= 6; i++)
        {
            int4 key = new int4(xKey, yKey, zKey, i);
            if (chunkEntities.ContainsKey(key))
            {
                EntityManager.DestroyEntity(chunkEntities[key]);
                chunkEntities.Remove(key);
            }
        }
        chunkEntities.Remove(new int4(xKey, yKey, zKey, 0));

    }

    protected override void OnUpdate()
    {
        
        var mapData = SystemAPI.GetSingleton<MapData>();
        var em = World.EntityManager;
        if (init)
        {

            Mesh mesh = new Mesh();
            mesh.vertices = new Vector3[] { Vector3.zero, Vector3.right, Vector3.up, new Vector3(1, 1, 0), new Vector3(1, 1, 1), new Vector3(0, 1, 1) };
            mesh.triangles = new int[] { 0, 2, 1, 1, 2, 3, 2, 4, 3, 2, 5, 4};
            mesh.normals = new Vector3[] { Vector3.back, Vector3.back, Vector3.back, Vector3.back, Vector3.up, Vector3.up };

            UnityEngine.Material material = Resources.Load<UnityEngine.Material>("ClearRed");
            // Create a RenderMeshDescription using the convenience constructor
            // with named parameters.
            var desc = new RenderMeshDescription(
                shadowCastingMode: ShadowCastingMode.Off,
                receiveShadows: false);

            // Create an array of mesh and material required for runtime rendering.
            var renderMeshArray = new RenderMeshArray(new UnityEngine.Material[] { material }, new Mesh[] { mesh });

            // Create empty base entity
            Entity prototype = em.CreateEntity();

            // Call AddComponents to populate base entity with the components required
            // by Entities Graphics
            RenderMeshUtility.AddComponents(
                prototype,
                em,
                desc,
                renderMeshArray,
                MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0));

            CollisionFilter filter = new CollisionFilter()
            {
                BelongsTo = 1u,
                CollidesWith = 64u, // all 1s, so all layers, collide with everything
                GroupIndex = 0
            };
            BlobAssetReference<Unity.Physics.Collider> box = Unity.Physics.BoxCollider.Create(new BoxGeometry
            {
                Center = new float3(.5f, .5f, .5f),
                BevelRadius = 0f,
                Orientation = quaternion.identity,
                Size = new float3(1f, 1f, 1f),
            }, filter);
            //Unity.Physics.PhysicsCollider physicsCollider = new Unity.Physics.PhysicsCollider { Value = box };
            if (box.IsCreated)
            {
                Debug.Log("created");
                em.AddComponentData(prototype, new PhysicsCollider { Value = box });
            }
            em.AddSharedComponentManaged(prototype, new PhysicsWorldIndex
            {
                Value = 0
            });

            em.AddComponentData(prototype, new LocalToWorld());
            em.AddComponentData(prototype, new LocalTransform
            {
                Position = new float3(-.5f, 2.5f, 2.5f),
                Rotation = quaternion.identity,
                Scale = 1
            });

            //Entity prototype2 = em.Instantiate(prototype);
            //em.AddComponentData(prototype2, new PhysicsCollider { Value = box });

        }




        Entity player = SystemAPI.GetSingletonEntity<Player>();
        LocalTransform transform = SystemAPI.GetComponent<LocalTransform>(player);

        int3 currentChunk = new int3((int)math.floor(transform.Position.x / (float)dim), (int)math.floor(transform.Position.y / (float)dim), (int)math.floor(transform.Position.z / (float)dim));
        bool newXChunk = math.abs(transform.Position.x - (currentChunk.x * dim + dim / 2)) > dim / 2 - 1;
        bool newZChunk = math.abs(transform.Position.z - (currentChunk.z * dim + dim / 2)) > dim / 2 - 1;

        int renderDist = 5;

        if (init)
        {
            for (int i = -renderDist; i <= renderDist; i++)
            {
                for (int j = -10; j < 5; j++)
                {
                    for (int k = -renderDist; k <= renderDist; k++)
                    {
                        int3 newChunk = currentChunk + new int3(i, j, k);
                        // move more functions to cgs
                        cgs.CreatePerlinChunk(newChunk);
                        generateChunk(newChunk);
                    }
                }
            }
        }
        init = false;
        
        /*
        if (newXChunk)
        {
            for (int j = -10; j < 0; j++)
                {
                for (int i = -renderDist; i <= renderDist; i++)
                {
                    int3 nextChunkOffset = new int3((int)(transform.Position.x - (currentChunk.x * dim + dim / 2)) < 0 ? -renderDist - 1 : renderDist + 1, j, i);
                    int3 newChunk = new int3(currentChunk.x + nextChunkOffset.x, nextChunkOffset.y, currentChunk.z + nextChunkOffset.z);
                    if (!queuedChunks.Contains(newChunk) && !generatedChunks.Contains(newChunk))
                    {
                        chunksToGenerate.Enqueue(newChunk);
                        queuedChunks.Add(newChunk);
                    }
                }
            }
        }
        if (newZChunk)
        {
            for (int j = -10; j < 0; j++)
                {
                for (int i = -renderDist; i <= renderDist; i++)
                {
                    int3 nextChunkOffset = new int3(i, j, (int)(transform.Position.z - (currentChunk.z * dim + dim / 2)) < 0 ? -renderDist - 1 : renderDist + 1);
                    int3 newChunk = new int3(currentChunk.x + nextChunkOffset.x, nextChunkOffset.y, currentChunk.z + nextChunkOffset.z);
                    if (!queuedChunks.Contains(newChunk) && !generatedChunks.Contains(newChunk))
                    {
                        chunksToGenerate.Enqueue(newChunk);
                        queuedChunks.Add(newChunk);
                    }
                }
            }
        }

        if (chunksToGenerate.Count > 0)
        {
            int3 chunkToGenerate = chunksToGenerate.Dequeue();
            queuedChunks.Remove(chunkToGenerate);
            cgs.CreatePerlinChunk(chunkToGenerate);
            generateChunk(chunkToGenerate);
            Debug.Log(chunkToGenerate);
        }

        foreach (int3 chunkToDelete in generatedChunks)
        {
            if (queuedDeleteChunks.Contains(chunkToDelete))
            {
                continue;
            }
            if (math.abs(chunkToDelete.x - currentChunk.x) > renderDist * 2 ||
                math.abs(chunkToDelete.z - currentChunk.z) > renderDist * 2)
            {
                chunksToDelete.Enqueue(chunkToDelete);
                queuedDeleteChunks.Add(chunkToDelete);
            }
        }
        if (chunksToDelete.Count > 0)
        {
            int3 chunkToDelete = chunksToDelete.Dequeue();
            queuedDeleteChunks.Remove(chunkToDelete);
            destroyChunk(chunkToDelete);
        }
        */




        if (mapData.build == 1)
        {
            SystemAPI.GetSingletonRW<MapData>().ValueRW.build = -1;
            createBlock(mapData.build_position);
        }
        else if (mapData.build == 0)
        {
            SystemAPI.GetSingletonRW<MapData>().ValueRW.build = -1;
            destroyBlock(mapData.build_position);
        }
          
    }
}
