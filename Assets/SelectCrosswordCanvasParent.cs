
using System.Collections.Generic;
using System.Linq;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;

public class SelectCrosswordCanvasParent : BaseCanvasParent
{

    public static Dictionary<CanvasGroup, CrosswordSettings> CanvasGroups = new Dictionary<CanvasGroup, CrosswordSettings>();
    
    [SerializeField]private CanvasGroup _easyGroup;
    [SerializeField]private CanvasGroup _intermediateGroup;
    [SerializeField] private CanvasGroup _hardGroup;
    [SerializeField]private CanvasGroup _veryHardGroup;
    [SerializeField] private CanvasGroup _themedGroup;
    [SerializeField]private CanvasGroup _letterGroup;

    public override void Awake()
    {
        var easyCrosswords = _easyGroup.GetComponentsInChildren<selectCrosswordItem>();
        var intermediateCrosswords = _intermediateGroup.GetComponentsInChildren<selectCrosswordItem>();
        var hardCrosswords = _hardGroup.GetComponentsInChildren<selectCrosswordItem>();
        var vHardCrosswords = _veryHardGroup.GetComponentsInChildren<selectCrosswordItem>();
        var themedCrosswords = _themedGroup.GetComponentsInChildren<selectCrosswordItem>();
        var letteredCrosswords = _letterGroup.GetComponentsInChildren<selectCrosswordItem>();
        
        CrosswordSettings easy = new CrosswordSettings(CrosswordsDifficulty.Easy, Color.green,easyCrosswords);
        CrosswordSettings medium = new CrosswordSettings(CrosswordsDifficulty.Intermediate, Color.blue,intermediateCrosswords);
        CrosswordSettings hard = new CrosswordSettings(CrosswordsDifficulty.Hard, Color.magenta,hardCrosswords);
        CrosswordSettings veryHard = new CrosswordSettings(CrosswordsDifficulty.VeryHard, Color.red,vHardCrosswords);
        CrosswordSettings themed = new CrosswordSettings(CrosswordsDifficulty.Themed, Color.yellow,themedCrosswords);
        CrosswordSettings letter = new CrosswordSettings(CrosswordsDifficulty.Letter, Color.cyan,letteredCrosswords);

        CanvasGroups.Add(_easyGroup, easy);
        CanvasGroups.Add(_intermediateGroup, medium);
        CanvasGroups.Add(_hardGroup, hard);
        CanvasGroups.Add(_veryHardGroup, veryHard);
        CanvasGroups.Add(_themedGroup, themed);
        CanvasGroups.Add(_letterGroup, letter);
        
        AddBackButtonOverride(() => CanvasManager.Instance.GoToCanvas<SelectDifficultyCanvasParent>());
    }

    
    public override void Start()// todo: rtime init on load
    {
        SetUpCanvas(_easyGroup);
        SetUpCanvas(_intermediateGroup);
        SetUpCanvas(_hardGroup);
        SetUpCanvas(_veryHardGroup);
        SetUpCanvas(_themedGroup);
        SetUpCanvas(_letterGroup);
    }

    private void SetUpCanvas(CanvasGroup group)
    {
        var itemGroup = CanvasGroups[group].Items;
        for (var index = 0; index < itemGroup.Length; index++)
        {
            var cw = itemGroup[index];
            if (CanvasGroups[group].Difficulty == CrosswordsDifficulty.Easy)
            {
                cw.Init(CanvasGroups[group].Color, index+1, index > 2);
            }
            else
            {
                cw.Init(CanvasGroups[group].Color, index+1, true);
            }
   
        }
    }

    private void UpdateCanvas(CanvasGroup group)
    {
        var itemGroup = CanvasGroups[group].Items;
        for (var index = 0; index < itemGroup.Length; index++)
        {
            var cw = itemGroup[index];

            if (cw.ShouldUpdate)
            {
                cw.SetStructure();
            }
   
        }
    }
    
    

    public override void OnBeforeActive()
    {
        var chosenDifficulty = CanvasManager.Instance.GetChosenDifficulty();
        var settings = CanvasGroups.Values.First(T => T.Difficulty == chosenDifficulty);

        foreach (var gr in CanvasGroups)
        {
            if (gr.Value == settings)
            {
                gr.Key.alpha = 1;
                gr.Key.interactable = true;
                gr.Key.blocksRaycasts = true;
                
                UpdateCanvas(gr.Key);
            }
            else
            {
                gr.Key.alpha = 0;
                gr.Key.interactable = false;
                gr.Key.blocksRaycasts = false;
            }
     
        }

    }
}

public enum CrosswordsDifficulty
{
    Easy,
    Intermediate,
    Hard,
    VeryHard,
    Letter,
    Themed
}