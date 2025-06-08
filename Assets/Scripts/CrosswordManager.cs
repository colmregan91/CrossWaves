using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks.Sources;
using OpenCover.Framework.Model;
using Unity.VisualScripting;
using UnityEditor.UIElements;

public class CrosswordManager : MonoSingleton<CrosswordManager>
{
    public CrosswordGridEntry[,] grid;

    private int MaxGridX = 12;
    private int MaxGridY = 12;

    private CrosswordStructure curCrossword;
    private int curQuestion;

    private List<CrosswordEntryPositional> allQuestions = new List<CrosswordEntryPositional>();
    public Action<List<CrosswordGridEntry>, CrosswordEntryPositional> OnNewWordClicked;
    public Action<int, CrosswordGridEntry> OnLetterRevealed;
    public Action<List<CrosswordGridEntry>> OnWordRevealed;

    private List<CrosswordGridEntry> SelectedPositions = new List<CrosswordGridEntry>();

    private CrosswordGridEntry curGridSelected;

    private bool isCurrentCrosswordCompleted;


    public List<CrosswordEntryPositional> getQuestions()
    {
        return allQuestions;
    }

    public int GetCurrentCrosswordNumber()
    {
        return curCrossword.crosswordNumber;
    }

    protected override void Awake()
    {
        base.Awake();
        ClearCrossword();
    }


    public void ClearCrossword()
    {
        allQuestions.Clear();
        InitializeGrid();
    }


    private void OnEnable()
    {
        LetterInputManager.Instance.OnCorrectAnswer += HandleCorrectAnswerEntered;
    }


    void InitializeGrid()
    {
        grid = new CrosswordGridEntry[MaxGridX, MaxGridY];

        int childIndex = 0;

        for (int y = 0; y < MaxGridY; y++)
        {
            for (int x = 0; x < MaxGridX; x++)
            {
                Transform child = transform.GetChild(childIndex);

                grid[x, y] = child.GetComponent<CrosswordGridEntry>();
                grid[x, y].Reset();
                childIndex++;
            }
        }
    }


    public void Reselect()
    {
        SelectQuestion(allQuestions[curQuestion]);
    }

    public CrosswordEntryPositional getWordPosition()
    {
        return allQuestions[curQuestion];
    }

    private void TurnOfFUnusedElements()
    {
        for (int y = 0; y < MaxGridY; y++)
        {
            for (int x = 0; x < MaxGridX; x++)
            {
                var gridItem = grid[x, y];
                if (gridItem.HasLetter == false)
                {
                    gridItem.TurnOffGridElement();
                }
            }
        }
    }

    public void SelectPrevWord()
    {
        if (LetterWheel.Instance.IsSpinning())
        {
            return;
        }

        UnselectPreviousWord(allQuestions[curQuestion]);
        curQuestion = (curQuestion - 1 + allQuestions.Count) % allQuestions.Count;

        if (isCurrentCrosswordCompleted == false)
        {
            if (allQuestions[curQuestion].IsEntryFilled)
            {
                SelectPrevWord();
                return;
            }
        }

        UnselectPreviousWord(allQuestions[curQuestion]);
        SelectQuestion(allQuestions[curQuestion]);
    }

    public void SelectNextWord()
    {
        if (LetterWheel.Instance.IsSpinning())
        {
            return;
        }

        UnselectPreviousWord(allQuestions[curQuestion]);
        curQuestion = (curQuestion + 1 + allQuestions.Count) % allQuestions.Count;

        if (isCurrentCrosswordCompleted == false)
        {
            if (allQuestions[curQuestion].IsEntryFilled)
            {
                SelectNextWord();
                return;
            }
        }

        SelectQuestion(allQuestions[curQuestion]);
    }

    private void SelectFirstAvailable()
    {
        if (allQuestions[curQuestion].IsEntryFilled)
        {
            curQuestion = (curQuestion + 1 + allQuestions.Count) % allQuestions.Count;
            SelectFirstAvailable();
            return;
        }

        SelectQuestion(allQuestions[curQuestion]);
    }

    private void HandleCorrectAnswerEntered()
    {
        allQuestions[curQuestion].IsEntryFilled = true;
        isCurrentCrosswordCompleted = allQuestions.All(q => q.IsEntryFilled);
        LetterWheel.Instance.OnWheelCleared += SelectNewAndUnsub;
    }

    private void SelectNewAndUnsub()
    {
        LetterWheel.Instance.OnWheelCleared -= SelectNewAndUnsub;

        if (isCurrentCrosswordCompleted)
        {
            Invoke(nameof(GoToCrosswordSelect), 1f);
        }
        else
        {
            SelectNextWord();
        }
    }

    private void GoToCrosswordSelect()
    {
        CanvasManager.Instance.GoToCanvas<SelectCrosswordCanvasParent>();
    }

