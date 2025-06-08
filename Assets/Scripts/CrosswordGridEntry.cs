using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CrosswordGridEntry : ClickHandlers
{
    [SerializeField] protected Image completeImg;
    [SerializeField] protected Image selImg;
    [SerializeField] protected Image img;
    public CrosswordEntryPositional entryInfo;
    [SerializeField] protected Image individuallySelectedImage;

    public TextMeshProUGUI textField;

    public char letterAtCell;

    public bool HasLetter;

    public bool isShowing;
    public bool IsSelected => selImg.enabled == true;


    public override void OnPointerDown(PointerEventData eventData)
    {
        if (HasLetter == false)
        {
            return;
        }

        CrosswordManager.Instance.UpdateCurSelected(this);

        if (selImg.enabled)
        {
            return;
        }

        CrosswordManager.Instance.SelectClickedtWord(entryInfo, this);
    }

    public bool GetShowing()
    {
        return isShowing;
    }

    public void SetShowing(bool val)
    {
        isShowing = val;
    }

    public void ShowCell()
    {
        completeImg.enabled = true;
        img.enabled = false;
        textField.color = Color.white;
        textField.text = letterAtCell.ToString();
        SetShowing(true);
    }

    public virtual void SetCell(char text, CrosswordEntryPositional info)
    {
        entryInfo = info;
        HasLetter = true;
        letterAtCell = text;
    }

    public void Reset()
    {
        HasLetter = false;
        letterAtCell = ' ';
        textField.text = String.Empty;
        img.enabled = true;
        completeImg.enabled = false;
        individuallySelectedImage.enabled = false;
        selImg.enabled = false;
        SetShowing(false);
        
    }

    public void Select()
    {
        selImg.enabled = true;
    }

    public void SelectIndividual()
    {
        individuallySelectedImage.enabled = true;
    }

    public void UnSelectIndividual()
    {
        individuallySelectedImage.enabled = false;
    }

    public void Unselect()
    {
        selImg.enabled = false;
        individuallySelectedImage.enabled = false;
    }

    public char GetCell()
    {
        return letterAtCell;
    }

    public virtual void TurnOffGridElement()
    {
        HasLetter = false;
        img.enabled = false;
        letterAtCell = ' ';
        textField.text = string.Empty;
    }
}