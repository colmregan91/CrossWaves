using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class Letter : ClickHandlers,  IQueuedObject
{
    private TextMeshProUGUI _letterText;
    public char letterChar;
    [SerializeField] private GameObject bg;

    private bool isSelected;
    private int _anchoredPositionIndex;
    private void Awake()
    {
        _letterText = GetComponentInChildren<TextMeshProUGUI>();
    }
    
    public void Init(object letter)
    { 
        isSelected = false;
            bg.SetActive(false);
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
        isSelected = false;
    }

    public  void SetSelected(bool val)
    {
        isSelected = val;
        bg.SetActive(val);
    }
    
    public override void OnPointerDown(PointerEventData eventData)
    {
        if (isSelected)
        {
            return;
        }
        SetSelected(true);
        LetterInputManager.Instance.OnLetterSelected?.Invoke(this);
    }

    public override void OnDrag(PointerEventData eventData)
    {
        
     //   LineManager.Instance.OnDraggedLetter?.Invoke(this);
        
    }
    
    public override void OnEndDrag(PointerEventData eventData)
    {

   //     LineManager.Instance.OnDraggedLetterEnd?.Invoke();

    }
    public override void OnPointerUp(PointerEventData eventData)
    {

    }
    


    
    

}

public class ClickHandlers :  MonoBehaviour,IPointerDownHandler, IDragHandler, IPointerUpHandler, IEndDragHandler
{
    public virtual void OnPointerDown(PointerEventData eventData)
    {

    }

    public virtual void OnDrag(PointerEventData eventData)
    {

    }
    public virtual void OnPointerUp(PointerEventData eventData)
    {

    }
    
    public virtual void OnEndDrag(PointerEventData eventData)
    {
        
    }
}
