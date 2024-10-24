using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnchoredPosition : MonoBehaviour
{
    private RectTransform rectTransform;
    public bool isTaken;
    
    public Vector2 GetAnchoredPosition()
    {
        return rectTransform.position;
    }
    
    public void SetAnchoredPosition(float x, float y)
    {
        rectTransform.anchoredPosition = new Vector2(x, y);

    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void OnDisable()
    {
        isTaken = false;
    }
}
