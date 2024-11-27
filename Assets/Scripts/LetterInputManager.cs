using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;


public class LetterInputManager : MonoSingleton<LetterInputManager>
{
    // Start is called before the first frame update

    public  Action OnCorrectAnswer;
    private InputtedLetter[] AllInputLetters;
    public Action<Letter> OnLetterSelected;

    private InputtedLetter[] UsedLetters=> AllInputLetters.Where(t => t.gameObject.activeSelf && t.isShowing).ToArray();

    public float pulseDuration = 0.2f; // Duration of the pulsate effect
    public float pulseScale = 2f; // How much bigger the prefab grows
    public float delayBetween = 0.1f;
    private CollectionOrganiser<FloatedLetters> _floatedletterCollection;
    [SerializeField] private Transform floatedLettersHolder;

    private void Awake()
    {
        AllInputLetters = GetComponentsInChildren<InputtedLetter>();
        _floatedletterCollection = new CollectionOrganiser<FloatedLetters>("FloatedLetter", floatedLettersHolder);
    }


    private void Start()
    {
        CrosswordManager.Instance.OnNewWordClicked += ClearInputtedLetters;
        OnLetterSelected += HandleLetterInput;
    }


    private void Pulsate()
    {
        var pos = CrosswordManager.Instance.getWordPosition();

        var length = UsedLetters.Length;

        foreach (var let in UsedLetters)
        {
            var floatedLetter = _floatedletterCollection.AddOrDequeue();
            floatedLetter.Init(let.transform.localPosition,let.GetLetter());;
            let.ClearLetter();
            floatedLetter.gameObject.SetActive(true);
            let.gameObject.SetActive(false);
         
        }
        
        for (int i = 0; i < _floatedletterCollection.DisplayCount; i++)
        {
            int x = pos.isHorizontal ? pos.StartX + i : pos.StartX;
            int y = pos.isHorizontal ? pos.StartY : pos.StartY + i;

            var cell = CrosswordManager.Instance.GetCell(x, y);
            cell.SetShowing(true);
            var curFloatedLetter = _floatedletterCollection.DisplayList[i];
            curFloatedLetter.Pulsate(i,cell,() => _floatedletterCollection.ReturnToQueue(curFloatedLetter));;
            // floatedLetter.transform.DOScale(pulseScale, pulseDuration).SetEase(Ease.OutSine) // Smooth scaling up
            //     .SetDelay(index* delayBetween) // Add delay for staggered effect
            //     .OnComplete(() =>
            //     {
            //   //      floatedLetter.Init(floatedLetter.transform.localPosition, cell, () => _floatedletterCollection.ReturnToQueue(floatedLetter));
            //         
            //         floatedLetter.transform.DOScale(1f, pulseDuration).SetEase(Ease.InSine).OnComplete(() =>
            //         {
            //
            //             // var floatedLetter = _floatedletterCollection.AddOrDequeue();
            //             // floatedLetter.gameObject.SetActive(true);
            //        
            //         }); 
            //     }); 
        }
        
    }


    private void HandleLetterInput(Letter letter)
    {
        for (int i = 0; i < AllInputLetters.Length; i++)
        {
            if (AllInputLetters[i].isShowing)
            {
                continue;
            }

            AllInputLetters[i].ShowLetter(letter.letterChar);
            AllInputLetters[i].gameObject.SetActive(true);
            break;
        }

        CheckWord();
    }

    private void InitLetter(int index, char letter)
    {

        AllInputLetters[index].ShowLetter(letter);
        AllInputLetters[index].gameObject.SetActive(true);
    }


    private void InitLetter(int index)
    {
        AllInputLetters[index].HideLetter();
        AllInputLetters[index].gameObject.SetActive(true);
    }

    private void CheckWord()
    {

        if (UsedLetters.Count() == LetterWheel.answerWord.Length)
        {
            var arr = UsedLetters.Select(item => item.GetLetter()).ToArray();
            string result = new string(arr);
            if (result.Equals(LetterWheel.answerWord))
            {
                Pulsate();
                OnCorrectAnswer?.Invoke();
            }
            else
            {
                CrosswordManager.Instance.Reselect();
            }
        }
    }

    private void ClearInputtedLetters(List<CrosswordGridEntry> arg1, CrosswordEntryPositional posData)
    {
        foreach (var inpLetter in AllInputLetters)
        {
            inpLetter.ClearLetter();
            inpLetter.gameObject.SetActive(false);
        }

        for (int i = 0; i < arg1.Count; i++)
        {
            if (!arg1[i].GetShowing())
            {
                InitLetter(i);
            }
            else
            {
                InitLetter(i, arg1[i].letterAtCell);
            }
        }
    }


    void OnDisable()
    {
        CrosswordManager.Instance.OnNewWordClicked -= ClearInputtedLetters;
        OnLetterSelected -= HandleLetterInput;
    }
}