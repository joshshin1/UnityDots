using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using UnityEngine.UI;
using UnityEngine.TextCore.Text;
using TMPro;
using Unity.Mathematics;
using Unity.Entities.UniversalDelegates;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour
{
    public GameObject crafting_ui;
    public GameObject hotbar_ui;
    public Sprite[] item_pictures;

    private int last_hotbar = -1;
    private GraphicRaycaster ui_raycaster;
    private EventSystem event_system;  
    // Start is called before the first frame update
    void Start()
    {
        crafting_ui.SetActive(false);
        hotbar_ui.SetActive(true);
        ui_raycaster = hotbar_ui.GetComponent<GraphicRaycaster>();
        event_system = GetComponent<EventSystem>();
        //EntityManager em = World.DefaultGameObjectInjectionWorld.EntityManager;
        //Player stats = em.CreateEntityQuery(typeof(Player)).GetSingleton<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Interact"))
        {
            ToggleSystems();
            Cursor.lockState = 1 - Cursor.lockState;
            crafting_ui.SetActive(!crafting_ui.activeInHierarchy);
        }
        UpdateHotbarIfActive();
        //Vector2 position;
        //RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)crafting_ui.transform, Input.mousePosition, Camera.main, out position);
        //Camera.main.ScreenPointToRay(Input.mousePosition);
        PointerEventData temp = new PointerEventData(event_system);
        temp.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        ui_raycaster.Raycast(temp, results);
        foreach (RaycastResult result in results)
        {
            Debug.Log("Hit " + result.gameObject.name);
        }
    }

    void UpdateHotbarIfActive()
    {
        if (hotbar_ui.activeInHierarchy)
        {
            EntityManager em = World.DefaultGameObjectInjectionWorld.EntityManager;
            Entity player = em.CreateEntityQuery(typeof(Player)).GetSingletonEntity();
            Player stats = em.CreateEntityQuery(typeof(Player)).GetSingleton<Player>();
            DynamicBuffer<HotbarSlot> buffer = em.GetBuffer<HotbarSlot>(player);
            int current_hotbar = stats.current_hotbar;
            if (last_hotbar != current_hotbar)
            {
                Image last_slot = hotbar_ui.transform.GetChild(last_hotbar >= 0 ? last_hotbar : 0).GetComponent<Image>();
                last_slot.color = new Color(0, 0, 0, 150f / 255f);
                Image current_slot = hotbar_ui.transform.GetChild(current_hotbar).GetComponent<Image>();
                current_slot.color = Color.black;
                last_hotbar = current_hotbar;
            }

            for (int i = 0; i < 10; i++)
            {
                Image slot = hotbar_ui.transform.GetChild(i).GetChild(0).GetComponent<Image>();
                slot.color = Color.white;
                slot.sprite = item_pictures[buffer[i].value];

                TextMeshProUGUI text = hotbar_ui.transform.GetChild(i).GetChild(1).GetComponent<TextMeshProUGUI>();
                text.text = i.ToString();

                TextMeshProUGUI quantity = hotbar_ui.transform.GetChild(i).GetChild(2).GetComponent<TextMeshProUGUI>();
                quantity.text = buffer[i].quantity.ToString();
            }
        }
    }

    void ToggleSystems()
    {
        var systemref = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<CameraSystem>();
        systemref.Enabled = !systemref.Enabled;
        var systemref2 = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<MousePointerSystem>();
        systemref2.Enabled = !systemref2.Enabled;
        var systemref3 = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<InputSystem>();
        systemref3.Enabled = !systemref3.Enabled;
        bool enabled = World.DefaultGameObjectInjectionWorld.Unmanaged.GetExistingSystemState<PlayerInputSystem>().Enabled;
        World.DefaultGameObjectInjectionWorld.Unmanaged.GetExistingSystemState<PlayerInputSystem>().Enabled = !enabled;
    }
}
