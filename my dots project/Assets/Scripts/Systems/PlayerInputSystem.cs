using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Assertions;

[UpdateBefore(typeof(TransformSystemGroup))]
public partial struct PlayerInputSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Player>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    //[BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        PlayerInput player_input = SystemAPI.GetSingleton<PlayerInput>();
        MoveJob(ref state, player_input);

        //if (player_input.interact) // && Entity.interactable?)
            //Debug.Log('1');
    }

    private void MoveJob(ref SystemState state, PlayerInput player_input)
    {
        Entity player = SystemAPI.GetSingletonEntity<Player>();
        float2 move_input = player_input.move_input;
        float delta_time = SystemAPI.Time.DeltaTime;

        var childbuffer = SystemAPI.GetBufferLookup<Child>(true);
        Entity orientation_entity = childbuffer[player][0].Value;
        LocalTransform orientation_transform = SystemAPI.GetComponent<LocalTransform>(orientation_entity);

        // Corner Collision
        LocalTransform transform = SystemAPI.GetComponent<LocalTransform>(player);
        float3 collisionNormal = DetectCollision(ref state, player_input);
        float3 right_input = orientation_transform.Right() * move_input.x;
        float3 forward_input = orientation_transform.Forward() * move_input.y;
        Vector3 positionVector = new Vector3(right_input.x + forward_input.x, 0, right_input.z + forward_input.z);
        float sin = math.sin(Vector3.SignedAngle(collisionNormal, positionVector, Vector3.up) * math.PI / 180);
        Vector3 alongWallVector = Quaternion.AngleAxis(90, Vector3.up) * (Vector3.Magnitude(positionVector) * sin * collisionNormal);
        float3 cornerCollisionNormal = castCollider(ref state, transform.Position, alongWallVector * .1f);
        if (!cornerCollisionNormal.Equals(new float3()))
        {
            SystemAPI.GetComponentRW<LocalTransform>(player).ValueRW.Position += cornerCollisionNormal * .1f;
            return;
        }

        new PlayerMoveJob
        {
            delta_time = delta_time,
            positionVector = collisionNormal.Equals(new float3()) ? positionVector : alongWallVector,
        }.Run(); 
    }

    private unsafe float3 castCollider(ref SystemState state, float3 start, float3 distanceVector)
    {
        PhysicsWorldSingleton world = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        /*
        CapsuleGeometry capsule = new CapsuleGeometry()
        {
            Radius = .4f,
            Vertex0 = new float3(0, 1f, 0),
            Vertex1 = new float3(0, 2f, 0),
        };
        */
        SphereGeometry sphere = new SphereGeometry()
        {
            Center = float3.zero,//transform.Position,
            Radius = .4f,
        };
        var filter = new CollisionFilter()
        {
            BelongsTo = 64u,
            CollidesWith = 1u,
            GroupIndex = 0
        };
        BlobAssetReference<Unity.Physics.Collider> collider = Unity.Physics.SphereCollider.Create(sphere, filter);
        ColliderCastInput input = new ColliderCastInput()
        {
            Collider = (Unity.Physics.Collider*)collider.GetUnsafePtr(),
            Orientation = quaternion.identity,
            Start = start,
            End = start + distanceVector,
        };

        ColliderCastHit hit = new ColliderCastHit();
        try
        {
            if (world.CastCollider(input, out hit))
            {
                return hit.SurfaceNormal;
            }
        }
        catch (AssertionException e)
        {
            // TEMP FIX
            Debug.Log("collision detection error " + e.Message);
            return new float3();
        }
        collider.Dispose();
        return new float3();
    }

    private float3 DetectVerticalCollision(ref SystemState state, PlayerInput player_input)
    {
        return new float3();
    }

    private float3 DetectCollision(ref SystemState state, PlayerInput player_input)
    {
        float2 move_input = player_input.move_input;

        Entity player = SystemAPI.GetSingletonEntity<Player>();
        var childbuffer = SystemAPI.GetBufferLookup<Child>(true);
        Entity orientation_entity = childbuffer[player][0].Value;
        LocalTransform orientation_transform = SystemAPI.GetComponent<LocalTransform>(orientation_entity);
        float3 right_input = orientation_transform.Right() * move_input.x;
        float3 forward_input = orientation_transform.Forward() * move_input.y;
        float3 offset = (right_input + forward_input) * .1f;

        LocalTransform transform = SystemAPI.GetComponent<LocalTransform>(player);
        float3 position = transform.Position;

        return castCollider(ref state, position, offset);
    }
}

[BurstCompile]
public partial struct PlayerMoveJob : IJobEntity
{

    public float delta_time;
    public float3 positionVector;
    //[BurstCompile]
    private void Execute(ref LocalTransform transform, in PlayerInput player_input)
    {
        
        float speed = 5 * delta_time;
        transform.Position.x += positionVector.x * speed;
        transform.Position.z += positionVector.z * speed;
    }
}
