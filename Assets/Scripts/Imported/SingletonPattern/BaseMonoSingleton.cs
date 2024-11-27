using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseMonoSingleton : MonoBehaviour
{
    public BaseMonoSingleton Instance { get; protected set; }

    public abstract Type Type { get; }

    protected bool _deinitialised = false;

    public virtual void Deinit()
    {
        StopAllCoroutines();
        _deinitialised = true;

        Debug.Log($"{Type} deinitialised.");
    }

    protected virtual void OnDestroy()
    {
        if (!_deinitialised)
        {
            Debug.LogWarning($"OnDestroy called on {Type} before Deinit!");
            Deinit();
        }
    }
}
