using UnityEngine;
using UnityEngine.UI;

public class KeyboardManager : MonoBehaviour
{
    [SerializeField] private Button backspaceButton;
    [SerializeField] private CanvasGroup _canvasGroup;


    private void OnEnable()
    {
        backspaceButton.onClick.AddListener(() => LetterInputManager.Instance.Backspace());
    }
    private void OnDisable()
    {
        backspaceButton.onClick.RemoveListener(LetterInputManager.Instance.Backspace);
    }
}
