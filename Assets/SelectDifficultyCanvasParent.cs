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
    [SerializeField] private Button themedButton;


    


    public override void Start()
    {
        easyButton.onClick.AddListener(HandleEasySelected);
        intermediateButton.onClick.AddListener(HandleIntermediateSelected);
        hardButton.onClick.AddListener(HandleHardSelected);
        themedButton.onClick.AddListener(HandleThemedSelected);
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
    

    private void HandleThemedSelected()
    {
        CanvasManager.Instance.GoToSelectionAtChosenDifficulty(CrosswordsDifficulty.Themed);
    }
    

    private void OnDisable()
    {
        easyButton.onClick.RemoveListener(HandleEasySelected);
        intermediateButton.onClick.RemoveListener(HandleIntermediateSelected);
        hardButton.onClick.RemoveListener(HandleHardSelected);
        themedButton.onClick.RemoveListener(HandleThemedSelected);
    }
}