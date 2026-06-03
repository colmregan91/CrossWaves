using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CrosswordGridEntry : ClickHandlers
{
    public CrosswordEntryPositional entryInfo;
    [SerializeField] protected GameObject CellHolder;
    public Image mainImg;

    public TextMeshProUGUI textField;

    public char letterAtCell;

    public bool HasLetter;

    public bool isShowing;

    private Color defaultColor;

    private void Awake()
    {
        if (mainImg != null)
            defaultColor = mainImg.color;
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (HasLetter == false)
            return;

        CrosswordManager.Instance.UpdateCurSelected(this);
        CrosswordManager.Instance.SelectClickedtWord(entryInfo, this);
    }

    public bool GetShowing() => isShowing;

    public void SetShowing(bool val) => isShowing = val;

    public void ShowCell()
    {
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
        CellHolder.SetActive(true);
        SetShowing(false);
        mainImg.DOKill();
        mainImg.color = defaultColor;
    }

    public void Select(float delay = 0f)
    {
        mainImg.DOKill();
        mainImg.DOColor(CrosswordManager.Instance.wordSelectColor, 0.3f).SetDelay(delay);
    }

    public void SelectIndividual()
    {
        mainImg.DOKill();
        mainImg.DOColor(CrosswordManager.Instance.individualCellColor, 0.3f);
    }

    public void UnSelectIndividual()
    {
        mainImg.DOKill();
        mainImg.DOColor(CrosswordManager.Instance.wordSelectColor, 0.3f);
    }

    public void Unselect(float delay = 0f)
    {
        mainImg.DOKill();
        mainImg.DOColor(defaultColor, 1f).SetDelay(delay);
    }

    public char GetCell() => letterAtCell;

    public virtual void TurnOffGridElement()
    {
        CellHolder.SetActive(false);
        HasLetter = false;
        letterAtCell = ' ';
        textField.text = string.Empty;
    }
}