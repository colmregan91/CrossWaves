using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Unity.VisualStudio.Editor;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Image = UnityEngine.UI.Image;

public class selectCrosswordItem : ClickHandlers
{

    [SerializeField] private Image lockImage;
    [SerializeField] private Image ProgressImage;
    [SerializeField] private Image completeImage;
    [SerializeField] private TextMeshProUGUI  text;
    [SerializeField] private TextMeshProUGUI  progressText;
    private CrosswordStructure structure;
    private int crosswordNumber;
    private bool isLocked => lockImage.gameObject.activeSelf;
    private bool isComplete;

    public bool ShouldUpdate => !isLocked && !isComplete;

    public void Init(Color difficultyColor,int number, bool isLocked)
    {
        SetDifficultyColor(difficultyColor);
        SetNumber(number);
        if (isLocked)
        {
            Lock();
        }
        else
        {
            Unlock();
            SetStructure();
        }
    }

    private void SetProgress(int prog)
    {
        progressText.text = prog.ToString();
    }

    public void SetStructure()
    { 
        structure = CrosswordUtils.LoadCrosswordFromFile($"{crosswordNumber}.json");
        var horzFilled = structure.horizontalEntries.Select(t => t.IsEntryFilled);
        var vertFilled = structure.verticalEntries.Select(t => t.IsEntryFilled);
        var totalHorz = horzFilled.Count();
        var totalVert = vertFilled.Count();
        bool allTrue = horzFilled.All(val => val) && vertFilled.All(val => val) ;
        isComplete = allTrue;
        var total = totalHorz + totalVert;
        SetProgress(total);
        if (isComplete)
        {
            SetAsCompleted();
        }
    }
    
    

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (isLocked)
        {
            // todo unlock level for coins?
            return;
        }

        CrosswordManager.Instance.GenerateCrossword(structure,isComplete);
        CanvasManager.Instance.GoToCanvas<CrosswordCanvasParent>();
    }
    
    public void SetDifficultyColor(Color difficultyColor)
    {
        lockImage.color = difficultyColor;
        ProgressImage.color = difficultyColor;
    }
    
    public void SetNumber(int number)
    {
        crosswordNumber = number;
        text.text = crosswordNumber.ToString();
    }

    public void Lock()
    {
        lockImage.gameObject.SetActive(true);
        SetProgress(0);
    }
    
    public void Unlock()
    {
        lockImage.gameObject.SetActive(false);
    }

    public void SetAsCompleted()
    {
        lockImage.gameObject.SetActive(false);
        completeImage.gameObject.SetActive(true);
    }

}
