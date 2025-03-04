using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
[System.Serializable]
public class HierarchyIConDrawerAsset : ScriptableObject
{
    [System.Serializable]
    public class IconData
    {
        public string    ClassName   = "";
        public string    DisplayName = "";
        public Texture2D Icon;
    }

    public enum AligmentType
    {
        Right, Left, Middle
    }

    public bool           ShowIcon  = true;
    public AligmentType   Aligment  = AligmentType.Right;
    public List<IconData> IconList  = new List<IconData>();
}
#endif