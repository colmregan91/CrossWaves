using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CrosswordGenerator : MonoBehaviour
{
    public CrosswordDatabase CrosswordEntryBase;

    public static List<CrosswordEntry> AllEntries = new List<CrosswordEntry>();

    public GeneratedEntry[,] grid;

    private int MaxGridX = 12;
    private int MaxGridY = 12;
    public const int MaxHorzAndVert = 12;

    public List<CrosswordEntryPositional> UsedHorizontalEntries = new List<CrosswordEntryPositional>();
    public List<CrosswordEntryPositional> UsedVerticalEntries = new List<CrosswordEntryPositional>();

    public CrosswordEntry AttemptToFit;

    public List<string> usedAnswers = new List<string>();

    // private void Start()
    // {
    //     CrosswordEntryBase = new CrosswordDatabase();
    //     var crossOrig = new OriginalDatabase();
    //
    //     var seen = new List<string>();
    //     bool can = true;
    //     for (int i = 0; i < crossOrig.crosswordEntries.Count; i++)
    //     {
    //         if (seen.Contains(crossOrig.crosswordEntries[i].answer))
    //         {
    //             can = false;
    //             Debug.LogError($"{crossOrig.crosswordEntries[i].answer} appears twice");
    //         }
    //         else
    //         {
    //             seen.Add(crossOrig.crosswordEntries[i].answer);
    //         }
    //     }
    //
    //     // if (can)
    //     // {
    //     CrosswordEntryBase.Entries = crossOrig.crosswordEntries;
    //     CrosswordUtils.WriteNewDatabaseToFile(CrosswordEntryBase);
    //     // }
    //     // else
    //     // {
    //     //     Debug.LogError($"needs recheck");
    //     // }
    // }

    public void TryFit()
    {
        bool wordPlaced = false;

        // Try to place the word in each cell in both orientations
        for (int y = 0; y < grid.GetLength(1) && !wordPlaced; y++)
        {   
            for (int x = 0; x < grid.GetLength(0) && !wordPlaced; x++)
            {    
                if (grid[x, y].IsSelected == false)
                {
                    continue;
                }
                // Check if the word can fit horizontally
          
                if (CanFitWord(AttemptToFit.answer, x, y, true) && !usedAnswers.Contains(AttemptToFit.answer) && UsedHorizontalEntries.Count() < MaxHorzAndVert)
                {

                    wordPlaced = true;

                    CrosswordEntryPositional newEntryData = new CrosswordEntryPositional()
                    {
                        StartX = x, StartY = y, isHorizontal = true, entry = new CrosswordEntry(AttemptToFit.question, AttemptToFit.answer)
                    };
                    PlaceWord(newEntryData);
                    UsedHorizontalEntries.Add(newEntryData);
                    Debug.Log($"placed at {newEntryData.StartX}, {newEntryData.StartY} ");
                    grid[x, y].Unselect();
                }
                // If not, try fitting it vertically
                else
                {
                    if (CanFitWord(AttemptToFit.answer, x, y, false) && !usedAnswers.Contains(AttemptToFit.answer) && UsedHorizontalEntries.Count() < MaxHorzAndVert)
                    {
                        
                        wordPlaced = true;

                        CrosswordEntryPositional newEntryData = new CrosswordEntryPositional()
                        {
                            StartX = x, StartY = y, isHorizontal = false, entry = new CrosswordEntry(AttemptToFit.question, AttemptToFit.answer),
                        };
                        PlaceWord(newEntryData);
                        UsedVerticalEntries.Add(newEntryData);
                        Debug.Log($"placed at {newEntryData.StartX}, {newEntryData.StartY} ");
                        grid[x, y].Unselect();
                    }
                }
            }
        }
    }


    public void SaveNewCrossword()
    {
        CrosswordUtils.SaveNewCrossword(UsedHorizontalEntries, UsedVerticalEntries);
    }


    public void GenerateNewCrossword()
    {
        InitializeGrid();
        ClearGrid();


        var entries = CrosswordUtils.ReadDatabaseFromFile();
        GenerateCrossword(4, entries);
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


    void InitializeGrid()
    {
        // Initialize the 2D array based on rows and columns
        grid = new GeneratedEntry[MaxGridX, MaxGridY];

        int childIndex = 0;

        // Iterate through each row and column to fill the grid
        for (int y = 0; y < MaxGridY; y++)
        {
            for (int x = 0; x < MaxGridX; x++)
            {
                Transform child = transform.GetChild(childIndex); // Get the child at the current index
                grid[x, y] = child.GetComponent<GeneratedEntry>(); // Assign the GridCell component to the array
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


    public void GenerateCrossword(int min, CrosswordDatabase database)
    {
        var answers = database.Entries.Where(t => (t.answer.Length >= min)).ToArray();

        // Try to place the word in each cell in both orientations
        for (int y = 0; y < grid.GetLength(1); y++)
        {
            for (int x = 0; x < grid.GetLength(0); x++)
            {
                // Check if the word can fit horizontally

                for (int i = 0; i < answers.Length; i++)
                {
                    var word = answers[i];
                    if (CanFitWord(word.answer, x, y, true) && !usedAnswers.Contains(word.answer) && UsedHorizontalEntries.Count() < MaxHorzAndVert)
                    {
                        CrosswordEntryPositional newEntryData = new CrosswordEntryPositional()
                        {
                            StartX = x, StartY = y, isHorizontal = true, entry = word,
                        };
                        PlaceWord(newEntryData);
                        //     grid[x, y].SetCell(newEntryData);
                        UsedHorizontalEntries.Add(newEntryData);
                        usedAnswers.Add(word.answer);
                        continue;
                    }

                    if (CanFitWord(word.answer, x, y, false) && !usedAnswers.Contains(word.answer) && UsedVerticalEntries.Count() < MaxHorzAndVert)
                    {
                        CrosswordEntryPositional newEntryData = new CrosswordEntryPositional()
                        {
                            StartX = x, StartY = y, isHorizontal = false, entry = word,
                        };

                        PlaceWord(newEntryData);
                        //   grid[x, y].Init(newEntryData);
                        UsedVerticalEntries.Add(newEntryData);
                        usedAnswers.Add(word.answer);
                    }
                }
            }
        }

        if (UsedVerticalEntries.Count() == MaxHorzAndVert && UsedHorizontalEntries.Count() == MaxHorzAndVert)
        {
            TurnOfFUnusedElements();

            return;
        }

        var threeLetterAnswers = database.Entries.Where(t => (t.answer.Length < min)).ToArray();

        // Try to place the word in each cell in both orientations
        for (int y = 0; y < grid.GetLength(1); y++)
        {
            for (int x = 0; x < grid.GetLength(0); x++)
            {
                // Check if the word can fit horizontally


                for (int i = 0; i < threeLetterAnswers.Length; i++)
                {
                    var word = threeLetterAnswers[i];
                    if (CanFitWord(word.answer, x, y, true) && !usedAnswers.Contains(word.answer) && UsedHorizontalEntries.Count() < MaxHorzAndVert)
                    {
                        CrosswordEntryPositional newEntryData = new CrosswordEntryPositional()
                        {
                            StartX = x, StartY = y, isHorizontal = true, entry = word,
                        };
                        PlaceWord(newEntryData);
                        //     grid[x, y].SetCell(newEntryData);
                        UsedHorizontalEntries.Add(newEntryData);
                        usedAnswers.Add(word.answer);
                        continue;
                    }

                    if (CanFitWord(word.answer, x, y, false) && !usedAnswers.Contains(word.answer) && UsedVerticalEntries.Count() < MaxHorzAndVert)
                    {
                        CrosswordEntryPositional newEntryData = new CrosswordEntryPositional()
                        {
                            StartX = x, StartY = y, isHorizontal = false, entry = word,
                        };

                        PlaceWord(newEntryData);
                        //   grid[x, y].Init(newEntryData);
                        UsedVerticalEntries.Add(newEntryData);
                        usedAnswers.Add(word.answer);
                    }
                }
            }
        }

        TurnOfFUnusedElements();
        
    }


    public bool CanFitWord(string word, int startX, int startY, bool isHorizontal)
    {
        int length = word.Length;
        int maxX = grid.GetLength(0) - 1;
        int maxY = grid.GetLength(1) - 1;

        // Check bounds and ensure no conflicts before/after the word
        if (isHorizontal)
        {
            if (startX + length > maxX + 1) // Check if the word exceeds the grid's maximum width
            {
                Debug.LogWarning($"{word} exceeds the grid's maximum width {startX + length} is greater than {maxX}");
                return false;
            }

            if (startX > 0 && grid[startX - 1, startY].HasLetter) // Check if the cell before the word has a letter
            {
                Debug.LogWarning($"{word} cell before has letter ");
                return false;
            }

            if (startX + length <= maxX && grid[startX + length, startY].HasLetter) // Check if the cell after the word has a letter
            {
                Debug.LogWarning($"{word} cell after has letter ");
                return false;
            }
        }
        else
        {
            if (startY + length > maxY+1)
            {
                Debug.LogWarning($"{word} exceeds the grid's maximum width {startY + length} is greater than {maxY}");
                return false;
            }

            if (startY > 0 && grid[startX, startY - 1].HasLetter)
            {
                Debug.LogWarning($"{word} cell before has letter ");
                return false;
            }

            if (startY + length <= maxY && grid[startX, startY + length].HasLetter)
            {
                Debug.LogWarning($"{word} cell after has letter ");
                return false;
            }
        }


        // Check each cell in the word's path and its neighbors
        for (int i = 0; i < length; i++)
        {
            int x = isHorizontal ? startX + i : startX;
            int y = isHorizontal ? startY : startY + i;

            // Check if current cell has conflicting letters
            if (grid[x, y].HasLetter && grid[x, y].GetCell() != word[i])
            {
                string horz = isHorizontal ? "horz" : "vert";
                Debug.LogWarning($"{word} has neighbor issue when placing {horz}, {grid[x, y].GetCell()} dont equal {word[i]}");
                return false;
            }

            bool intersecting = grid[x, y].GetCell() == word[i];

            if (!intersecting)
            {
                if (!IsValidNeighbor(x, y, isHorizontal))
                {
                    string horz = isHorizontal ? "horz" : "vert";
                    Debug.LogWarning($"{word} has not a valid neighbor when placing {horz}");
                    return false;
                }
            }
        }

        return true;
    }

// Helper method to place a word in the grid once we've found a valid spot
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

            grid[x, y].SetCell(entryInfo.entry.answer[i], entryInfo); // Assuming SetLetter is a method in GridCell to assign a letter

            if (entryInfo.isHorizontal)
            {
                // var gridsee = grid[startX + 1, y];
                //
                // if (gridsee != null)
                // {
                //     grid[startX + 1, y].SetAllowedEntry(true);
                // }
            }
            else
            {
                // var gridsee = grid[startX, y+1];
                //
                // if (gridsee != null)
                // {
                //     grid[startX, y+1].SetAllowedEntry(false);
                // }
            }
        }

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

    private bool IsValidNeighbor(int x, int y, bool isHorizontal)
    {
        int maxX = grid.GetLength(0) - 1;
        int maxY = grid.GetLength(1) - 1;

        if (!isHorizontal)
        {
            if ((x > 0 && grid[x - 1, y].HasLetter) || (x < maxX && grid[x + 1, y].HasLetter))
            {
                return false;
            }
        }
        else
        {
            if (y > 0 && grid[x, y - 1].HasLetter || // Above
                (y < maxY && grid[x, y + 1].HasLetter))
            {
                return false;
            }
        }


        return true;
    }
}