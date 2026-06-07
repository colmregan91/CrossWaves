using UnityEngine;
using UnityEngine.UI;

public class InputModeToggle : MonoBehaviour
{
    [SerializeField] private Image wheelIcon;
    [SerializeField] private Image keyboardIcon;
    [SerializeField] private CanvasGroup wheelCanvas;
    [SerializeField] private CanvasGroup keyboardCanvas;

    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(Toggle);
    }

    private void OnDestroy()
    {
        _button.onClick.RemoveListener(Toggle);
    }

    public void Toggle()
    {
        bool showKeyboard = !keyboardCanvas.interactable;
        wheelIcon.enabled = showKeyboard;
        keyboardIcon.enabled = !showKeyboard;
        SetCanvas(wheelCanvas, !showKeyboard);
        SetCanvas(keyboardCanvas, showKeyboard);
    }

    private void SetCanvas(CanvasGroup cg, bool active)
    {
        cg.alpha = active ? 1f : 0f;
        cg.interactable = active;
        cg.blocksRaycasts = active;
    }
}
