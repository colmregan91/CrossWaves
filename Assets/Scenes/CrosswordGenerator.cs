using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class CrosswordGenerator : MonoBehaviour
{
    public GeneratedEntry[,] grid;

    private int MaxGridX = 13;
    private int MaxGridY = 13;

    public List<CrosswordEntryPositional> UsedHorizontalEntries = new List<CrosswordEntryPositional>();
    public List<CrosswordEntryPositional> UsedVerticalEntries = new List<CrosswordEntryPositional>();

    public string SelectedDifficulty = "easy";
    public int CrosswordsToGenerate = 10;

    public void GenerateNewCrossword()
    {
        StartCoroutine(GenerateBatch(CrosswordsToGenerate));
    }

    public void SaveNewCrossword()
    {
        CrosswordUtils.SaveNewCrossword(SelectedDifficulty, UsedHorizontalEntries, UsedVerticalEntries);
    }

    private IEnumerator GenerateBatch(int count)
    {
        string path = Path.Combine(Application.persistentDataPath, "Grids", "GridTemplateNormals.json");
        string json = File.ReadAllText(path);
        var templateFile = JsonUtility.FromJson<GridTemplateFile>(json);

        var database = CrosswordUtils.ReadDatabaseFromFile();
        var usedTemplateIds = LoadUsedTemplateIds(SelectedDifficulty);

        for (int i = 0; i < count; i++)
        {
            var availableTemplates = templateFile.grids
                .Where(t => !usedTemplateIds.Contains(t.id))
                .ToArray();

            if (availableTemplates.Length == 0)
            {
                usedTemplateIds.Clear();
                availableTemplates = templateFile.grids;
                Debug.Log("All templates used — resetting template pool.");
            }

            var savedAnswers = CrosswordUtils.GetSavedAnswers(SelectedDifficulty);
            var poolsByLength = database.Entries
                .Where(e => e.difficulty == SelectedDifficulty
                         && !e.isUsed
                         && !savedAnswers.Contains(e.answer)
                         && e.answer.Length >= 3
                         && e.answer.Length <= MaxGridX)
                .GroupBy(e => e.answer.Length)
                .ToDictionary(g => g.Key, g => g.ToList());

            InitializeGrid();

            bool filled = false;
            GridTemplateDef usedTemplate = null;
            while (!filled)
            {
                usedTemplate = availableTemplates[Random.Range(0, availableTemplates.Length)];

                ClearGrid();
                ApplyTemplate(usedTemplate);

                UsedHorizontalEntries.Clear();
                UsedVerticalEntries.Clear();

                foreach (var pool in poolsByLength.Values)
                    Shuffle(pool);

                filled = TryFillTemplate(usedTemplate, poolsByLength);

                if (!filled)
                    yield return null;
            }

            usedTemplateIds.Add(usedTemplate.id);
            SaveUsedTemplateIds(SelectedDifficulty, usedTemplateIds);

            foreach (var entry in UsedHorizontalEntries.Concat(UsedVerticalEntries))
                PlaceWord(entry);

            CrosswordUtils.SaveNewCrossword(SelectedDifficulty, UsedHorizontalEntries, UsedVerticalEntries);
            database = CrosswordUtils.ReadDatabaseFromFile();

            Debug.Log($"Crossword {i + 1}/{count} saved (template {usedTemplate.id}).");
            yield return null;
        }

        Debug.Log($"Batch complete: {count} crosswords saved.");
    }

    private bool TryFillTemplate(GridTemplateDef template, Dictionary<int, List<CrosswordEntry>> poolsByLength)
    {
        var letterGrid = new char[MaxGridX, MaxGridY];
        var remaining = template.slots.ToList();
        var usedAnswers = new HashSet<string>();

        while (remaining.Count > 0)
        {
            var slot = remaining
                .OrderByDescending(s => CountKnownLetters(s, letterGrid))
                .ThenByDescending(s => s.length)
                .First();

            if (!poolsByLength.TryGetValue(slot.length, out var candidates))
                return false;

            CrosswordEntry match = null;
            foreach (var entry in candidates)
            {
                if (!usedAnswers.Contains(entry.answer) && MatchesLetterGrid(entry.answer, slot, letterGrid))
                {
                    match = entry;
                    break;
                }
            }

            if (match == null) return false;

            PlaceWordInGrid(match.answer, slot, letterGrid);
            usedAnswers.Add(match.answer);

            var positional = new CrosswordEntryPositional
            {
                StartX = slot.col,
                StartY = slot.row,
                isHorizontal = slot.dir == "across",
                entry = match
            };

            if (slot.dir == "across") UsedHorizontalEntries.Add(positional);
            else UsedVerticalEntries.Add(positional);

            remaining.Remove(slot);
        }

        return true;
    }

    private int CountKnownLetters(GridSlotDef slot, char[,] letterGrid)
    {
        bool isH = slot.dir == "across";
        int count = 0;
        for (int i = 0; i < slot.length; i++)
        {
            int x = isH ? slot.col + i : slot.col;
            int y = isH ? slot.row : slot.row + i;
            if (letterGrid[x, y] != '\0') count++;
        }
        return count;
    }

    private bool MatchesLetterGrid(string word, GridSlotDef slot, char[,] letterGrid)
    {
        bool isH = slot.dir == "across";
        for (int i = 0; i < word.Length; i++)
        {
            int x = isH ? slot.col + i : slot.col;
            int y = isH ? slot.row : slot.row + i;
            if (letterGrid[x, y] != '\0' && letterGrid[x, y] != word[i])
                return false;
        }
        return true;
    }

    private void PlaceWordInGrid(string word, GridSlotDef slot, char[,] letterGrid)
    {
        bool isH = slot.dir == "across";
        for (int i = 0; i < word.Length; i++)
        {
            int x = isH ? slot.col + i : slot.col;
            int y = isH ? slot.row : slot.row + i;
            letterGrid[x, y] = word[i];
        }
    }

    private void ApplyTemplate(GridTemplateDef template)
    {
        for (int y = 0; y < template.rows.Length; y++)
            for (int x = 0; x < template.rows[y].Length; x++)
                if (template.rows[y][x] == '#')
                    grid[x, y].TurnOffGridElement();
    }

    private HashSet<int> LoadUsedTemplateIds(string difficulty)
    {
        string filePath = Path.Combine(Application.persistentDataPath, "Grids", $"usedTemplates_{difficulty}.json");
        if (!File.Exists(filePath)) return new HashSet<int>();
        var data = JsonUtility.FromJson<UsedTemplateIdList>(File.ReadAllText(filePath));
        return new HashSet<int>(data.ids);
    }

    private void SaveUsedTemplateIds(string difficulty, HashSet<int> ids)
    {
        string filePath = Path.Combine(Application.persistentDataPath, "Grids", $"usedTemplates_{difficulty}.json");
        var data = new UsedTemplateIdList { ids = ids.ToList() };
        File.WriteAllText(filePath, JsonUtility.ToJson(data));
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private void PlaceWord(CrosswordEntryPositional entryInfo)
    {
        for (int i = 0; i < entryInfo.entry.answer.Length; i++)
        {
            int x = entryInfo.isHorizontal ? entryInfo.StartX + i : entryInfo.StartX;
            int y = entryInfo.isHorizontal ? entryInfo.StartY : entryInfo.StartY + i;
            if (grid[x, y].HasLetter) continue;
            grid[x, y].SetCell(entryInfo.entry.answer[i], entryInfo);
        }
    }

    private void InitializeGrid()
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
    }
}

[System.Serializable]
class UsedTemplateIdList
{
    public List<int> ids = new List<int>();
}

[System.Serializable]
class GridSlotDef
{
    public int number;
    public string dir;
    public int row, col, length;
}

[System.Serializable]
class GridTemplateDef
{
    public int id;
    public string[] rows;
    public GridSlotDef[] slots;
}

[System.Serializable]
class GridTemplateFile
{
    public GridTemplateDef[] grids;
}
