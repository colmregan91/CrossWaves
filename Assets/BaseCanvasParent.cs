using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public  class BaseCanvasParent : MonoBehaviour
{
    [SerializeField] private Transform _camLerpPos;
    [SerializeField]private CanvasGroup canvasGroup;
    [SerializeField] private Button backButton;
    public bool isCurrentCanvas;
    public virtual void Awake()
    {
        
    }

    protected void AddBackButtonOverride(Action callback)
    {
        backButton.onClick.AddListener(()=> callback?.Invoke());
    }
    
    public virtual void Start()
    {
        
    }

    public Vector3 GetCamLerpPos()
    {
        return _camLerpPos.position;
    }

    public virtual void OnActive()
    {
        canvasGroup.interactable = true;
        isCurrentCanvas = true;
    }
    
    public virtual void OnBeforeActive()
    {
    }

    public virtual void OnInActive()
    {
        canvasGroup.interactable = false;
        isCurrentCanvas = false;
    }

    public virtual void OnDisable()
    {
        backButton.onClick.RemoveAllListeners();

    }
}