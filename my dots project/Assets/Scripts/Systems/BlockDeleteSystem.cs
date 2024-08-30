using Unity.Burst;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Assertions;

/*
public partial struct BlockDeleteSystemn : ISystem
{
    public void OnCreate(ref SystemState state)
    {
    }

    //[BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        new ChunkDeleteJob
        {
            blockPosition = ,
            world = SystemAPI.GetSingleton<PhysicsWorldSingleton>()
        }.Run();
    }

    

[BurstCompile]
public partial struct ChunkDeleteJob : IJobEntity
{

    public PhysicsWorldSingleton world;
    public int3 blockPosition;
    //[BurstCompile]
    private void Execute()
        {
            var filter = new CollisionFilter()
            {
                BelongsTo = 64u, // 6 -> player layer
                CollidesWith = 1u, // 1 -> default layer
                GroupIndex = 0,
            };
            RaycastInput input = new RaycastInput
            {
                Start = blockPosition,
                End = blockPosition + new float3(1, 0, 0),
                Filter = filter,
            };

            if (world.CastRay(input, out var hit))
            {
                Entity entity = world.Bodies[hit.RigidBodyIndex].Entity;
                EntityManager.DestroyEntity(entity);
            }
        }
}
*/