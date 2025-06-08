using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HintManager : MonoSingleton<HintManager>
{ 
    private CanvasGroup hintCanvas;
    public float fadeDuration = 1.0f; // Duration of the fade effect

    private Task fadeTask;
    public TextMeshProUGUI coinAmountText;
    private int coinAmount;
    private void Start()
    {
        if (hintCanvas == null)
        {
            hintCanvas = GetComponent<CanvasGroup>();
        }
        
        coinAmount = 100;
        coinAmountText.text = coinAmount.ToString();
    }
    
    [SerializeField] private Button revealLetterButton;
    [SerializeField] private Button revealWorrdButton;

    private void OnEnable()
    {
        revealLetterButton.onClick.AddListener(HandleRevealLetterClicked);
        revealWorrdButton.onClick.AddListener(HandleRevealWordClicked);

    }
    
    private void HandleRevealWordClicked()
    {
       CrosswordManager.Instance.RevealWord();
       DeductCoins(5);
        CloseHintMenu();
    }

    private void DeductCoins(int amount)
    {
        coinAmount -= amount;
        coinAmountText.text = coinAmount.ToString();
    }

    private void HandleRevealLetterClicked()
    {
        CrosswordManager.Instance.RevealLetter();
        DeductCoins(2);
        CloseHintMenu();
    }

    private void OnDisable()
    {
        revealLetterButton.onClick.RemoveListener(HandleRevealLetterClicked);
        revealWorrdButton.onClick.RemoveListener(HandleRevealWordClicked);

    }

    public void OpenHintMenu()
    {
        FadeIn();
    }


    public void CloseHintMenu()
    {
        FadeOut();
    }

    public async void FadeIn()
    {
        if (fadeTask != null && !fadeTask.IsCompleted)
        {
            return; // Prevent overlapping fade tasks
        }

        fadeTask = FadeCanvasGroup(0, 1);
        await fadeTask;
        hintCanvas.blocksRaycasts = true;
        hintCanvas.interactable = true;
    }

    public async void FadeOut()
    {
        if (fadeTask != null && !fadeTask.IsCompleted)
        {
            return; // Prevent overlapping fade tasks
        }

        fadeTask = FadeCanvasGroup(1, 0);
        await fadeTask;

        hintCanvas.blocksRaycasts = false;
        hintCanvas.interactable = false;
    }

    private async Task FadeCanvasGroup(float startAlpha, float endAlpha)
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            hintCanvas.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / fadeDuration);
            await Task.Yield(); // Wait until the next frame
        }

        hintCanvas.alpha = endAlpha;
    }
}