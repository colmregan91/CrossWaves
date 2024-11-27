using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineManager : MonoSingleton<LineManager>
{


    public Action<Letter> OnDraggedLetter;
    public Action OnDraggedLetterEnd;
    
    public LineRenderer lineRenderer; // Reference to the LineRenderer component
    private List<Letter> selectedLetters = new List<Letter>(); // Store selected letters
    private bool isSelecting = false;
    private bool isDragging= false;
    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
    }

    void OnEnable()
    {
 
        OnDraggedLetter += HandleLetterDragged;
        OnDraggedLetterEnd += HandleDragEnded;
    }

    private void Start()
    {
        CrosswordManager.Instance.OnNewWordClicked += HandleNewWordClicked;
    }

    private void HandleNewWordClicked(List<CrosswordGridEntry> arg1, CrosswordEntryPositional arg2)
    {
        HandleNewWordClicked();
    }
    

    private void HandleNewWordClicked()
    {
        foreach (var letter in selectedLetters)
        {
            letter.SetSelected(false);
        }
        selectedLetters.Clear();
    }

    private void HandleLetterAdded(Letter letter)
    {   
        AddLetter(letter);
    }
    
    private void HandleDragEnded()
    {
        if (isDragging == false)
        {
            return;
        }
        isDragging = false;
        lineRenderer.positionCount = selectedLetters.Count - 1;
    }
    
    private void HandleLetterDragged(Letter letter)
    {
        if (IsLastAddedLetter(letter) == false)
        {
            Debug.Log("not last added");
            return;
        }

        isSelecting = false;
        isDragging = true;
        lineRenderer.positionCount = selectedLetters.Count + 1;
     

    }

    private void Update()
    {
        if (!isDragging)
        {
            return;
        }
        var screenPoint = Input.mousePosition;
        screenPoint.z = 100.0f; //distance of the plane from the camera
       var f = Camera.main.ScreenToWorldPoint(screenPoint);
       
        lineRenderer.SetPosition(selectedLetters.Count,f);
    }

    private void AddLetter(Letter letter)
    {
        if (letter != null && !selectedLetters.Contains(letter))
        {
            selectedLetters.Add(letter);
    
            // Update line points
            lineRenderer.positionCount = selectedLetters.Count;
            for (int i = 0; i < selectedLetters.Count; i++)
            {
                lineRenderer.SetPosition(i, selectedLetters[i].transform.position);
            }
        }

    }

    public bool IsLastAddedLetter(Letter letter)
    {
        return letter == selectedLetters[^1];
    }
    

    void OnDisable()
    {
        CrosswordManager.Instance.OnNewWordClicked -= HandleNewWordClicked;
        OnDraggedLetter -= HandleLetterDragged;
        OnDraggedLetterEnd -= HandleDragEnded;
    }
}