    public void SelectClickedtWord(CrosswordEntryPositional wordInfo, CrosswordGridEntry gridEnttry)
    {
        if (LetterWheel.Instance.IsSpinning())
        {
            return;
        }

        UnselectPreviousWord(allQuestions[curQuestion]);
        curQuestion = allQuestions.IndexOf(wordInfo);
        SelectQuestion(allQuestions[curQuestion], gridEnttry);
    }


    private void UnselectPreviousWord(CrosswordEntryPositional wordInfo)
    {
        bool horz = wordInfo.isHorizontal;

        for (int i = 0; i < wordInfo.entry.answer.Length; i++)
        {
            int x = horz ? wordInfo.StartX + i : wordInfo.StartX;
            int y = horz ? wordInfo.StartY : wordInfo.StartY + i;

            if (grid[x, y].IsSelected)
            {
                grid[x, y].Unselect();
            }
        }
    }

    public void UpdateCurSelected(CrosswordGridEntry entry)
    {
        curGridSelected.UnSelectIndividual();
        curGridSelected = entry;
        curGridSelected.SelectIndividual();
    }


    public void SelectQuestion(CrosswordEntryPositional wordInfo, CrosswordGridEntry gridEntry = null)
    {
        if (LetterWheel.curLm.isSpinning)
        {
            return;
        }

        if (gridEntry == null)
        {
            gridEntry = grid[wordInfo.StartX, wordInfo.StartY];
        }

        UpdateCurSelected(gridEntry);
        SelectedPositions.Clear();
        bool horz = wordInfo.isHorizontal;

        for (int i = 0; i < wordInfo.entry.answer.Length; i++)
        {
            int x = horz ? wordInfo.StartX + i : wordInfo.StartX;
            int y = horz ? wordInfo.StartY : wordInfo.StartY + i;
            CrosswordGridEntry entry = grid[x, y];

            if (!entry.IsSelected)
            {
                entry.Select();
            }


            SelectedPositions.Add(entry);
        }

        OnNewWordClicked?.Invoke(SelectedPositions, wordInfo);
    }


    public CrosswordGridEntry GetCell(int X, int Y)
    {
        return grid[X, Y];
    }

    public void RevealWord()
    {
        if (LetterWheel.curLm.isSpinning)
        {
            return;
        }

        OnWordRevealed?.Invoke(SelectedPositions);
    }

    public void GenerateCrossword(CrosswordStructure str, bool isComplete)
    {
        curCrossword = str;
        foreach (var entryData in curCrossword.horizontalEntries)
        {
            PlaceWord(entryData);
        }


        foreach (var entryData in curCrossword.verticalEntries)
        {
            PlaceWord(entryData);
        }

        TurnOfFUnusedElements();

        allQuestions.AddRange(curCrossword.horizontalEntries);
        allQuestions.AddRange(curCrossword.verticalEntries);
        curQuestion = 0;
        var entry = allQuestions[curQuestion];
        curGridSelected = grid[entry.StartX, entry.StartY];
        isCurrentCrosswordCompleted = isComplete;
        if (!isCurrentCrosswordCompleted)
        {
            SelectFirstAvailable();
        }
        else
        {
            SelectQuestion(allQuestions[curQuestion]);
        }
    }

    public void RevealLetter()
    {
        if (curGridSelected.isShowing)
        {
            return;
        }

        if (LetterWheel.curLm.isSpinning)
        {
            return;
        }

        var next = SelectedPositions.Skip(SelectedPositions.IndexOf(curGridSelected) + 1).FirstOrDefault(t => t.isShowing == false) ?? SelectedPositions.FirstOrDefault(t => t.isShowing == false);
        if (next == null)
        {
            Debug.Log("return");
            return;
        }

        OnLetterRevealed?.Invoke(SelectedPositions.IndexOf(curGridSelected), curGridSelected);
        UpdateCurSelected(next);
    }

    private void PlaceWord(CrosswordEntryPositional entryInfo)
    {
        bool complete = entryInfo.IsEntryFilled;
        for (int i = 0; i < entryInfo.entry.answer.Length; i++)
        {
            int x = entryInfo.isHorizontal ? entryInfo.StartX + i : entryInfo.StartX;
            int y = entryInfo.isHorizontal ? entryInfo.StartY : entryInfo.StartY + i;

            if (grid[x, y].HasLetter)
            {
                continue;
            }

            grid[x, y].SetCell(entryInfo.entry.answer[i], entryInfo);

            if (complete)
            {
                grid[x, y].ShowCell();
            }
        }
    }


    private void OnDisable()
    {
        LetterInputManager.Instance.OnCorrectAnswer -= HandleCorrectAnswerEntered;
    }
}