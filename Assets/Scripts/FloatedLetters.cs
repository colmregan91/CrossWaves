using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class FloatedLetters : MonoBehaviour
{
    public float pulseDuration = 0.2f; // Duration of the pulsate effect
    public float pulseScale = 2f; // How much bigger the prefab grows
    public float delayBetween = 0.1f;
    
    [SerializeField]private TextMeshProUGUI _letterText;

    public void Init(Vector3 pos,char let)
    {
        transform.localPosition = pos;
        _letterText.text = let.ToString();

    }


    public void Pulsate(int index,CrosswordGridEntry entry, Action callback)
    {
        bool wasShowing = entry.isShowing;
        entry.SetShowing(true);
        transform.transform.DOScale(pulseScale, pulseDuration).SetEase(Ease.OutSine) // Smooth scaling up
            .SetDelay(index * delayBetween) // Add delay for staggered effect
            .OnComplete(() =>
            {

                // Scale back down
                transform.transform.DOScale(1f, pulseDuration).SetEase(Ease.InSine).OnComplete(() =>
                {

                    if (wasShowing)
                    {
                        callback?.Invoke();
                        return;
                    }
                    
                    Float(entry, callback);
                }

            ); // Smooth scaling down
            }); // Smooth scaling down
    }
    
    private void Float(CrosswordGridEntry entry, Action callback)
    {
        transform.DOMove(entry.transform.position, 1).SetEase(Ease.Linear).OnComplete(() =>
        {
            entry.ShowCell();
            callback?.Invoke();
            // reset 
        });;
        
    }

    private void OnDisable()
    {
        _letterText.text = string.Empty;
    }
}