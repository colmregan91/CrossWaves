using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LetterWheel : MonoBehaviour
{
    public LetterManager lm1;
    
    public LetterManager lm2;
    public string answerWord;

    private LetterManager curLm;

    public static float InSpeed = 0.15f;
    public static float OutSpeed = 0.12f;
    
    public float delay;
    
    public  void SortLetterWheel()
    {

        if (answerWord == string.Empty)
        {
            List<string> words = new List<string>() {"Londonssloon",  "Londonnddwn" , "Londonnddwn" , "Lwdonnlondon" , "Lonnlonwddon" };
            var rand = UnityEngine.Random.Range(0, words.Count);
            curLm = lm1;
            answerWord = words[rand];
            curLm.Process(words[rand]);
        }
        else
        {
            curLm.ClearWheel();
            curLm = nextLm();
            
            List<string> words = new List<string>(){"London4vldon",  "Londvvonldon" , "ndonlvvondon" , "Ldonlondvvon" , "Londvvonldon" };
            var rand = UnityEngine.Random.Range(0, words.Count);
            answerWord = words[rand];
            Invoke("del", delay);
            
          
        }
    }

    private void del()
    {
        curLm.Process(answerWord);
    }

    LetterManager nextLm()
    {
        // se interactability here
        return curLm == lm1 ? lm2 : lm1;
    }
}
