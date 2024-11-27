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

    
    private CollectionOrganiser<Letter> _letterCollection;
    public AnchoredPositionsManager apm;

    public bool isSpinning;


    private void Awake()
    {
        _letterCollection = new CollectionOrganiser<Letter>("Letter", transform);
    }
    
    private void OnEnable()
    {
        LetterInputManager.Instance.OnLetterSelected += HandleLetterSelected;
    }

    private void HandleLetterSelected(Letter obj)
    {
        obj.SetSelected(true);
    }

    public void UnselectLetters()
    {
        foreach (var letter in _letterCollection.DisplayList)
        {
            letter.SetSelected(false);
        }
    }

    public void SpinWheel(string s)
    {
    
        List<char> letters = new List<char>(s.ToCharArray());
        
        apm.ArrangeAnchoredPositions(letters.Count);
        isSpinning = true;
        ProcessLetters(letters);
    }
    

    public void ClearWheel()
    {
   
        var count = _letterCollection.DisplayCount;
        int completedSequences = 0;
        for (int i = 0; i < count; i++)
        {
            isSpinning = true;
            var letter = _letterCollection.DisplayList[i];
            MoveThroughPoints(letter, LetterWheel.OutSpeed,count,null,()=>
            {
               
                letter.Deinit();
                _letterCollection.ReturnToQueue(letter);
                completedSequences++;
                if (completedSequences == count)
                {
                    isSpinning = false;
                    LetterWheel.Instance.OnWheelCleared?.Invoke();
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
            
            
            MoveThroughPoints(letter, LetterWheel.InSpeed,  count ,() =>
            {
                remainingLetters.RemoveAt(randomIndex);
                ProcessLetters(remainingLetters);
            }, ()=>
            {
                if (count == 1)
                {
                    isSpinning = false;
                }
                letter.SetAnchoredPositionIndex(count);
            });
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

    private void OnDisable()
    {
        LetterInputManager.Instance.OnLetterSelected -= HandleLetterSelected;
    }
}


