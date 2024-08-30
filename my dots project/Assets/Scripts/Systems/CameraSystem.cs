using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

[UpdateBefore(typeof(TransformSystemGroup))]
public partial class CameraSystem : SystemBase
{
    float rotation_x = 0f;
    float rotation_y = 0f;
    protected override void OnCreate()
    {
        RequireForUpdate<Player>();
    }

    // copies player position
    // updates player rotation
    protected override void OnUpdate()
    {
        
        Entity player_entity = SystemAPI.GetSingletonEntity<PlayerInput>();
        var childbuffer = GetBufferLookup<Child>(true);
        Entity orientation_entity = childbuffer[player_entity][0].Value;

        float3 player_position = SystemAPI.GetComponent<LocalTransform>(player_entity).Position;
        Camera.main.transform.position = player_position;

        float2 mouse_movement = SystemAPI.GetSingleton<PlayerInput>().mouse_movement;
        float mouse_sensitivity = SystemAPI.GetSingleton<Player>().mouse_sensitivity;
        float mouse_x = mouse_movement.x * mouse_sensitivity;
        float mouse_y = mouse_movement.y * mouse_sensitivity;
        rotation_y += mouse_x;
        rotation_x -= mouse_y;
        rotation_x = Mathf.Clamp(rotation_x, -90f, 90f);

        Quaternion current_rotation = Quaternion.Euler(rotation_x, rotation_y, 0);
        Camera.main.transform.rotation = current_rotation;

        //float4 current_rotation_float4 = new float4(current_rotation.x, current_rotation.y, current_rotation.z, current_rotation.w);
        float4 current_rotation_float4_y = new float4(0, current_rotation.y, 0, current_rotation.w);
        //SystemAPI.GetComponentRW<LocalTransform>(player_entity).ValueRW.Rotation.value = current_rotation_float4;
        SystemAPI.GetComponentRW<LocalTransform>(orientation_entity).ValueRW.Rotation.value = current_rotation_float4_y;
        
    }
}