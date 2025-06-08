using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;


public class LetterInputManager : MonoSingleton<LetterInputManager>
{
    // Start is called before the first frame update

    public Action OnCorrectAnswer;
    private InputtedLetter[] AllInputLetters;
    public Action<Letter> OnLetterSelected;

    private InputtedLetter[] UsedLetters => AllInputLetters.Where(t => t.gameObject.activeSelf && t.isShowing).ToArray();

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
        CrosswordManager.Instance.OnLetterRevealed += HandleLetterRevealed;
        CrosswordManager.Instance.OnWordRevealed += HandleWordRevealed;

        OnLetterSelected += HandleLetterInput;
    }

    private void HandleWordRevealed(List<CrosswordGridEntry> obj)
    {
        for (int i = 0; i < AllInputLetters.Length; i++)
        {
       

            AllInputLetters[i].ShowCorrectLetter();
            
        }
        
        CheckWord();
    }

    private void HandleLetterRevealed(int index, CrosswordGridEntry entry)
    {
        AllInputLetters[index].ShowCorrectLetter();
        CheckWord();
        var floatedLetter = _floatedletterCollection.AddOrDequeue();
        floatedLetter.Init(AllInputLetters[index].transform.localPosition, AllInputLetters[index].GetLetter());
        floatedLetter.gameObject.SetActive(true);

        floatedLetter.Pulsate(0, entry, () => { _floatedletterCollection.ReturnToQueue(floatedLetter); });
        ;
        ;
    }

    public void ClearLetters()
    {
        _floatedletterCollection.ClearList();
    }


    private void RevealCorrectAnswer()
    {
        var pos = CrosswordManager.Instance.getWordPosition();
        List<FloatedLetters> lettersList = new List<FloatedLetters>();
        foreach (var let in UsedLetters)
        {
            var floatedLetter = _floatedletterCollection.AddOrDequeue();
            floatedLetter.Init(let.transform.localPosition, let.GetLetter());
            ;
            let.ClearLetter();
            floatedLetter.gameObject.SetActive(true);
            let.gameObject.SetActive(false);
            lettersList.Add(floatedLetter);
        }

        for (int i = 0; i < lettersList.Count; i++)
        {
            int x = pos.isHorizontal ? pos.StartX + i : pos.StartX;
            int y = pos.isHorizontal ? pos.StartY : pos.StartY + i;

            var cell = CrosswordManager.Instance.GetCell(x, y);

            var curFloatedLetter = lettersList[i];
            curFloatedLetter.Pulsate(i, cell, () => _floatedletterCollection.ReturnToQueue(curFloatedLetter));
            ;
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
            break;
        }

        CheckWord();
    }


    private void CheckWord()
    {
        if (UsedLetters.Count() == LetterWheel.answerWord.Length)
        {
            var arr = UsedLetters.Select(item => item.GetInputtedLetter()).ToArray();
            string result = new string(arr);
            if (result.Equals(LetterWheel.answerWord))
            {
                RevealCorrectAnswer();
                OnCorrectAnswer?.Invoke();
            }
            else
            {
                CrosswordManager.Instance.Reselect();
            }
        }
    }

    private void ClearInputtedLetters(List<CrosswordGridEntry> entryPositions, CrosswordEntryPositional posData) // look int remivng posData
    {
        foreach (var inpLetter in AllInputLetters)
        {
            inpLetter.ClearLetter();
            inpLetter.gameObject.SetActive(false);
        }

        for (int i = 0; i < entryPositions.Count; i++)
        {
            var cur = entryPositions[i];
            AllInputLetters[i].InitInputtedLetter(cur.letterAtCell, cur.isShowing);
            AllInputLetters[i].gameObject.SetActive(true);
        }
    }


    void OnDisable()
    {
        CrosswordManager.Instance.OnNewWordClicked -= ClearInputtedLetters;
        OnLetterSelected -= HandleLetterInput;
        CrosswordManager.Instance.OnLetterRevealed -= HandleLetterRevealed;
        CrosswordManager.Instance.OnWordRevealed -= HandleWordRevealed;
    }
}