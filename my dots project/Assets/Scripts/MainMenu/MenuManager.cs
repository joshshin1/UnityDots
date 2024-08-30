using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

public class MenuManager : MonoBehaviour
{
    public Button start_button;
    
    private enum Scene
    {
        Main = 0, 
        Game = 1,
    }
    void Start()
    {
        start_button.onClick.AddListener(ChangeScene);
        //var handle = World.DefaultGameObjectInjectionWorld.GetExistingSystem<BlockSystem>();
        //var systemReference = World.DefaultGameObjectInjectionWorld.Unmanaged.GetUnsafeSystemRef<BlockSystem>(handle);
    }

    private void ChangeScene()
    {
        Cursor.lockState = CursorLockMode.Locked;
        SceneManager.LoadScene((int) Scene.Game);
    }

}
