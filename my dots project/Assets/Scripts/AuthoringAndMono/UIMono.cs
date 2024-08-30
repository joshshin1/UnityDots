using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;

public class UIMono : MonoBehaviour
{
    public GameObject crafting_ui;
}
public class UIBaker : Baker<UIMono>
{
    public override void Bake(UIMono authoring)
    {
         //AddComponent(GetEntity(authoring, TransformUsageFlags.Renderable), new UIData
         //{
         //    crafting_ui = authoring.crafting_ui
         //});
    }
}
