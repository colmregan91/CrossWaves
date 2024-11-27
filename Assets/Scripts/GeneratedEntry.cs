
using UnityEngine.EventSystems;

public class GeneratedEntry : CrosswordGridEntry
{
    public bool IsSelected => selImg.enabled == true;
    
    public override void SetCell(char text, CrosswordEntryPositional info)
    {
        img.enabled = true;
        entryInfo = info;
        HasLetter = true;
        letterAtCell = text;
        textField.text = letterAtCell.ToString();
    }
    
    public override void OnPointerDown(PointerEventData eventData)
    {
        if (HasLetter == false)
        {
            return;
        }



        selImg.enabled = !selImg.enabled;
    }

    public override void TurnOffGridElement()
    {
        HasLetter = false;
        img.enabled = false;
        letterAtCell = ' ';
        textField.text = string.Empty;

    }
}