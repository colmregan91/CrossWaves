using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;

public static class CrosswordUtils
{
    static string CrosswordsPath => Path.Combine(Application.persistentDataPath, "Crosswords");
    static string DatabasePath => Path.Combine(Application.persistentDataPath, "CrosswordDatabase.json");
    
    
    public static string GetCrosswordPath(string difficulty, int number)
    {
        return Path.Combine(CrosswordsPath, $"{difficulty}cw_{number}.json");
    }

    public static void SaveProgress(string difficulty, int num, List<CrosswordEntryPositional> progress)
    {
        CrosswordStructure newCrosswordStructure = new CrosswordStructure() { crosswordNumber = num, horizontalEntries = progress.Take(10).ToList(), verticalEntries = progress.Skip(10).ToList() };
        var json = JsonUtility.ToJson(newCrosswordStructure, true);
        File.WriteAllText(GetCrosswordPath(difficulty, num), json);
    }

    public static CrosswordStructure LoadCrosswordFromFile(string difficulty, int number)
    {
        string json = File.ReadAllText(GetCrosswordPath(difficulty, number));
        CrosswordStructure crossword = JsonUtility.FromJson<CrosswordStructure>(json);
        return crossword;
    }
    
    
    public static HashSet<string> GetSavedAnswers(string difficulty, int maxRecent = 0)
    {
        var used = new HashSet<string>();
        if (!Directory.Exists(CrosswordsPath)) return used;

        var files = Directory.GetFiles(CrosswordsPath, $"{difficulty}cw_*.json");

        if (maxRecent > 0)
        {
            files = files
                .OrderByDescending(f => {
                    var name = Path.GetFileNameWithoutExtension(f);
                    var numStr = name.Substring(difficulty.Length + 2);
                    return int.TryParse(numStr, out int n) ? n : 0;
                })
                .Take(maxRecent)
                .ToArray();
        }

        foreach (string file in files)
        {
            string json = File.ReadAllText(file);
            CrosswordStructure cw = JsonUtility.FromJson<CrosswordStructure>(json);
            foreach (var e in cw.horizontalEntries) used.Add(e.entry.answer);
            foreach (var e in cw.verticalEntries) used.Add(e.entry.answer);
        }
        return used;
    }

    public static CrosswordDatabase ReadDatabaseFromFile()
    {
        string json = File.ReadAllText(DatabasePath);
        CrosswordDatabase crosswordEntryBase = JsonUtility.FromJson<CrosswordDatabase>(json);
        return crosswordEntryBase;
    }
    
    
    public static void WriteNewCrosswordToFile(string difficulty, CrosswordStructure newCrossword)
    {
        var json = JsonUtility.ToJson(newCrossword, true);
        File.WriteAllText(GetCrosswordPath(difficulty, newCrossword.crosswordNumber), json);
    }
    
    
    public static void SaveNewCrossword(string difficulty, List<CrosswordEntryPositional> horizontal, List<CrosswordEntryPositional> vertical)
    {
        Directory.CreateDirectory(CrosswordsPath);
        string[] files = Directory.GetFiles(CrosswordsPath, $"{difficulty}cw_*.json");
        var newCrosswordStructure = new CrosswordStructure() { crosswordNumber = files.Length + 1, horizontalEntries = horizontal, verticalEntries = vertical };

        WriteNewCrosswordToFile(difficulty, newCrosswordStructure);

        CrosswordDatabase db = ReadDatabaseFromFile();

        foreach (var positional in horizontal.Concat(vertical))
        {
            foreach (var match in db.Entries.Where(e => e.answer.Equals(positional.entry.answer)
                                                     && e.question.Equals(positional.entry.question)))
                match.isUsed = true;
        }

        WriteNewDatabaseToFile(db);
    }
    
    public static void AddEntryToDatabase(CrosswordEntry entry)
    {
        CrosswordDatabase db = ReadDatabaseFromFile();
        bool exists = db.Entries.Any(e => e.answer.Equals(entry.answer));
        if (!exists)
        {
            db.Entries.Add(entry);
            WriteNewDatabaseToFile(db);
        }
    }

    public static void WriteNewDatabaseToFile(CrosswordDatabase newDatabase)
    {
        var json = JsonUtility.ToJson(newDatabase, true);
        File.WriteAllText(DatabasePath, json);
    }
    
    // public static   List<bool>  ReadProgressFromFile()
    // {
    //     string json = File.ReadAllText(ProgressPath);
    //     List<bool> crosswordProgress = JsonUtility.FromJson< List<bool>>(json);
    //     return crosswordProgress;
    // }
    //
    // public static bool IsThereProgress(int num)
    // {
    //     return File.Exists(ProgressPath);
    // }
    
}
