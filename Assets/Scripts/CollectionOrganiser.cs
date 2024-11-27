using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class CollectionOrganiser<T> where T : Object
{
    private Transform _holder;
    private List<T> _displayList = new List<T>();
    private Queue<T> _availableModifiers = new Queue<T>();
    private T _modifierPrefab;

    public T ModifierPrefab => _modifierPrefab;
    public List<T> DisplayList => _displayList;
    public Queue<T> AvailableModifiers => _availableModifiers;
    public int ModifierCount => _availableModifiers.Count;
    public int DisplayCount => _displayList.Count;



    public CollectionOrganiser(string prefabPath, Transform holder)
    {
        var gameObject = Resources.Load<GameObject>(prefabPath);
        _modifierPrefab = gameObject.GetComponent<T>();
        _holder = holder;
    }

    public void AddToList(T t)
    {
        if (!_displayList.Contains(t))
        {
            _displayList.Add(t);
        }
    }

    public T AddOrDequeue()
    {
        T modifier;
        if (ModifierCount > 0)
        {
            modifier = _availableModifiers.Dequeue();
        }
        else
        {
            modifier = Object.Instantiate(_modifierPrefab, _holder);
        }

        AddToList(modifier);
        return modifier;
    }

    public void ClearList()
    {
        for (int i = 0; i < DisplayCount; i++)
        {
            _displayList[i].GameObject().SetActive(false);
            _availableModifiers.Enqueue(_displayList[i]);
        }
        
        _displayList.Clear();
    }

    public void ReturnToQueue(T index)
    {
        index.GameObject().SetActive(false);
        _availableModifiers.Enqueue(index);
        _displayList.Remove(index);
    }
}

public interface IQueuedObject
{
    void Init(object obj);
    void Deinit();
}