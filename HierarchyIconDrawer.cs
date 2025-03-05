using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;

/**************************************************************************
 *     Hierarchy Window�� ������ �������� �׷��ִ� static class...
 * *****/
[InitializeOnLoad]
public static class HierarchyIconDrawer
{
    private struct CacheData
    {
        public int    StartIdx;
        public int    Count;
        public float  PrefabOffset;
        public string Name;
    }


    //===================================================
    //////                Fields...                //////
    ///==================================================
    private const float  _IconSize  = 20f;
    private const string _AssetName = "HierarchyIconDrawerData.asset";

    private static float                      _nameMaxWidth  = 0f;
    private static GUIContent                 _sharedContent = new GUIContent();
    private static HierarchyIConDrawerAsset   _asset;

    private static List<int>            _drawCache    = new List<int>();
    private static List<System.Type>    _typeCache    = new List<System.Type>();
    private static Dictionary<int, int> _objCacheMap  = new Dictionary<int, int>();
    private static CacheData[]          _objCache     = new CacheData[10];



    //=======================================================
    ////////             Core methods...              ///////
    ///======================================================
    static HierarchyIconDrawer()
    {
        #region Omit
        if (EditorApplication.isPlayingOrWillChangePlaymode){
            return;
        }

        RefereshTypeCache();
        RefreshDrawCache();

        /**Hierarchy â�� �������� �׸��� �븮�ڸ� ����Ѵ�....**/
        EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
        EditorApplication.hierarchyChanged         += OnHierarchyChanged;
        #endregion
    }

    private static void OnHierarchyGUI(int instanceID, Rect selectionRect)
    {
        #region Omit
        /*******************************************************
         *    �ش�Ǵ� �����ܵ��� ��� ǥ���Ѵ�....
         * ******/
        if (_asset == null || _asset.ShowIcon == false){
            return;
        }


        /*********************************************************************
         *    ĳ�̵� �����Ͱ� �����Ѵٸ�, ĳ�̵� �ε����� �����ܸ� ��� ǥ���Ѵ�....
         * ******/
        if (_objCacheMap.ContainsKey(instanceID) == false) return;

        ref CacheData data = ref _objCache[_objCacheMap[instanceID]];

        int   showCount  = 0;
        int   goalIdx    = (data.StartIdx+data.Count);
        float iconX      = (selectionRect.xMax - data.PrefabOffset);
        float moveOffset = -20f; 

        /**������ ���Ĺ���� �����Ѵ�....**/
        switch(_asset.Aligment)
        {
            case (HierarchyIConDrawerAsset.AligmentType.Middle):{
               _sharedContent.text = data.Name;

               float nameWidth = (EditorStyles.label.CalcSize(_sharedContent).x + selectionRect.x + _IconSize);
               iconX      = (nameWidth < _nameMaxWidth? _nameMaxWidth:(_nameMaxWidth=nameWidth));
               moveOffset = 20f;
               break;
            }

            case (HierarchyIConDrawerAsset.AligmentType.Left):{
               _sharedContent.text = data.Name;
               iconX = (EditorStyles.label.CalcSize(_sharedContent).x + selectionRect.x + _IconSize);
               moveOffset = 20f;
               break;
            }
        }



        /**��� �����ܵ��� ���ʴ�� ǥ���Ѵ�....**/
        for(int i=data.StartIdx; i<goalIdx; i++)
        {
            HierarchyIConDrawerAsset.IconData element = _asset.IconList[_drawCache[i]];

            if (element.Icon!=null){
                Rect iconRect = new Rect(iconX + (moveOffset * showCount++), selectionRect.y, 16, 16);
                GUI.DrawTexture(iconRect, element.Icon);
            }
        }
        #endregion
    }

    private static void OnHierarchyChanged()
    {
        RefreshDrawCache();
    }

