using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;

/**************************************************************************
 *     Hierarchy Window에 지정한 아이콘을 그려주는 static class...
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

        /**Hierarchy 창에 아이콘을 그리는 대리자를 등록한다....**/
        EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
        EditorApplication.hierarchyChanged         += OnHierarchyChanged;
        #endregion
    }

    private static void OnHierarchyGUI(int instanceID, Rect selectionRect)
    {
        #region Omit
        /*******************************************************
         *    해당되는 아이콘들을 모두 표시한다....
         * ******/
        if (_asset == null || _asset.ShowIcon == false){
            return;
        }


        /*********************************************************************
         *    캐싱된 데이터가 존재한다면, 캐싱된 인덱스의 아이콘만 모두 표시한다....
         * ******/
        if (_objCacheMap.ContainsKey(instanceID) == false) return;

        ref CacheData data = ref _objCache[_objCacheMap[instanceID]];

        int   showCount  = 0;
        int   goalIdx    = (data.StartIdx+data.Count);
        float iconX      = (selectionRect.xMax - data.PrefabOffset);
        float moveOffset = -20f; 

        /**나머지 정렬방식을 적용한다....**/
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



        /**모든 아이콘들을 차례대로 표시한다....**/
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
         *     캐싱을 해야하는지를 확인한다.....
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
         *     Hierarchy 창에서 그려질 가능성이 있는 것들만 캐싱한다....
         * *****/
        GameObject[] sceneObjs = GameObject.FindObjectsOfType<GameObject>();

        int objCount   = sceneObjs.Length;
        int startIdx   = 0;
        int drawCount  = 0;
        int cacheIdx   = 0;
        _nameMaxWidth = 0f;

        for (int i=0; i<objCount; i++){

            GameObject currObj = sceneObjs[i];

            /**어떤 컴포넌트를 가지고 있는지 확인한다...**/
            for(int j=0; j<iconCount; j++)
            {
                HierarchyIConDrawerAsset.IconData currComp = _asset.IconList[j];
                if (currComp.Icon == null || _typeCache[j]==null || currObj.GetComponent(_typeCache[j]) == null){
                    continue;
                }

                drawCount++;
                _drawCache.Add(j);
            }

            /**캐싱할 정보가 있다면 추가한다...***/
            if (drawCount>0){
                int  instanceID = currObj.GetInstanceID();
                int  cacheLen   = _objCache.Length;
                bool isPrefab   = (PrefabUtility.GetPrefabAssetType(currObj) != PrefabAssetType.NotAPrefab);

                //공간이 부족하면 배로 할당한다....
                if(_objCache.Length <= cacheIdx){
                    CacheData[] oldCache = _objCache;
                    _objCache = new CacheData[cacheLen*2];
                    oldCache.CopyTo(_objCache, 0);
                }

                //캐시 데이터를 삽입한다....
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
         *     컴포넌트들의 타입들을 캐싱한다....
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