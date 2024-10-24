using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Letter : MonoBehaviour, IQueuedObject
{
    private TextMeshProUGUI _letterText;
    public char letterChar;
    
    private int _anchoredPositionIndex;
    private void Awake()
    {
        _letterText = GetComponentInChildren<TextMeshProUGUI>();
    }
    
    public void Init(object letter)
    {
        
            transform.position = Vector3.zero;
            char uppercaseChar = char.ToUpper((char)letter);
            ShowLetter(uppercaseChar);
        
    }
    
    public void SetAnchoredPositionIndex(int index)
    {
        _anchoredPositionIndex = index;
    }


    public int GetAnchoredPositionIndex()
    {
        return _anchoredPositionIndex;
    }
    
    public void ShowLetter(char letter)
    {
        letterChar = letter;
        _letterText.text = letter.ToString();
    }

    public void Deinit()
    {
        SetAnchoredPositionIndex(0);
        
        gameObject.SetActive(false);
    }
}
