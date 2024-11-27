using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BootUp : MonoBehaviour
{
  
    void Awake()
    {
        CrosswordManager.CreateInstance();
        LetterInputManager.CreateInstance();

    }


}
