using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(HierarchyIConDrawerAsset))]
public class HierarchyIconDrawerAssetEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayout.HelpBox("Edit in \"Utility > HierarchyIconDrawer Settings.\"", MessageType.Info);
    }
}

#endif