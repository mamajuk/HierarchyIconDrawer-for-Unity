using System.Collections;
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
    private sealed class CacheData
    {
        public int StartIdx;
        public int Count; 
    }


    //===================================================
    //////                Fields...                //////
    ///==================================================
    private const string _AssetName = "HierarchyIconDrawerData.asset";

    private static HierarchyIConDrawerAsset   _asset;
    private static Dictionary<int, CacheData> _cacheMap  = new Dictionary<int, CacheData>();
    private static List<int>                  _cacheList = new List<int>();



    //=======================================================
    ////////             Core methods...              ///////
    ///======================================================
    static HierarchyIconDrawer()
    {
        #region Omit
        RefreshCacheData();

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
        if (_asset == null || _asset.ShowIcon == false || _cacheMap==null || _cacheMap.Count==0){
            return;
        }


        /*********************************************************************
         *    ĳ�̵� �����Ͱ� �����Ѵٸ�, ĳ�̵� �ε����� �����ܸ� ��� ǥ���Ѵ�....
         * ******/
        if (_cacheMap.ContainsKey(instanceID) == false) return;

        CacheData data       = _cacheMap[instanceID];
        int       showCount  = 0;
        int       goalIdx    = (data.StartIdx+data.Count);

        for(int i=data.StartIdx; i<goalIdx; i++){

            Rect iconRect = new Rect( selectionRect.xMax - (20 * showCount++), selectionRect.y, 16, 16);
            GUI.DrawTexture(iconRect, _asset.IconList[_cacheList[i]].Icon);
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
         *     ĳ���� �����͸� ������ ������ ��ȿ�Ѱ�..?
         * *****/
        if(_asset==null){
            _asset = AssetDatabase.LoadAssetAtPath<HierarchyIConDrawerAsset>(GetAssetPath());
        }

        if(_asset==null || _asset.IconList==null || _cacheMap==null || _cacheList==null || _asset.IconList.Count==0){
            return;
        }



        /***********************************************************************
         *     Hierarchy â�� �ִ� ��� GameObject���� ������ ������ ĳ���Ѵ�...
         * *****/
        GameObject[] sceneObjs = GameObject.FindObjectsOfType<GameObject>();

        int objCount   = sceneObjs.Length;
        int compCount  = _asset.IconList.Count;
        int startIdx   = 0;
        int drawCount  = 0;

        _cacheList.Clear();
        _cacheMap.Clear();

        for(int i=0; i<objCount; i++)
        {
            GameObject currObj = sceneObjs[i];
            for(int j=0; j<compCount; j++)
            {
                HierarchyIConDrawerAsset.IconData currComp = _asset.IconList[j];

                /*�ش� ������Ʈ�� �������� �ʴٸ� ��ŵ�Ѵ�.....*/
                if (currComp.Icon == null || currObj.GetComponent(currComp.ClassName) == null){
                    continue;
                }

                drawCount++;
                _cacheList.Add(j);
            }

            /**ĳ���� ������ �ִٸ� �߰��Ѵ�...*/
            if (drawCount>0){
                _cacheMap.Add(currObj.GetInstanceID(), new CacheData() { StartIdx=startIdx, Count=drawCount });
            }

            startIdx += drawCount;
            drawCount = 0;
        }
        #endregion
    }

}
#endif