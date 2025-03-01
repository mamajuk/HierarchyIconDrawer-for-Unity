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
        public string    ClassName;   
        public Texture2D Icon;
    }

    public bool           ShowIcon = true;
    public List<IconData> IconList = new List<IconData>();
}
#endif