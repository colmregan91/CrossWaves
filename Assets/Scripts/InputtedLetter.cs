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
    private char letter;
    private void Awake()
    {
        _letterText = GetComponentInChildren<TextMeshProUGUI>();
        bg = GetComponentInChildren<Image>();
    }

    public char GetLetter()
    {
        return letter;

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

    public void ShowLetter(char inputLetter)
    {
        ToggleBg(true);
        letter = inputLetter;
        _letterText.text = inputLetter.ToString();
  
        _letterText.enabled = true;
    }

    public void ClearLetter()
    {
        ToggleBg(true);
        _letterText.text = string.Empty;
        _letterText.enabled = false;
    }
    
}