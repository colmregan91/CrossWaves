
using UnityEngine.EventSystems;

public class GeneratedEntry : CrosswordGridEntry
{
    
    public override void SetCell(char text, CrosswordEntryPositional info)
    {
        
        entryInfo = info;
        HasLetter = true;
        letterAtCell = text;
        textField.text = letterAtCell.ToString();
        CellHolder.SetActive(true);
    }
    
    


    public override void TurnOffGridElement()
    {
        CellHolder.SetActive(false);
        HasLetter = false;
        letterAtCell = ' ';
        textField.text = string.Empty;

    }
}