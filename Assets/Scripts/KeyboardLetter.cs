using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class KeyboardLetter : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private TextMeshProUGUI letterText;
    public char letterChar;

    public void Init(char c)
    {
        letterChar = char.ToUpper(c);
        letterText.text = letterChar.ToString();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        LetterInputManager.Instance.OnLetterSelected?.Invoke(letterChar);
    }
}
