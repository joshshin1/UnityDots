using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

enum Item
{
    dirt = 0,
    gun = 100,
}

[InternalBufferCapacity(10)]
public struct HotbarSlot : IBufferElementData
{
    public int value;
    public int item_id;
    public int quantity;
}

public struct Player : IComponentData
{
    public int id;
    public float mouse_sensitivity;
    public int current_item;
    public int current_hotbar;
}

public struct PlayerInput : IComponentData
{
    public float2 move_input;
    public float2 mouse_movement;
    public bool mouse_click;
    public bool interact;
    public bool jump;
}