    public static void RefreshDrawCache()
    {
        #region Omit
        /****************************************************
         *     ĳ���� �ؾ��ϴ����� Ȯ���Ѵ�.....
         * *****/
        _drawCache.Clear();
        _objCacheMap.Clear();

        if (_asset==null){
            _asset = AssetDatabase.LoadAssetAtPath<HierarchyIConDrawerAsset>(GetAssetPath());
        }

        if (_asset==null || _asset.IconList==null){
            return;
        }

        int iconCount  = _asset.IconList.Count;
        if (iconCount!=_typeCache.Count){
            return;
        }


        /***********************************************************************
         *     Hierarchy â���� �׷��� ���ɼ��� �ִ� �͵鸸 ĳ���Ѵ�....
         * *****/
        GameObject[] sceneObjs = GameObject.FindObjectsOfType<GameObject>();

        int objCount   = sceneObjs.Length;
        int startIdx   = 0;
        int drawCount  = 0;
        int cacheIdx   = 0;
        _nameMaxWidth = 0f;

        for (int i=0; i<objCount; i++){

            GameObject currObj = sceneObjs[i];

            /**� ������Ʈ�� ������ �ִ��� Ȯ���Ѵ�...**/
            for(int j=0; j<iconCount; j++)
            {
                HierarchyIConDrawerAsset.IconData currComp = _asset.IconList[j];
                if (currComp.Icon == null || _typeCache[j]==null || currObj.GetComponent(_typeCache[j]) == null){
                    continue;
                }

                drawCount++;
                _drawCache.Add(j);
            }

            /**ĳ���� ������ �ִٸ� �߰��Ѵ�...***/
            if (drawCount>0){
                int  instanceID = currObj.GetInstanceID();
                int  cacheLen   = _objCache.Length;
                bool isPrefab   = (PrefabUtility.GetPrefabAssetType(currObj) != PrefabAssetType.NotAPrefab);

                //������ �����ϸ� ��� �Ҵ��Ѵ�....
                if(_objCache.Length <= cacheIdx){
                    CacheData[] oldCache = _objCache;
                    _objCache = new CacheData[cacheLen*2];
                    oldCache.CopyTo(_objCache, 0);
                }

                //ĳ�� �����͸� �����Ѵ�....
                _objCache[cacheIdx] = new CacheData { 
                    StartIdx     = startIdx, 
                    Count        = drawCount, 
                    Name         = currObj.name, 
                    PrefabOffset = (isPrefab ? 20f : 0f) 
                };

                _objCacheMap.Add(instanceID, cacheIdx++);
            }

            startIdx += drawCount;
            drawCount = 0;
        }
        #endregion
    }

    public static void RefereshTypeCache()
    {
        #region Omit

        if (_asset == null){
            _asset = AssetDatabase.LoadAssetAtPath<HierarchyIConDrawerAsset>(GetAssetPath());
        }

        if (_asset == null || _asset.IconList == null || _asset.IconList.Count == 0){
            return;
        }

        _typeCache.Clear();

        /******************************************************
         *     ������Ʈ���� Ÿ�Ե��� ĳ���Ѵ�....
         * *****/
        List<HierarchyIConDrawerAsset.IconData> compList = _asset.IconList;

        int len = compList.Count;
        for (int i = 0; i < len; i++){

            _typeCache.Add(System.Type.GetType(compList[i].ClassName));
        }
        #endregion
    }



    //========================================================
    //////////          Utility methods..             ////////
    //========================================================
    private static string GetAssetPath()
    {
        #region Omit
        string lawPath = AssetDatabase.FindAssets("t:MonoScript")
            .Select(AssetDatabase.GUIDToAssetPath)
            .FirstOrDefault(p =>
            {
                MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(p);
                return script != null && script.GetClass() != null && script.GetClass().Name == "HierarchyIconDrawer";
            });

        string[] pathSplit = lawPath.Split('/');
        return lawPath.Replace(pathSplit[pathSplit.Length - 1], "") + _AssetName;
        #endregion
    }
}
#endif