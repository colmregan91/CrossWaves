using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputtedLetter : MonoBehaviour
{
    private TextMeshProUGUI _letterText;
    private Image bg;
    public bool isShowing => _letterText.enabled == true;
    private char CorrectLetter;
    private char Inputtedletter;
    private void Awake()
    {
        _letterText = GetComponentInChildren<TextMeshProUGUI>();
        bg = GetComponentInChildren<Image>();
    }

    public char GetLetter()
    {
        return CorrectLetter;

    }
    
    public char GetInputtedLetter()
    {
        return Inputtedletter;

    }

    public void InitInputtedLetter(char inputLetter, bool show)
    {
        CorrectLetter = inputLetter;

        if (show)
        {
            ShowCorrectLetter();
        }else
        {
            HideLetter();
        }
    }

    public void ToggleBg(bool val)
    {
        bg.enabled = val;
//        bg.tog = val ;

    }
    
    
    public void HideLetter()
    {
   
        ToggleBg(true);
        _letterText.text = string.Empty;

    }
    public void ShowCorrectLetter()
    {
        Inputtedletter = CorrectLetter;
        ToggleBg(true);
        _letterText.text = CorrectLetter.ToString();
        _letterText.enabled = true;
    }

    public void ShowLetter(char letter)
    {
        Inputtedletter = letter;
        ToggleBg(true);
        _letterText.text = letter.ToString();
        _letterText.enabled = true;
    }

    public void ClearLetter()
    {
        ToggleBg(true);
        _letterText.text = string.Empty;
        _letterText.enabled = false;
    }
    
}