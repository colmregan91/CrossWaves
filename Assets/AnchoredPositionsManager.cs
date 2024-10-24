using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class AnchoredPositionsManager : MonoBehaviour // make singleton
{
    private CollectionOrganiser<RectTransform> _anchoredPositionCollection;
    private RectTransform _startIndex;

    private void Awake()
    {
        _anchoredPositionCollection = new CollectionOrganiser<RectTransform>("AnchoredPosition", transform);
    }

    public RectTransform GetStartIndex()
    {
        return _startIndex;
    }

    public RectTransform GetPosAtIndex(int index)
    {
        return _anchoredPositionCollection.DisplayList[index];
    }
    
    public List<RectTransform> GetAnchoredList()
    {
        return _anchoredPositionCollection.DisplayList;
    }

    public void ResetAnchoredPositions()
    {
        _anchoredPositionCollection.ClearList();
    }

    public void ArrangeAnchoredPositions(int count, float radius)
    {
        float angleStep = 360f / count; // The angle between each object
        int curCount = _anchoredPositionCollection.DisplayCount;
        for (int i = 1; i <= count; i++)
        {
            float angle = i * angleStep; // Calculate the angle for each object
            float angleRad = angle * Mathf.Deg2Rad; // Convert angle to radians

            // Calculate the position of each object
            float x = Mathf.Cos(angleRad) * radius;
            float y = Mathf.Sin(angleRad) * radius;

            // Set the object's position
            var anchoredPosition =  _anchoredPositionCollection.AddOrDequeue();
            anchoredPosition.gameObject.SetActive(true);
            anchoredPosition.anchoredPosition = new Vector2(x, y);
            
        }

        _startIndex = _anchoredPositionCollection.DisplayList[curCount + count-1];
        _startIndex.gameObject.name = "Start";
    }
    
}

public class AnchoredDimensions
{
    public int startVal;
    public int endVal;
}