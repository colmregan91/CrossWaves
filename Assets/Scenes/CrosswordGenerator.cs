using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CrosswordGenerator : MonoBehaviour
{
    public CrosswordDatabase CrosswordEntryBase;
    public static List<CrosswordEntry> AllEntries = new List<CrosswordEntry>();

    public GeneratedEntry[,] grid;

    private int MaxGridX = 13;
    private int MaxGridY = 13;
    private const int MinWordCount = 12;
    private int minHWords = 12;
    private int minVWords = 12;

    public List<CrosswordEntryPositional> UsedHorizontalEntries = new List<CrosswordEntryPositional>();
    public List<CrosswordEntryPositional> UsedVerticalEntries = new List<CrosswordEntryPositional>();

    public CrosswordEntry AttemptToFit;
    private HashSet<string> usedAnswers = new HashSet<string>();
    public string SelectedDifficulty = "easy";
    public int CrosswordsToGenerate = 10;

    private Dictionary<char, List<(int x, int y)>> letterCells = new Dictionary<char, List<(int x, int y)>>();

    // Manual editor tool: select a cell, assign AttemptToFit, click TryFit
    public void TryFit()
    {
        bool wordPlaced = false;
        for (int y = 0; y < grid.GetLength(1) && !wordPlaced; y++)
        {
            for (int x = 0; x < grid.GetLength(0) && !wordPlaced; x++)
            {
                if (!grid[x, y].IsSelected) continue;

                if (CanFitWord(AttemptToFit.answer, x, y, true, false)
                    && !usedAnswers.Contains(AttemptToFit.answer)
                    && UsedHorizontalEntries.Count < MinWordCount)
                {
                    wordPlaced = true;
                    var entry = new CrosswordEntryPositional { StartX = x, StartY = y, isHorizontal = true, entry = new CrosswordEntry(AttemptToFit.question, AttemptToFit.answer) };
                    PlaceWord(entry);
                    UsedHorizontalEntries.Add(entry);
                    usedAnswers.Add(AttemptToFit.answer);
                    CrosswordUtils.AddEntryToDatabase(AttemptToFit);
                    grid[x, y].Unselect();
                }
                else if (CanFitWord(AttemptToFit.answer, x, y, false, false)
                    && !usedAnswers.Contains(AttemptToFit.answer)
                    && UsedVerticalEntries.Count < MinWordCount)
                {
                    wordPlaced = true;
                    var entry = new CrosswordEntryPositional { StartX = x, StartY = y, isHorizontal = false, entry = new CrosswordEntry(AttemptToFit.question, AttemptToFit.answer) };
                    PlaceWord(entry);
                    UsedVerticalEntries.Add(entry);
                    usedAnswers.Add(AttemptToFit.answer);
                    CrosswordUtils.AddEntryToDatabase(AttemptToFit);
                    grid[x, y].Unselect();
                }
            }
        }
    }

    public void SaveNewCrossword()
    {
        CrosswordUtils.SaveNewCrossword(SelectedDifficulty, UsedHorizontalEntries, UsedVerticalEntries);
    }

    public void GenerateNewCrossword()
    {
        StartCoroutine(GenerateBatch(CrosswordsToGenerate));
    }

    private IEnumerator GenerateBatch(int count)
    {
        var database = CrosswordUtils.ReadDatabaseFromFile();

        for (int i = 0; i < count; i++)
        {
            minHWords = Random.Range(10, 15);
            minVWords = 24 - minHWords;
            int framesSinceStart = 0;

            var savedAnswers = CrosswordUtils.GetSavedAnswers(SelectedDifficulty);
            var eligible = database.Entries
                .Where(e => e.answer.Length >= 3 && e.answer.Length <= MaxGridX && e.difficulty == SelectedDifficulty && !e.isUsed && !savedAnswers.Contains(e.answer));

            var wordPool = eligible
                .Where(e => e.answer.Length >= 4)
                .OrderByDescending(e => e.answer.Length)
                .ToList();

            var shortPool = eligible
                .Where(e => e.answer.Length == 3)
                .ToList();

            InitializeGrid();

            // Run up to 200 attempts per frame — keeps Unity responsive without yielding every attempt
            while (!MeetsMinimum())
            {
                // If an asymmetric target is proving too hard, fall back to balanced
                if (framesSinceStart > 20 && (minHWords != 12 || minVWords != 12))
                {
                    minHWords = 12;
                    minVWords = 12;
                }

                for (int attempt = 0; attempt < 200 && !MeetsMinimum(); attempt++)
                {
                    ClearGrid();
                    UsedHorizontalEntries.Clear();
                    UsedVerticalEntries.Clear();
                    usedAnswers.Clear();
                    GenerateCrossword(wordPool, shortPool);
                }

                framesSinceStart++;
                yield return null;
            }

            TurnOfFUnusedElements();
            CrosswordUtils.SaveNewCrossword(SelectedDifficulty, UsedHorizontalEntries, UsedVerticalEntries);
            database = CrosswordUtils.ReadDatabaseFromFile();

            Debug.Log($"Crossword {i + 1}/{count} saved.");
            yield return null;
        }

        Debug.Log($"Batch complete: {count} crosswords saved.");
    }

    private void GenerateCrossword(List<CrosswordEntry> wordPool, List<CrosswordEntry> shortPool)
    {
        if (wordPool.Count == 0) return;

        // Pick a random word from the longest available to seed variety across attempts
        int firstPick = Random.Range(0, Mathf.Min(5, wordPool.Count));
        var first = wordPool[firstPick];

        // Bias seed direction toward whichever orientation has the larger target
        bool startHorizontal = Random.value < (float)minHWords / (minHWords + minVWords);
        int firstStartX, firstStartY;
        if (startHorizontal)
        {
            firstStartX = Mathf.Clamp((MaxGridX - first.answer.Length) / 2 + Random.Range(-2, 3), 0, MaxGridX - first.answer.Length);
            firstStartY = Random.Range(2, MaxGridY - 2);
        }
        else
        {
            firstStartX = Random.Range(2, MaxGridX - 2);
            firstStartY = Mathf.Clamp((MaxGridY - first.answer.Length) / 2 + Random.Range(-2, 3), 0, MaxGridY - first.answer.Length);
        }

        var firstEntry = new CrosswordEntryPositional { StartX = firstStartX, StartY = firstStartY, isHorizontal = startHorizontal, entry = first };
        PlaceWord(firstEntry);
        if (startHorizontal) UsedHorizontalEntries.Add(firstEntry);
        else UsedVerticalEntries.Add(firstEntry);
        usedAnswers.Add(first.answer);

        bool madeProgress = true;
        while (!MeetsMinimum() && madeProgress)
        {
            madeProgress = false;
            // Longest words first, random tiebreak — gives large words priority at every pass
            var candidates = wordPool
                .Where(e => !usedAnswers.Contains(e.answer))
                .OrderByDescending(e => e.answer.Length)
                .ThenBy(_ => Random.value)
                .ToList();

            foreach (var word in candidates)
            {
                if (MeetsMinimum()) break;
                if (TryPlaceAtIntersection(word))
                    madeProgress = true;
            }
        }

        // Only fall back to 3-letter words if 4+ letter words can't complete the grid
        if (!MeetsMinimum())
        {
            madeProgress = true;
            while (!MeetsMinimum() && madeProgress)
            {
                madeProgress = false;
                var candidates = shortPool
                    .Where(e => !usedAnswers.Contains(e.answer))
                    .OrderBy(_ => Random.value)
                    .ToList();

                foreach (var word in candidates)
                {
                    if (MeetsMinimum()) break;
                    if (TryPlaceAtIntersection(word))
                        madeProgress = true;
                }
            }
        }
    }

    public bool MeetsMinimum() =>
        UsedHorizontalEntries.Count >= minHWords && UsedVerticalEntries.Count >= minVWords;

    private bool TryPlaceAtIntersection(CrosswordEntry word)
    {
        bool needHorz = UsedHorizontalEntries.Count < minHWords;
        bool needVert = UsedVerticalEntries.Count < minVWords;

        // Randomly decide which direction to try first so growth isn't always biased
        bool tryHorzFirst = Random.value > 0.5f;

        var letterIndices = Enumerable.Range(0, word.answer.Length).OrderBy(_ => Random.value).ToList();

        foreach (int i in letterIndices)
        {
            char letter = word.answer[i];
            if (!letterCells.TryGetValue(letter, out var cells)) continue;

            foreach (var (gx, gy) in cells)
            {

                bool existingIsHorizontal = grid[gx, gy].entryInfo.isHorizontal;

                bool triedHorz = false, triedVert = false;

                for (int pass = 0; pass < 2; pass++)
                {
                    bool tryHorz = (pass == 0) ? tryHorzFirst : !tryHorzFirst;

                    if (tryHorz && !triedHorz && !existingIsHorizontal && needHorz)
                    {
                        triedHorz = true;
                        int sx = gx - i;
                        if (CanFitWord(word.answer, sx, gy, true))
                        {
                            var entry = new CrosswordEntryPositional { StartX = sx, StartY = gy, isHorizontal = true, entry = word };
                            PlaceWord(entry);
                            UsedHorizontalEntries.Add(entry);
                            usedAnswers.Add(word.answer);
                            return true;
                        }
                    }

                    if (!tryHorz && !triedVert && existingIsHorizontal && needVert)
                    {
                        triedVert = true;
                        int sy = gy - i;
                        if (CanFitWord(word.answer, gx, sy, false))
                        {
                            var entry = new CrosswordEntryPositional { StartX = gx, StartY = sy, isHorizontal = false, entry = word };
                            PlaceWord(entry);
                            UsedVerticalEntries.Add(entry);
                            usedAnswers.Add(word.answer);
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }

    private void TurnOfFUnusedElements()
    {
        for (int y = 0; y < MaxGridY; y++)
            for (int x = 0; x < MaxGridX; x++)
                if (!grid[x, y].HasLetter)
                    grid[x, y].TurnOffGridElement();
    }

    void InitializeGrid()
    {
        grid = new GeneratedEntry[MaxGridX, MaxGridY];
        int childIndex = 0;
        for (int y = 0; y < MaxGridY; y++)
        {
            for (int x = 0; x < MaxGridX; x++)
            {
                Transform child = transform.GetChild(childIndex);
                grid[x, y] = child.GetComponent<GeneratedEntry>();
                grid[x, y].Reset();
                childIndex++;
            }
        }
    }

    private void ClearGrid()
    {
        foreach (var cell in grid) cell.Reset();
        letterCells.Clear();
    }

    public bool CanFitWord(string word, int startX, int startY, bool isHorizontal, bool requireIntersection = true)
    {
        if (startX < 0 || startY < 0) return false;

        int length = word.Length;
        int maxX = grid.GetLength(0) - 1;
        int maxY = grid.GetLength(1) - 1;

        if (isHorizontal)
        {
            if (startX + length > maxX + 1) return false;
            if (startX > 0 && grid[startX - 1, startY].HasLetter) return false;
            if (startX + length <= maxX && grid[startX + length, startY].HasLetter) return false;
        }
        else
        {
            if (startY + length > maxY + 1) return false;
            if (startY > 0 && grid[startX, startY - 1].HasLetter) return false;
            if (startY + length <= maxY && grid[startX, startY + length].HasLetter) return false;
        }

        bool hasIntersection = false;

        for (int i = 0; i < length; i++)
        {
            int x = isHorizontal ? startX + i : startX;
            int y = isHorizontal ? startY : startY + i;

            if (grid[x, y].HasLetter)
            {
                if (grid[x, y].GetCell() != word[i]) return false;
                if (grid[x, y].entryInfo.isHorizontal == isHorizontal) return false;
                hasIntersection = true;
            }
            else if (!IsValidNeighbor(x, y, isHorizontal))
            {
                return false;
            }
        }

        return !requireIntersection || hasIntersection;
    }

    private void PlaceWord(CrosswordEntryPositional entryInfo)
    {
        for (int i = 0; i < entryInfo.entry.answer.Length; i++)
        {
            int x = entryInfo.isHorizontal ? entryInfo.StartX + i : entryInfo.StartX;
            int y = entryInfo.isHorizontal ? entryInfo.StartY : entryInfo.StartY + i;
            if (grid[x, y].HasLetter) continue;
            grid[x, y].SetCell(entryInfo.entry.answer[i], entryInfo);
            char c = entryInfo.entry.answer[i];
            if (!letterCells.TryGetValue(c, out var list))
            {
                list = new List<(int, int)>();
                letterCells[c] = list;
            }
            list.Add((x, y));
        }
    }

    private bool IsValidNeighbor(int x, int y, bool isHorizontal)
    {
        int maxX = grid.GetLength(0) - 1;
        int maxY = grid.GetLength(1) - 1;

        if (!isHorizontal)
            return !(x > 0 && grid[x - 1, y].HasLetter) && !(x < maxX && grid[x + 1, y].HasLetter);
        else
            return !(y > 0 && grid[x, y - 1].HasLetter) && !(y < maxY && grid[x, y + 1].HasLetter);
    }
}
