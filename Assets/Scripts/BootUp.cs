using System.Collections.Generic;
using UnityEngine;

public class BootUp : MonoBehaviour
{
    void Awake()
    {
        CrosswordManager.CreateInstance();
        LetterInputManager.CreateInstance();
    }
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        GameObject obj = new GameObject("RuntimeStartup");
        DontDestroyOnLoad(obj);
        obj.AddComponent<RunTimeStartUp>();
    }
    


}