using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class LetterWheel : MonoSingleton<LetterWheel>
{
    public Action OnWheelCleared;
    public LetterManager lm1;
    
    public LetterManager lm2;
    public static string answerWord=" ";

    public static LetterManager curLm;

    public static float InSpeed = 0.05f;
    public static float OutSpeed = 0.05f;
    
    char[] alphabet = new char[]
    {
        'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 
        'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'
    };
    
    
    public  void SortLetterWheel(CrosswordEntryPositional entryData)
    {
        curLm.UnselectLetters();
        if (answerWord.Equals(entryData.entry.answer))
        {
            return;
        }
        
        curLm.ClearWheel();

        if (entryData.IsEntryFilled)
        {
            answerWord = string.Empty;
            return;
        }
        curLm = nextLm();

        answerWord = entryData.entry.answer;
        string word = answerWord;
        int diff = 7 - word.Length;

        if (diff > 0)
        {
            for (int i = 0; i < diff; i++)
            {
                word += alphabet[UnityEngine.Random.Range(0, alphabet.Length)];
            }
        }
  
        curLm.SpinWheel(word);
    }

    private void OnEnable()
    {
        curLm = lm1;
        LetterInputManager.Instance.OnCorrectAnswer += ClearWheel;
        CrosswordManager.Instance.OnNewWordClicked += SortLetterWheel;

    }

    public void ClearSelection()
    {
        CrosswordManager.Instance.Reselect();
    }

    public bool IsSpinning()
    {
        return lm1.isSpinning == true || lm2.isSpinning == true;
    }

    public void ClearWheel()
    {
        curLm.UnselectLetters();
        curLm.ClearWheel();
    }

    private void SortLetterWheel(List<CrosswordGridEntry> arg1, CrosswordEntryPositional arg2)
    {
        SortLetterWheel(arg2);
    }


    LetterManager nextLm()
    {
        // se interactability here
        return curLm == lm1 ? lm2 : lm1;
    }
    
    private void OnDisable()
    {
        answerWord = string.Empty;
        curLm = null;
        LetterInputManager.Instance.OnCorrectAnswer -= ClearWheel;
        CrosswordManager.Instance.OnNewWordClicked -= SortLetterWheel;

    }
}
