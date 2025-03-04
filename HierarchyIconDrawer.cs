using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;

#if UNITY_EDITOR
using UnityEditor;

/**************************************************************************
 *     Hierarchy Window에 지정한 아이콘을 그려주는 static class...
 * *****/
[InitializeOnLoad]
public static class HierarchyIconDrawer
{
    private sealed class CacheData
    {
        public int    StartIdx;
        public int    Count;
        public float  Offset;
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

    private static Dictionary<int, CacheData> _cacheMap  = new Dictionary<int, CacheData>();
    private static List<System.Type>          _typeCache = new List<System.Type>();
    private static List<int>                  _drawCache = new List<int>();



    //=======================================================
    ////////             Core methods...              ///////
    ///======================================================
    static HierarchyIconDrawer()
    {
        #region Omit
        if (EditorApplication.isPlayingOrWillChangePlaymode){
            return;
        }

        RefreshCacheData();

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
        if (_cacheMap.ContainsKey(instanceID) == false) return;

        CacheData data = _cacheMap[instanceID];

        int   showCount  = 0;
        int   goalIdx    = (data.StartIdx+data.Count);
        float iconX      = (selectionRect.xMax - data.Offset);
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
        RefreshCacheData();
    }

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

    public static void RefreshCacheData()
    {
        #region Omit
        /****************************************************
         *     캐싱을 해야하는지를 확인한다.....
         * *****/
        _drawCache.Clear();
        _cacheMap.Clear();
        _typeCache.Clear();

        if (_asset==null){
            _asset = AssetDatabase.LoadAssetAtPath<HierarchyIConDrawerAsset>(GetAssetPath());
        }

        if (_asset==null || _asset.IconList==null || _asset.IconList.Count==0){
            return;
        }



        /***************************************************************************************
         *     Hierarchy 창에 있는 모든 GameObject들의 정보를 가져온 후, 캐싱 데이터를 초기화한다...
         * *****/
        GameObject[] sceneObjs = GameObject.FindObjectsOfType<GameObject>();

        int objCount   = sceneObjs.Length;
        int compCount  = _asset.IconList.Count;
        int startIdx   = 0;
        int drawCount  = 0;
        _nameMaxWidth = 0f;



        /******************************************************
         *     컴포넌트들의 타입들을 캐싱한다....
         * *****/
        List<HierarchyIConDrawerAsset.IconData> compList = _asset.IconList;

        for (int i = 0; i < compCount; i++){
            _typeCache.Add(System.Type.GetType(compList[i].ClassName));
        }



        /***********************************************************************
         *     Hierarchy 창에서 그려질 가능성이 있는 것들만 캐싱한다....
         * *****/
        for (int i=0; i<objCount; i++){
            GameObject currObj = sceneObjs[i];

            /**어떤 컴포넌트를 가지고 있는지 확인한다...**/
            for(int j=0; j<compCount; j++)
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
                bool isPrefab = (PrefabUtility.GetPrefabAssetType(currObj) != PrefabAssetType.NotAPrefab);
                _cacheMap.Add(currObj.GetInstanceID(), new CacheData() { StartIdx=startIdx, Count=drawCount, Name=currObj.name, Offset=(isPrefab?20f:0f) });
            }

            startIdx += drawCount;
            drawCount = 0;
        }
        #endregion
    }

}
#endif