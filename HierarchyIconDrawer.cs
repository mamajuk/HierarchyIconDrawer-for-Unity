using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static UnityEngine.GraphicsBuffer;

#if UNITY_EDITOR
using UnityEditor;

/**************************************************************************
 *     Hierarchy Window에 지정한 아이콘을 그려주는 static class...
 * *****/
[InitializeOnLoad]
public static class HierarchyIconDrawer
{
    //===================================================
    //////                Fields...                //////
    ///==================================================
    private const string _AssetName = "HierarchyIconDrawerData.asset";

    private static HierarchyIConDrawerAsset _asset;
    private static Dictionary<int, int>     _cacheList = new Dictionary<int, int>();



    //=======================================================
    ////////             Core methods...              ///////
    ///======================================================
    static HierarchyIconDrawer()
    {
        #region Omit
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
        int len       = _asset.IconList.Count;
        int showCount = 0; 

        if (_asset == null || _asset.ShowIcon == false || _cacheList==null || _cacheList.Count==0){
            return;
        }



        /*********************************************************************
         *    캐싱된 데이터가 존재한다면, 캐싱된 인덱스의 아이콘만 모두 표시한다....
         * ******/
        if (_cacheList.ContainsKey(instanceID) == false) return;

        for (int i=0; i<len; i++)
        {
            /***해당 컴포넌트를 사용하지 않는다면 스킵한다....**/
            if((_cacheList[instanceID] & (1<<i))==0){
                continue;
            }

            HierarchyIConDrawerAsset.IconData curr = _asset.IconList[i];

            Rect iconRect = new Rect(selectionRect.xMax - (20 * showCount++), selectionRect.y, 16, 16);
            GUI.DrawTexture(iconRect, curr.Icon);
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
         *     캐싱할 데이터를 저장할 에셋이 유효한가..?
         * *****/
        if(_asset==null){
            _asset = AssetDatabase.LoadAssetAtPath<HierarchyIConDrawerAsset>(GetAssetPath());
        }

        if(_asset==null || _asset.IconList==null || _cacheList==null || _asset.IconList.Count==0){
            return;
        }



        /***********************************************************************
         *     Hierarchy 창에 있는 모든 GameObject들의 정보를 적절히 캐싱한다...
         * *****/
        GameObject[] sceneObjs = GameObject.FindObjectsOfType<GameObject>();

        int objCount   = sceneObjs.Length;
        int compCount  = _asset.IconList.Count;
        int cacheValue = 0;


        _cacheList.Clear();
        for(int i=0; i<objCount; i++)
        {
            GameObject currObj = sceneObjs[i];
            for(int j=0; j<compCount; j++)
            {
                HierarchyIConDrawerAsset.IconData currComp = _asset.IconList[j];

                /*해당 컴포넌트가 존재하지 않다면 스킵한다.....*/
                if (currComp.Icon == null || currObj.GetComponent(currComp.ClassName) == null){
                    continue;
                }

                cacheValue |= (1<<j);
            }

            /**캐싱할 정보가 있다면 추가한다...*/
            if (cacheValue!=0){
                _cacheList.Add(currObj.GetInstanceID(), cacheValue);
            }

            cacheValue = 0;
        }

        #endregion
    }

}
#endif