using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;

public static class CrosswordUtils
{
    static string CrosswordsPath => Path.Combine(Application.persistentDataPath, "Crosswords");
    static string DatabasePath => Path.Combine(Application.persistentDataPath, "CrosswordDatabase.json");
    
    public static string GetCrosswordPath()
    {
        string[] files = Directory.GetFiles(CrosswordsPath);
        string NewCrosswordPath = Path.Combine(Application.persistentDataPath, $"Crosswords/{files.Length + 1}.json");
        Debug.Log(NewCrosswordPath);
        return NewCrosswordPath;
    }

    public static  CrosswordStructure LoadCrosswordFromFile(string num)
    {

        string json = File.ReadAllText(Path.Combine(CrosswordsPath, num));
        CrosswordStructure crossword = JsonUtility.FromJson<CrosswordStructure>(json); 
        
        return crossword; 
    }
    
    
    public static CrosswordDatabase ReadDatabaseFromFile()
    {
        string json = File.ReadAllText(DatabasePath);
        CrosswordDatabase crosswordEntryBase = JsonUtility.FromJson<CrosswordDatabase>(json);
        return crosswordEntryBase;
    }
    
    
    public static void WriteNewCrosswordToFile(CrosswordStructure newCrossword)
    {
        var json = JsonUtility.ToJson(newCrossword, true);
        File.WriteAllText(GetCrosswordPath(), json);
    }
    
    
    public static void SaveNewCrossword(List<CrosswordEntryPositional> horizontal,List<CrosswordEntryPositional> vertical )
    {
        CrosswordStructure newCrosswordStructure = new CrosswordStructure() { horizontalEntries = horizontal, verticalEntries = vertical };
        WriteNewCrosswordToFile(newCrosswordStructure);


        // remove used from all entries
        CrosswordDatabase NewEntries = ReadDatabaseFromFile();

        for (int i = 0; i < horizontal.Count; i++)
        {
            var f = NewEntries.Entries.FirstOrDefault(T => T.question.Equals(horizontal[i].entry.question));

            if (f != null)
            {
                NewEntries.Entries.Remove(f);
            }
        }
        
        for (int i = 0; i < vertical.Count; i++)
        {
            var f = NewEntries.Entries.FirstOrDefault(T => T.question.Equals(vertical[i].entry.question));

            if (f != null)
            {
                NewEntries.Entries.Remove(f);
            }
        }

        CrosswordDatabase newBase = new CrosswordDatabase() { Entries = NewEntries.Entries };
        Debug.Log(newBase.Entries[0].question);

        WriteNewDatabaseToFile(newBase);
    }
    
    public static void WriteNewDatabaseToFile(CrosswordDatabase newDatabase)
    {
        var json = JsonUtility.ToJson(newDatabase, true);
        File.WriteAllText(DatabasePath, json);
    }
}