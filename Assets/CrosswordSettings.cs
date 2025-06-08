using UnityEngine;

public class CrosswordSettings
{
    public CrosswordsDifficulty Difficulty;
    public Color Color;
    public selectCrosswordItem[] Items;

    public CrosswordSettings(CrosswordsDifficulty difficulty, Color color, selectCrosswordItem[] items)
    {
        Difficulty = difficulty;
        Color = color;
        Items = items;
    }
}