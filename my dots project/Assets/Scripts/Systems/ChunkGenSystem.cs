using System;
using System.IO;
using Unity.Mathematics;
using UnityEngine;

public class ChunkGenerationSystem
{
    public int dim = 4;
    public int seed = 1;
    public float scaleDown = 20f;
    public float genThreshold = .5f;

    int mapDim = 10;
    int[,,] map = new int[10, 10, 10];
    float[,,] noiseGrid = new float[10, 10, 10];

    public string createChunkPath(int3 chunk)
    {
        return "C:\\Users\\joshy\\my dots project\\Assets\\Scripts\\MapData\\" + seed + "\\chunk_" + chunk.x + "_" + chunk.y + "_" + chunk.z + ".txt";
    }
    public string createChunkFile(int3 chunk)
    {
        string chunkPath = createChunkPath(chunk);
        if (!File.Exists(chunkPath)) {
            FileStream fs = File.Create(chunkPath);
            fs.Close(); 
        }
        return chunkPath;
    }

    public void CreatePerlinChunk(int3 chunk)
    {
        bool aboveGround = chunk.y >= 0;

        int x = chunk.x * dim;
        int y = chunk.y * dim;
        int z = chunk.z * dim;

        string chunkPath = createChunkFile(chunk);

        string newChunkData = "";
        for (int j = 0; j < dim; j++)
        {
            for (int i = 0; i < dim; i++)
            {
                for (int k = 0; k < dim - 1; k++)
                {
                    newChunkData += aboveGround ? "0," : (isBlock(new int3(x + i, y + j, z + k)) ? '1' : '0').ToString() + ',';
                }
                newChunkData += aboveGround ? "0\n" : (isBlock(new int3(x + i, y + j, z + dim - 1)) ? '1' : '0').ToString() + '\n';
            }
        }
        File.WriteAllText(chunkPath, newChunkData);
    }
    public void CreateChunk(int3 chunk)
    {
        int x = chunk.x * dim;
        int y = chunk.y * dim;
        int z = chunk.z * dim;

        string chunkPath = createChunkFile(chunk);
        
        string newChunkData = "";
        for (int j = 0; j < dim; j++)
        {
            for (int i = 0; i < dim; i++)
            {
                for (int k = 0; k < dim - 1; k++)
                {
                    newChunkData += map[x + i, y + j, z + k].ToString() + ',';
                }
                newChunkData += map[x + i, y + j, z + dim - 1].ToString() + ',';
            }
        }
        File.WriteAllText(chunkPath, newChunkData);
    }

    // GENERATE MAP BUT SLOW
    public void generateCaves()
    {
        plantSeeds();
        for (int i = 0; i < mapDim; i++) 
        { 
            for (int j = 0; j < mapDim; j++)
            {
                for (int k = 0; k < mapDim; k++)
                {
                    if (noise(new int3(i, j, k)) > genThreshold)
                    {
                        map[i, j, k] = 0;
                    }
                }
            }
        }
    }

    private void plantSeeds()
    {
        for (int i = 0; i < noiseGrid.GetLength(0); i++)
        {
            for (int j = 0; j < noiseGrid.GetLength(1); j++)
            {
                for (int k = 0; k < noiseGrid.GetLength(2); k++)
                {
                    noiseGrid[i, j, k] = 0;
                }
            }
        }
        for (int i = 0; i < 1000; i++)
        {
            int x = UnityEngine.Random.Range(0, 100);
            int y = UnityEngine.Random.Range(0, 100);
            int z = UnityEngine.Random.Range(0, 100);
            noiseGrid[x, y, z] = UnityEngine.Random.value;
            noiseGrid[x, y, z] = (noiseGrid[x, y, z] + .5f) / 1.5f;
        }
    }
    private float noise(int3 pos) 
    {
        int3 posX = new int3(0, 0, 0);
        int3 negX = new int3(0, 0, 0);
        int3 posY = new int3(0, 0, 0);
        int3 negY = new int3(0, 0, 0);
        int3 posZ = new int3(0, 0, 0);
        int3 negZ = new int3(0, 0, 0);

        for (int i = 0; i < mapDim; i++)
        {
            for (int j = -i - 1; j <= i + 1; j++)
            {
                for (int k = -i - 1; k <= i + 1; k++)
                {
                    if (pos.x + i < mapDim
                        && pos.y + j >= 0 && pos.y + j < mapDim
                        && pos.z + k >= 0 && pos.z + k < mapDim
                        && posX.Equals(int3.zero))
                    {
                        if (noiseGrid[pos.x + i, pos.y + j, pos.z + k] > 0)
                        {
                            posX = new int3(pos.x + i, pos.y + j, pos.z + k);
                        }
                    }
                    if (pos.x - i >= 0
                        && pos.y + j >= 0 && pos.y + j < mapDim
                        && pos.z + k >= 0 && pos.z + k < mapDim
                        && negX.Equals(int3.zero))
                    {
                        if (noiseGrid[pos.x - i, pos.y + j, pos.z + k] > 0)
                        {
                            negX = new int3(pos.x - i, pos.y + j, pos.z + k);
                        }
                    }
                    if (pos.x + j >= 0 && pos.x + j < mapDim
                        && pos.y + i < mapDim
                        && pos.z + k >= 0 && pos.z + k < mapDim
                        && posY.Equals(int3.zero))
                    {
                        if (noiseGrid[pos.x + j, pos.y + i, pos.z + k] > 0)
                        {
                            posY = new int3(pos.x + j, pos.y + i, pos.z + k);
                        }
                    }
                    if (pos.x + j >= 0 && pos.x + j < mapDim
                        && pos.y - i >= 0
                        && pos.z + k >= 0 && pos.z + k < mapDim
                        && negY.Equals(int3.zero))
                    {
                        if (noiseGrid[pos.x + j, pos.y - i, pos.z + k] > 0)
                        {
                            negY = new int3(pos.x + j, pos.y - i, pos.z + k);
                        }
                    }
                    if (pos.x + j >= 0 && pos.x + j < mapDim
                        && pos.y + k >= 0 && pos.y + k < mapDim
                        && pos.z + i < mapDim
                        && posZ.Equals(int3.zero))

                    {
                        if (noiseGrid[pos.x + j, pos.y + k, pos.z + i] > 0)
                        {
                            posZ = new int3(pos.x + j, pos.y + k, pos.z + i);
                        }
                    }
                    if (pos.x + j >= 0 && pos.x + j < mapDim
                        && pos.y + k >= 0 && pos.y + k < mapDim
                        && pos.z - i >= 0
                        && negZ.Equals(int3.zero))

                    {
                        if (noiseGrid[pos.x + j, pos.y + k, pos.z - i] > 0)
                        {
                            negZ = new int3(pos.x + j, pos.y + k, pos.z - i);
                        }
                    }
                }
            }
            if (!posX.Equals(int3.zero) && !negX.Equals(int3.zero)
                        && !posY.Equals(int3.zero) && !negY.Equals(int3.zero)
                        && !posZ.Equals(int3.zero) && !negZ.Equals(int3.zero))
            {
                break;
            }
        }
        float value = 0f;
        value += noiseGrid[posX.x, posX.y, posX.z] * (1f - math.abs(posX.x - pos.x) / (float)mapDim);
        value += noiseGrid[negX.x, negX.y, negX.z] * (1f - math.abs(negX.x - pos.x) / (float)mapDim);
        value += noiseGrid[posY.x, posY.y, posY.z] * (1f - math.abs(posY.y - pos.y) / (float)mapDim);
        value += noiseGrid[negY.x, negY.y, negY.z] * (1f - math.abs(negY.y - pos.y) / (float)mapDim);
        value += noiseGrid[posZ.x, posZ.y, posZ.z] * (1f - math.abs(posZ.z - pos.z) / (float)mapDim);
        value += noiseGrid[negZ.x, negZ.y, negZ.z] * (1f - math.abs(negZ.z - pos.z) / (float)mapDim);
        value /= 6f;
        return value;
    }

