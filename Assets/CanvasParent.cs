using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using DG.Tweening;
using UnityEngine;

public class CrosswordCanvasParent : BaseCanvasParent
{
    public override void Awake()
    {
        AddBackButtonOverride(() =>
        {
            CanvasManager.Instance.GoToCanvas<SelectCrosswordCanvasParent>();
        });
    }

    public override void OnActive()
    {
        base.OnActive();
    }

    public override void OnInActive()
    {
        base.OnInActive();
        DOTween.KillAll(true);
        CrosswordUtils.SaveProgress(CrosswordManager.Instance.GetCurrentCrosswordNumber(), CrosswordManager.Instance.getQuestions());
        LetterInputManager.Instance.ClearLetters(); // need to wait unti floated letters colletion is empty
        LetterWheel.Instance.ClearWheel();
        CrosswordManager.Instance.ClearCrossword();
    }

    public override void OnDisable()
    {
        base.OnDisable();

        if (isCurrentCanvas)
        {
            CrosswordUtils.SaveProgress(CrosswordManager.Instance.GetCurrentCrosswordNumber(), CrosswordManager.Instance.getQuestions());
        }
       
    }
}