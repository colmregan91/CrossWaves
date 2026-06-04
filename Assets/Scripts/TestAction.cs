using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

public class TestAction : MonoBehaviour
{
    

    [ContextMenu("Reset All Progress")]
    public void ResetAllProgress()
    {
        string crosswordsPath = Path.Combine(Application.persistentDataPath, "Crosswords");
        if (!Directory.Exists(crosswordsPath)) return;

        foreach (string file in Directory.GetFiles(crosswordsPath, "*.json"))
        {
            CrosswordStructure cw = JsonUtility.FromJson<CrosswordStructure>(File.ReadAllText(file));
            foreach (var entry in cw.horizontalEntries) entry.IsEntryFilled = false;
            foreach (var entry in cw.verticalEntries) entry.IsEntryFilled = false;
            File.WriteAllText(file, JsonUtility.ToJson(cw, true));
            
        }

        FindObjectOfType<SelectCrosswordCanvasParent>().OnBeforeActive();

        Debug.Log("All crossword progress reset.");
    }
    [ContextMenu("Complete Crossword")]
    public void CompleteCrossword()
    {
        StartCoroutine(CompleteCrosswordRoutine());
    }

    private IEnumerator CompleteCrosswordRoutine()
    {
        int count = CrosswordManager.Instance.getQuestions().Count(q => !q.IsEntryFilled);
        for (int i = 0; i < count; i++)
        {
            yield return new WaitUntil(() => !LetterWheel.Instance.IsSpinning());
            CrosswordManager.Instance.RevealWord();
        }
    }

}
