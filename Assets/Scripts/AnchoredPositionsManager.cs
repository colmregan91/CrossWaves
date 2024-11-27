using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class AnchoredPositionsManager : MonoBehaviour // make singleton
{
    private CollectionOrganiser<RectTransform> _anchoredPositionCollection;
    private RectTransform _startIndex;
    public float horizontalRadius = 100;
    public float verticalRadius = 100;
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

    public void ArrangeAnchoredPositions(int count)
    {
        float angleStep = 360f / count; // The angle between each object
        int curCount = _anchoredPositionCollection.DisplayCount;
        for (int i = 1; i <= count; i++)
        {
            float angle = i * angleStep; // Calculate the angle for each object
            float angleRad = angle * Mathf.Deg2Rad; // Convert angle to radians

            // Calculate the position of each object, using different radii for x and y to form an oval
            float x = Mathf.Cos(angleRad) * horizontalRadius;
            float y = Mathf.Sin(angleRad) * verticalRadius;

            // Set the object's position
            var anchoredObj =  _anchoredPositionCollection.AddOrDequeue();
            anchoredObj.gameObject.SetActive(true);
            anchoredObj.anchoredPosition = new Vector2(x, y);
        }

        _startIndex = _anchoredPositionCollection.DisplayList[curCount + count-1];
    }
    
}