using System;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public  class BaseCanvasParent : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    public CanvasGroup CanvasGroup => canvasGroup;
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

    public virtual void OnActive()
    {
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        isCurrentCanvas = true;
    }
    
    public virtual void OnBeforeActive()
    {
    }

    public virtual void OnInActive()
    {
        canvasGroup.DOFade(0f, 0.2f);
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        isCurrentCanvas = false;
    }

    public virtual void OnDisable()
    {
        backButton.onClick.RemoveAllListeners();

    }
}