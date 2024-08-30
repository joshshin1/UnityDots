using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;

public class PlayerMono : MonoBehaviour
{
    public int id;
    public float mouse_sensitivity;
    public int current_item;
    public int current_hotbar;

    public float2 move_input;
    public float2 mouse_movement;
    public bool mouse_click;
    public bool interact;
}

public class PlayerBaker : Baker<PlayerMono>
{
    public override void Bake(PlayerMono authoring)
    {
         AddComponent(GetEntity(authoring, TransformUsageFlags.Dynamic), new Player
         {
             id = authoring.id,
             mouse_sensitivity = authoring.mouse_sensitivity,
             current_item = authoring.current_item, 
             current_hotbar = authoring.current_hotbar,
         });
         AddComponent(GetEntity(authoring, TransformUsageFlags.Dynamic), new PlayerInput
         {
             move_input = authoring.move_input,
             mouse_movement = authoring.mouse_movement,
             mouse_click = authoring.mouse_click,
             interact = authoring.interact,
         });
         DynamicBuffer<HotbarSlot> hotbar = AddBuffer<HotbarSlot>(GetEntity(authoring, TransformUsageFlags.Dynamic));
         for (int i = 0; i < 10; i++)
         {
             hotbar.Add(new HotbarSlot 
             {
                 value = i,
                 item_id = (i == 0 ? 10 : i),
                 quantity = 99,
             });
         }
    }
}
