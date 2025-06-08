using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SelectDifficultyCanvasParent : BaseCanvasParent
{
    [SerializeField] private Button easyButton;
    [SerializeField] private Button intermediateButton;
    [SerializeField] private Button hardButton;
    [SerializeField] private Button veryHardButton;
    [SerializeField] private Button themedButton;
    [SerializeField] private Button letterButton;

    


    public override void Start()
    {
        easyButton.onClick.AddListener(HandleEasySelected);
        intermediateButton.onClick.AddListener(HandleIntermediateSelected);
        hardButton.onClick.AddListener(HandleHardSelected);
        veryHardButton.onClick.AddListener(HandleVeryHardSelected);
        themedButton.onClick.AddListener(HandleThemedSelected);
        letterButton.onClick.AddListener(HandleLetterSelected);
    }

    private void HandleEasySelected()
    {
        CanvasManager.Instance.GoToSelectionAtChosenDifficulty(CrosswordsDifficulty.Easy);
    }

    private void HandleIntermediateSelected()
    {
        CanvasManager.Instance.GoToSelectionAtChosenDifficulty(CrosswordsDifficulty.Intermediate);
    }

    private void HandleHardSelected()
    {
        CanvasManager.Instance.GoToSelectionAtChosenDifficulty(CrosswordsDifficulty.Hard);
    }

    private void HandleVeryHardSelected()
    {
        CanvasManager.Instance.GoToSelectionAtChosenDifficulty(CrosswordsDifficulty.VeryHard);
    }

    private void HandleThemedSelected()
    {
        CanvasManager.Instance.GoToSelectionAtChosenDifficulty(CrosswordsDifficulty.Themed);
    }

    private void HandleLetterSelected()
    {
        CanvasManager.Instance.GoToSelectionAtChosenDifficulty(CrosswordsDifficulty.Letter);
    }

    private void OnDisable()
    {
        easyButton.onClick.RemoveListener(HandleEasySelected);
        intermediateButton.onClick.RemoveListener(HandleIntermediateSelected);
        hardButton.onClick.RemoveListener(HandleHardSelected);
        veryHardButton.onClick.RemoveListener(HandleVeryHardSelected);
        themedButton.onClick.RemoveListener(HandleThemedSelected);
        letterButton.onClick.RemoveListener(HandleLetterSelected);
    }
}