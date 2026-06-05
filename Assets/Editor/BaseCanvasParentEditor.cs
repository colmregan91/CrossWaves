using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BaseCanvasParent), true)]
public class BaseCanvasParentEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var canvas = (BaseCanvasParent)target;

        if (GUILayout.Button("Set Active"))
        {
            var cg = canvas.CanvasGroup;
            if (cg != null)
            {
                Undo.RecordObject(cg, "Set Canvas Active");
                cg.alpha = 1f;
                cg.interactable = true;
                cg.blocksRaycasts = true;
                EditorUtility.SetDirty(cg);
            }
        }
    }
}