    public void generatePerlinCaves()
    {
        for (int i = 0; i < mapDim; i++)
        {
            for (int j = 0; j < mapDim; j++)
            {
                for (int k = 0; k < mapDim; k++)
                {
                    if (perlin3D(new int3(i, j, k), scaleDown) > genThreshold)
                    {
                        map[i, j, k] = 0;
                    }
                }
            }
        }
    }

    public float perlin3D(int3 pos, float scaleDown = 20f)
    {
        if (pos.x < 0)
        {
            pos.x += 1000;
        }
        if (pos.y < 0)
        {
            pos.x += 1000;
        }
        if (pos.z < 0)
        {
            pos.x += 1000;
        }
        float xy = Mathf.PerlinNoise(((float)pos.x + .5f) / scaleDown, ((float)pos.y + .5f) / scaleDown);
        float xz = Mathf.PerlinNoise(((float)pos.x + .5f) / scaleDown, ((float)pos.z + .5f) / scaleDown);
        float yz = Mathf.PerlinNoise(((float)pos.y + .5f) / scaleDown, ((float)pos.z + .5f) / scaleDown);

        float yx = Mathf.PerlinNoise((float)pos.y / scaleDown, (float)pos.x / scaleDown);
        float zx = Mathf.PerlinNoise((float)pos.z / scaleDown, (float)pos.x / scaleDown);
        float zy = Mathf.PerlinNoise((float)pos.z / scaleDown, (float)pos.y / scaleDown);

        return (xy + yx + xz + zx + yz + zy) / 6f;
    }

    public bool isBlock(int3 pos)
    {
        return !(perlin3D(pos, scaleDown) > genThreshold);
    }

    /*
    protected override void OnCreate()
    {
        
        for (int i = 0; i < 100; i++)
        {
            for (int j = 0; j < 100; j++)
            {
                for (int k = 0; k < 100; k++)
                {
                    map[i, j, k] = 1;
                }
            }
        }

        //generateCaves();
        generatePerlinCaves();
        
        for (int i = 0; i < mapDim / 10; i++)
        {
            for (int j = 0; j < mapDim / 10; j++)
            {
                for (int k = 0; k < mapDim / 10; k++) {
                    CreateChunk(new int3(i, j, k));
                }
            }
        }
        

    }
    protected override void OnUpdate()
    {
        if (!active) return;

        
        var em = World.EntityManager;
        DynamicBuffer<BlockEntity> buffer = SystemAPI.GetSingletonBuffer<BlockEntity>(true);


        for (int i = 0; i < mapDim; i++)
        {
            for (int j = 0; j < mapDim; j++)
            {
                for (int k = 0; k < mapDim; k++)
                {
                    if (map[i, j, k] == 1) //|| j == 0 && (i == 0 || j == 0))
                    {
                        var entity = em.Instantiate(buffer[6].value);
                        em.SetComponentData(entity, new LocalTransform
                        {
                            Position = new float3(i * .1f, j * .1f, 1 + k * .1f),
                            //Position = new float3(i, j, k),
                            Rotation = quaternion.identity,
                            Scale = .1f
                        });
                    }
                }
            }
        }
        

        active = false;
    }
    */
}