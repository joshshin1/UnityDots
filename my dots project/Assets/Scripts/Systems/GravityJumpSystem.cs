using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Assertions;

[UpdateBefore(typeof(TransformSystemGroup))]
public partial class GravityJumpSystem : SystemBase
{
    bool isJumping = false;
    bool isGrounded = false;
    bool hasJump = true;
    float downwardVelocity = 0f;
    protected override void OnCreate()
    {
        RequireForUpdate<Player>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    protected override void OnUpdate()
    {
        PlayerInput player_input = SystemAPI.GetSingleton<PlayerInput>();
        Entity player = SystemAPI.GetSingletonEntity<Player>();

        if (player_input.jump && hasJump)
        {
            isJumping = true;
        }

        if (isJumping)
        {
            float delta_time = SystemAPI.Time.DeltaTime;
            downwardVelocity -= .1f * delta_time;
            SystemAPI.GetComponentRW<LocalTransform>(player).ValueRW.Position.y += 20f * delta_time + downwardVelocity;
        }
        else if (!isGrounded)
        {
            float delta_time = SystemAPI.Time.DeltaTime;
            downwardVelocity -= .1f * delta_time;
            SystemAPI.GetComponentRW<LocalTransform>(player).ValueRW.Position.y += downwardVelocity;
        }

        LocalTransform transform = SystemAPI.GetComponent<LocalTransform>(player);
        float3 collisionNormal = castCollider(transform.Position, new float3(0, -.1f, 0));
        if (!collisionNormal.Equals(Vector3.zero))
        {
            isJumping = false;
            isGrounded = true;
            hasJump = true;
            downwardVelocity = 0f;
        } else
        {
            hasJump = false;
            isGrounded = false;
        }
    }

    private unsafe float3 castCollider(float3 start, float3 distanceVector)
    {
        PhysicsWorldSingleton world = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
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
            Debug.Log("collision detection error: " + e.Message);
            return new float3();
        }
        collider.Dispose();
        return new float3();
    }
}