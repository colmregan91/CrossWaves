using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestionManager : MonoBehaviour
{
    private TextMeshProUGUI _letterText;
    private void Awake()
    {
        _letterText = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Start()
    {
        CrosswordManager.Instance.OnNewWordClicked += ShowNewQuestion;
    }

    private void ShowNewQuestion(CrosswordEntryPositional obj)
    {
        _letterText.text = obj.entry.question;
    }
    
    
    private void OnDisable()
    {
        CrosswordManager.Instance.OnNewWordClicked -= ShowNewQuestion;
    }

    private void ShowNewQuestion(List<CrosswordGridEntry> arg1, CrosswordEntryPositional arg2)
    {
        ShowNewQuestion(arg2);
    }
}
