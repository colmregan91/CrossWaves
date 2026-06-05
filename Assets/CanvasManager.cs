using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class CanvasManager : MonoSingleton<CanvasManager>
{
    public CrosswordsDifficulty ChosenDifficulty;
    private BaseCanvasParent[] canvasses;

    [SerializeField] private BaseCanvasParent startingCanvas;
    private BaseCanvasParent curCanvas;

    protected override void Awake()
    {
        base.Awake();
        canvasses = GetComponentsInChildren<BaseCanvasParent>();
        foreach (var canvas in canvasses)
        {
            if (canvas == startingCanvas) continue;
            canvas.CanvasGroup.alpha = 0;
            canvas.CanvasGroup.interactable = false;
            canvas.CanvasGroup.blocksRaycasts = false;
        }
        curCanvas = startingCanvas;
    }
    
    
    public void GoToSelectionAtChosenDifficulty(CrosswordsDifficulty difficulty)
    {
        ChosenDifficulty = difficulty;
        GoToCanvas<SelectCrosswordCanvasParent>();
    }
    
    public CrosswordsDifficulty GetChosenDifficulty()
    {
      return  ChosenDifficulty;
    }



    public void GoToCanvas<T>() where T : BaseCanvasParent
    {
        if (curCanvas != null)
        {
            curCanvas.OnInActive();

        }
        
        T targetCanvas = canvasses.OfType<T>().FirstOrDefault();

        if (targetCanvas != null)
        {
            curCanvas = targetCanvas;
            curCanvas.OnBeforeActive();
            curCanvas.CanvasGroup.DOFade(1f, 0.2f).OnComplete(() => curCanvas.OnActive());
        }
        else
        {
            Debug.LogError("No instance of " + typeof(T));
        }

    }
}