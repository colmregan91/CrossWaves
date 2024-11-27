using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks.Sources;
using Unity.VisualScripting;

public class CrosswordManager : MonoSingleton<CrosswordManager>
{
    public CrosswordGridEntry[,] grid;

    private int MaxGridX = 12;
    private int MaxGridY = 12;

    private CrosswordStructure curCrossword;
    private int curQuestion;

    private List<CrosswordEntryPositional> allQuestions = new List<CrosswordEntryPositional>();
    public Action<List<CrosswordGridEntry>, CrosswordEntryPositional> OnNewWordClicked;
    private List<CrosswordGridEntry> SelectedPositions = new List<CrosswordGridEntry>();
    
    

    public void InitIaliseCrossword(int num)
    {
        CrosswordStructure structure = CrosswordUtils.LoadCrosswordFromFile($"{num}.json");
        InitializeGrid();
        ClearGrid();
        Debug.Log(structure.horizontalEntries.Count());
        Debug.Log(structure.verticalEntries.Count());
        GenerateCrossword(structure);
    }

    private void OnEnable()
    {
        LetterInputManager.Instance.OnCorrectAnswer += HandleCorrectAnswerEntered;
    }

    private void Start()
    {
        InitIaliseCrossword(1);
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


    private void ClearGrid()
    {
        foreach (var grid in grid)
        {
            grid.Reset();
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
        if (allQuestions[curQuestion].IsComplete)
        {
            SelectPrevWord();
            return;
        }

        UnselectPreviousWord(allQuestions[curQuestion]);
        SelectQuestion(allQuestions[curQuestion]);
    }

    public void SelectNewWord()
    {
        if (LetterWheel.Instance.IsSpinning())
        {
            return;
        }

        UnselectPreviousWord(allQuestions[curQuestion]);
        curQuestion = (curQuestion + 1 + allQuestions.Count) % allQuestions.Count;

        if (allQuestions[curQuestion].IsComplete)
        {
            SelectNewWord();
            return;
        }

        SelectQuestion(allQuestions[curQuestion]);
    }

    private void HandleCorrectAnswerEntered()
    {
        allQuestions[curQuestion].IsComplete = true;
        LetterWheel.Instance.OnWheelCleared += SelectNewAndUnsub;
    }

    private void SelectNewAndUnsub()
    {
        LetterWheel.Instance.OnWheelCleared -= SelectNewAndUnsub;
        SelectNewWord();
    }

    public void SelectClickedtWord(CrosswordEntryPositional wordInfo)
    {
        if (LetterWheel.Instance.IsSpinning())
        {
            return;
        }
        UnselectPreviousWord(allQuestions[curQuestion]);
        curQuestion = allQuestions.IndexOf(wordInfo);
        SelectQuestion(allQuestions[curQuestion]);
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


    public void SelectQuestion(CrosswordEntryPositional wordInfo)
    {
        if (LetterWheel.curLm.isSpinning)
        {
            return;
        }

        SelectedPositions.Clear();
        bool horz = wordInfo.isHorizontal;

        for (int i = 0; i < wordInfo.entry.answer.Length; i++)
        {
            int x = horz ? wordInfo.StartX + i : wordInfo.StartX;
            int y = horz ? wordInfo.StartY : wordInfo.StartY + i;

            if (!grid[x, y].IsSelected)
            {
                grid[x, y].Select();
            }

            SelectedPositions.Add(grid[x, y]);
        }

        OnNewWordClicked?.Invoke(SelectedPositions, wordInfo);
    }


    public CrosswordGridEntry GetCell(int X, int Y)
    {
        return grid[X, Y];
    }

    public void GenerateCrossword(CrosswordStructure str)
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
        SelectQuestion(allQuestions[curQuestion]);
    }

    private void PlaceWord(CrosswordEntryPositional entryInfo)
    {
        for (int i = 0; i < entryInfo.entry.answer.Length; i++)
        {
            int x = entryInfo.isHorizontal ? entryInfo.StartX + i : entryInfo.StartX;
            int y = entryInfo.isHorizontal ? entryInfo.StartY : entryInfo.StartY + i;

            if (grid[x, y].HasLetter)
            {
                continue;
            }

            grid[x, y].SetCell(entryInfo.entry.answer[i], entryInfo);
        }
        //
        // if (!entryInfo.isHorizontal)
        // {
        //     var gridsee = grid[entryInfo.StartX, entryInfo.StartY + entryInfo.entry.answer.Length];
        //
        //     if (gridsee != null)
        //     {
        //         grid[entryInfo.StartX, entryInfo.StartY + entryInfo.entry.answer.Length].TurnOffGridElement();
        //     }
        // }
        // else
        // {
        //     var gridsee = grid[entryInfo.StartX + entryInfo.entry.answer.Length, entryInfo.StartY];
        //
        //     if (gridsee != null)
        //     {
        //         grid[entryInfo.StartX + entryInfo.entry.answer.Length, entryInfo.StartY].TurnOffGridElement();
        //     }
        // }
    }


    private void OnDisable()
    {
        LetterInputManager.Instance.OnCorrectAnswer -= HandleCorrectAnswerEntered;
    }
}