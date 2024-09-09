using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UIStyleApplier))]
[CanEditMultipleObjects]
public class UIStyleApplierEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        UIStyleApplier styleApplier = (UIStyleApplier)target;

        if (GUILayout.Button("Apply Style"))
        {
            styleApplier.ApplyStyle();
        }
    }
}