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
    [SerializeField] private Image mainImage;
    [SerializeField] private TextMeshProUGUI  text;
    [SerializeField] private TextMeshProUGUI  progressText;
    private CrosswordStructure structure;
    private int crosswordNumber;
    private string difficulty;
    private bool isLocked => lockImage.gameObject.activeSelf;
    private bool isComplete;

    public bool ShouldUpdate => !isLocked && !isComplete;

    public void Init(Color difficultyColor, string difficultyKey, int number, bool isLocked)
    {
        difficulty = difficultyKey;
        SetDifficultyColor(difficultyColor);
        SetNumber(number);
        if (isLocked)
        {
            Lock();
        }
        else
        {
            Unlock();
        }
    }

    private void SetProgress(int prog)
    {
        progressText.text = prog.ToString();
    }

    public void SetStructure(bool locked)
    {
        if (!System.IO.File.Exists(CrosswordUtils.GetCrosswordPath(difficulty, crosswordNumber)))
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        if (locked)
        {
            Lock();
            return;
        }

        Unlock();
        structure = CrosswordUtils.LoadCrosswordFromFile(difficulty, crosswordNumber);
        var horzFilled = structure.horizontalEntries.Select(t => t.IsEntryFilled);
        var vertFilled = structure.verticalEntries.Select(t => t.IsEntryFilled);
        bool allTrue = horzFilled.All(val => val) && vertFilled.All(val => val);
        isComplete = allTrue;
        SetProgress(horzFilled.Count() + vertFilled.Count());
        if (isComplete)
            SetAsCompleted();
        else
            SetAsInProgress();
    }
    
    
    public override void OnPointerDown(PointerEventData eventData)
    {
        if (isLocked)
        {
            // todo unlock level for coins?
            return;
        }

        CrosswordManager.Instance.GenerateCrossword(structure, isComplete, difficulty);
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
    
    public void SetAsInProgress()
    {
        mainImage.color = Color.white;
    }

    public void SetAsCompleted()
    {
        mainImage.color = Color.yellow;
    }

}
