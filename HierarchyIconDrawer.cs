using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static UnityEngine.GraphicsBuffer;

#if UNITY_EDITOR
using UnityEditor;

/**************************************************************************
 *     Hierarchy Window�� ������ �������� �׷��ִ� static class...
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
        int len       = _asset.IconList.Count;
        int showCount = 0; 

        if (_asset == null || _asset.ShowIcon == false || _cacheList==null || _cacheList.Count==0){
            return;
        }



        /*********************************************************************
         *    ĳ�̵� �����Ͱ� �����Ѵٸ�, ĳ�̵� �ε����� �����ܸ� ��� ǥ���Ѵ�....
         * ******/
        if (_cacheList.ContainsKey(instanceID) == false) return;

        for (int i=0; i<len; i++)
        {
            /***�ش� ������Ʈ�� ������� �ʴ´ٸ� ��ŵ�Ѵ�....**/
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
         *     ĳ���� �����͸� ������ ������ ��ȿ�Ѱ�..?
         * *****/
        if(_asset==null){
            _asset = AssetDatabase.LoadAssetAtPath<HierarchyIConDrawerAsset>(GetAssetPath());
        }

        if(_asset==null || _asset.IconList==null || _cacheList==null || _asset.IconList.Count==0){
            return;
        }



        /***********************************************************************
         *     Hierarchy â�� �ִ� ��� GameObject���� ������ ������ ĳ���Ѵ�...
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

                /*�ش� ������Ʈ�� �������� �ʴٸ� ��ŵ�Ѵ�.....*/
                if (currComp.Icon == null || currObj.GetComponent(currComp.ClassName) == null){
                    continue;
                }

                cacheValue |= (1<<j);
            }

            /**ĳ���� ������ �ִٸ� �߰��Ѵ�...*/
            if (cacheValue!=0){
                _cacheList.Add(currObj.GetInstanceID(), cacheValue);
            }

            cacheValue = 0;
        }

        #endregion
    }

}
#endif