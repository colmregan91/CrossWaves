using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Random = System.Random;

public class LetterManager : MonoBehaviour
{
    public float radius;
    private CollectionOrganiser<Letter> _letterCollection;
    public AnchoredPositionsManager apm;
    
    public static string answerWord;

    public float inSpeed;
    public float outSpeed;
    private void Awake()
    {
        _letterCollection = new CollectionOrganiser<Letter>("Letter", transform);
    }
    
    private void Start()
    {
        Process();
    }


    public void SpinWheel()
    {
    
        List<char> letters = new List<char>(answerWord.ToCharArray());
        apm.ArrangeAnchoredPositions(letters.Count, radius);
        ProcessLetters(letters);
    }

    public void Process()
    {
            List<string> words = new List<string>() { "colin", "football", "sexy", "London" };
            var rand = UnityEngine.Random.Range(0, words.Count);
            answerWord = words[rand];
            SpinWheel();

    }
    

    public  void SortLetterWheel()
    {

        if (answerWord == string.Empty)
        {
            Process();
        }
        else
        {
            answerWord = string.Empty;
            ClearWheel();
        }
    }

    private void ClearWheel()
    {
        var count = _letterCollection.DisplayCount;
        int completedSequences = 0;
        for (int i = 0; i < count; i++)
        {
            var letter = _letterCollection.DisplayList[i];
            MoveThroughPoints(letter, outSpeed,count,null,()=>
            {
               
                letter.Deinit();
                _letterCollection.ReturnToQueue(letter);
                completedSequences++;
                if (completedSequences == count)
                {
                    apm.ResetAnchoredPositions();
                }
            });
        }
    }

    private void ProcessLetters(List<char> remainingLetters)
    {
        if (remainingLetters.Count == 0)
        {
            return;
        }
        
        var count = remainingLetters.Count;
  
            int randomIndex = UnityEngine.Random.Range(0, remainingLetters.Count);
            char selectedLetter = remainingLetters[randomIndex];
            var letter = GetRandomLetter(selectedLetter);
            letter.transform.position = apm.GetStartIndex().position;
            MoveThroughPoints(letter, inSpeed,count,() =>
            {
                remainingLetters.RemoveAt(randomIndex);
                ProcessLetters(remainingLetters);
            }, ()=> letter.SetAnchoredPositionIndex( count));
    }
    
    private Letter GetRandomLetter(char letter)
    {
        Letter letterObj = _letterCollection.AddOrDequeue();
        letterObj.gameObject.SetActive(true);         
        letterObj.Init(letter); // call from coll organiser
        return letterObj;
    }
    
    private void MoveThroughPoints(Letter letter,float speed,int count, Action callback, Action onCompleteCallback)
    {
        Sequence moveSequence = DOTween.Sequence();
        var anchoredList = apm.GetAnchoredList();

        for (int i = letter.GetAnchoredPositionIndex(); i < count; i++)
        {
            var waypoint = anchoredList[i];
            
            var tween =letter.transform.DOMove(waypoint.position, speed).SetEase(Ease.Linear);
            
            if (i == 0) 
            {
                tween.OnComplete(() =>
                {
                    callback?.Invoke();
                });
            }
            
            moveSequence.Append(tween);
        }
        
        moveSequence.OnComplete(() =>
        {
            onCompleteCallback?.Invoke();
        });
        
        moveSequence.Play();
    }
}


