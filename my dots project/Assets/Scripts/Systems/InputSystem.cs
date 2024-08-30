using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using System.Runtime.CompilerServices;
using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEngine.InputSystem;

[UpdateInGroup(typeof(InitializationSystemGroup), OrderLast = true)]
public partial class InputSystem : SystemBase
{
    private PlayerControls player_movement_actions;
    private Entity player_entity;

    protected override void OnCreate()
    {
        RequireForUpdate<Player>();

        player_movement_actions = new PlayerControls();
    }

    protected override void OnStartRunning()
    {
        player_movement_actions.Enable();
        player_movement_actions.PlayerMap.Hotbar.started += ctx => SetHotbar((int) ctx.ReadValue<float>());
        player_movement_actions.PlayerMap.MouseClick.started += ctx => SetMouseClick(ctx.ReadValue<float>() > 0);
        player_movement_actions.PlayerMap.Interact.started += ctx => SetInteract(ctx.ReadValue<float>() > 0);
        player_movement_actions.PlayerMap.PlayerJump.started += ctx => SetJump(ctx.ReadValue<float>() > 0);
        player_entity = SystemAPI.GetSingletonEntity<Player>();
    }

    protected override void OnStopRunning()
    {
        player_movement_actions.Disable();
        player_entity = Entity.Null;
    }
    protected override void OnUpdate()
    {
        var move_input = player_movement_actions.PlayerMap.PlayerMovement.ReadValue<Vector2>();
        var mouse_movement = player_movement_actions.PlayerMap.MouseLook.ReadValue<Vector2>();

        // WORKS BC OF UPDATE GROUP
        SystemAPI.SetSingleton(new PlayerInput 
        { 
            move_input = move_input,
            mouse_movement = mouse_movement,
            mouse_click = false,
            interact = false,
            jump = false,
        });
    }

    public void SetHotbar(int hotbar_press)
    {
        Player player = SystemAPI.GetSingleton<Player>();
        DynamicBuffer<HotbarSlot> hotbar = SystemAPI.GetBuffer<HotbarSlot>(player_entity);

        // TODO IMPLEMENT HOTBAR GUI
        //Debug.Log(hotbar_press);

        SystemAPI.SetSingleton<Player>(new Player {
            id = player.id,
            mouse_sensitivity = player.mouse_sensitivity,
            current_item = hotbar[hotbar_press].item_id,
            current_hotbar = hotbar_press,
        });
    }

    public void SetMouseClick(bool mouse_click)
    {
        PlayerInput player_input = SystemAPI.GetSingleton<PlayerInput>();
        SystemAPI.SetSingleton<PlayerInput>(new PlayerInput
        {
            move_input = player_input.move_input,
            mouse_movement = player_input.mouse_movement,
            mouse_click = mouse_click,
            interact = player_input.interact,
            jump = player_input.jump,
        });
    }

    public void SetInteract(bool interact)
    {
        PlayerInput player_input = SystemAPI.GetSingleton<PlayerInput>();
        SystemAPI.SetSingleton<PlayerInput>(new PlayerInput
        {
            move_input = player_input.move_input,
            mouse_movement = player_input.mouse_movement,
            mouse_click = player_input.mouse_click,
            interact = interact,
            jump = player_input.jump,
        });
    }


    public void SetJump(bool jump)
    {
        PlayerInput player_input = SystemAPI.GetSingleton<PlayerInput>();
        SystemAPI.SetSingleton<PlayerInput>(new PlayerInput
        {
            move_input = player_input.move_input,
            mouse_movement = player_input.mouse_movement,
            mouse_click = player_input.mouse_click,
            interact = player_input.interact,
            jump = jump
        });
    }
}
