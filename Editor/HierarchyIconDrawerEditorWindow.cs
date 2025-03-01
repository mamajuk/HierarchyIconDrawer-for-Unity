using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.Text;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;


/*************************************************************
 *   HierarchyIconDrawer의 설정들을 표시하는 윈도우 창입니다...
 * *****/
public sealed class HierarchyIconDrawerEditorWindow : EditorWindow
{
    //=========================================================
    ////////                Fields...                 /////////
    ///========================================================
    
    /**static and constants...**/
    private const string             _AssetName    = "HierarchyIconDrawerData.asset";
    private const string             _FocusName    = "SearchBarFocus";
    private static List<System.Type> _classList;


    /**gui fields....**/
    private HierarchyIConDrawerAsset _asset;
    private GUIContent               _classContent;
    private GUIContent               _searchContent;
    private StringBuilder            _strBuilder   = new StringBuilder();

    private SerializedObject  _assetObject;
    private ReorderableList   _list;
    private Vector2           _scrollPos            = Vector2.zero;
    private int               _lastSelectedIconIdx  = -1,
                              _lastSelectedClassIdx = -1;


    /***Wnd fields....***/
    private bool              _showWnd      = false;
    private Rect              _wndRect      = new Rect(0f, 0f, 530f, 600f);
    private Vector2           _wndScrollPos = new Vector2();
    private string            _wndIndexStr  = "";



    //==================================================================
    /////////         Magic and Override methods...            /////////
    ///=================================================================
    [MenuItem("Utility/HierarchyIconDrawer Settings")]
    public static void OpenWindow()
    {
        EditorWindow.GetWindow(typeof(HierarchyIconDrawerEditorWindow), false, "HierarchyIconDrawer");
    }

    public void OnGUI()
    {
        if (GUI_Initialized()==false) return;

        using (var scroll = new EditorGUILayout.ScrollViewScope(_scrollPos, false, true))
        {
            _scrollPos = scroll.scrollPosition;

            /**-------------------------------------**/
            EditorGUI.BeginDisabledGroup(_showWnd);
            {

                /**========================================**/
                using (var scope = new EditorGUI.ChangeCheckScope())
                {
                    _asset.ShowIcon = EditorGUILayout.ToggleLeft("Show Icon", _asset.ShowIcon);

                    GUI_ApplyPicker();

                    EditorGUILayout.HelpBox("Please specify the types of icons to be displayed in the Hierarchy window and assign each icon to its corresponding Component.", MessageType.Info);

                    _list.DoLayoutList();


                    /**최종 변경사항을 저장한다....**/
                    if (scope.changed){
                        _assetObject.ApplyModifiedProperties();
                        EditorUtility.SetDirty(_asset);
                        HierarchyIconDrawer.RefreshCacheData();
                        EditorApplication.RepaintHierarchyWindow();
                    }
                }
                /**==========================================**/

            }
            EditorGUI.EndDisabledGroup();
            /**-------------------------------------**/
        }


        /************************************************
         *     클래스 선택 윈도우를 표시한다.....
         * ****/
        if (_showWnd == false || _asset == null || _asset.IconList == null || _lastSelectedClassIdx<0){
            return;
        }

        /***윈도우가 포커스를 잃는다면 닫는다...**/
        Event curr = Event.current;
        if (_showWnd && curr.type == EventType.MouseDown && !_wndRect.Contains(curr.mousePosition))
        {
            _showWnd = false;
            curr.Use();
        }

        BeginWindows();
        {
            _wndRect = GUI.Window(100, _wndRect, GUI_DrawWindowContent, "Select component");
        }
        EndWindows();
    }



    //======================================================
    ///////              GUI methods...             ////////
    //======================================================
    private bool GUI_Initialized()
    {
        #region Omit

        /*****************************************
         *     에셋이 존재하지 않다면 생성한다...
         * *****/
        if(_asset==null){
            string path = GetAssetPath();
            try
            {
                if ((_asset = AssetDatabase.LoadAssetAtPath<HierarchyIConDrawerAsset>(path)) == null){
                    _asset = new HierarchyIConDrawerAsset();
                    AssetDatabase.CreateAsset(_asset, path);
                }
            }
            catch { return false; }

            _list        = null;
            _assetObject = null;
        }



        /***********************************************
         *    SerializedProperty/Object를 초기화한다...
         * *****/
        if(_assetObject==null && _asset!=null){
            _assetObject = new SerializedObject(_asset);
        }



        /******************************************
         *    나머지 요소들을 초기화한다....
         * *****/
        if (_list==null && _asset!=null && _asset.IconList!=null){
            _list = new ReorderableList(_asset.IconList, typeof(List<HierarchyIConDrawerAsset.IconData>));
            _list.multiSelect         = true;
            _list.drawHeaderCallback  = GUI_DrawListHeader;
            _list.drawElementCallback = GUI_DrawElement;
        }

        if (_classContent==null){
            _classContent = new GUIContent("", EditorGUIUtility.IconContent("cs Script Icon").image);
        }

        if(_searchContent==null){
            _searchContent = new GUIContent("", EditorGUIUtility.IconContent("Search Icon").image);
        }

        return (_asset!=null && _list!=null && _classContent!=null);
        #endregion
    }

    private void GUI_ApplyPicker()
    {
        #region Omit
        /*******************************************************
        *    선택된 아이콘/스크립트에 대한 최종 처리를  진행한다....
        * *****/
        string commandName = Event.current.commandName;
        if (commandName == "ObjectSelectorUpdated")
        {
            /**아이콘에 대한 처리....**/
            if (_lastSelectedIconIdx >= 0){
                _asset.IconList[_lastSelectedIconIdx].Icon = EditorGUIUtility.GetObjectPickerObject() as Texture2D;
            }

            HierarchyIconDrawer.RefreshCacheData();
            EditorApplication.RepaintHierarchyWindow();
            Repaint();
        }
        #endregion
    }

    private void GUI_DrawListHeader(Rect rect)
    {
        #region Omit
        int count = (_asset.IconList!=null? _asset.IconList.Count:0);
        GUI.Label(rect, $"Component Icon List({count})");
        #endregion
    }

    private void GUI_DrawElement(Rect rect, int index, bool isActive, bool isFocused)
    {
        #region Omit
        /*****************************************************
         *    내용물을 모조리 표시한다.....
         * *****/
        HierarchyIConDrawerAsset.IconData element = _asset.IconList[index];
        float   fieldHeight    = (rect.height*.9f);
        float   fieldY         = (rect.y);
        float   oldLabelWidth  = EditorGUIUtility.labelWidth;
        Rect    labelRect      = new Rect(rect.x, fieldY, 60f, fieldHeight);
        Rect    iconRect       = new Rect(rect.x+62f, fieldY, 25f, fieldHeight);
        Rect    classFieldRect = new Rect(rect.x+62f+27f, fieldY, (rect.width-62f-27f), fieldHeight);


        /**아이콘이 그려지는 순서를 표시한다....**/
        EditorGUI.LabelField(labelRect, $"Icon ({index})");


        /*************************************************
         *    사용할 아이콘을 갱신한다.....
         * ******/
        if (element.Icon == null && GUI.Button(iconRect, ""))
        {
            EditorGUIUtility.ShowObjectPicker<Texture2D>(null, false, "", 0);
            _lastSelectedIconIdx = index;
        }
        else if (GUI.Button(iconRect, element.Icon))
        {
            EditorGUIUtility.ShowObjectPicker<Texture2D>(null, false, "", 0);
            _lastSelectedIconIdx = index;
        }


        /*************************************************
         *   적용할 컴포넌트 종류를 갱신한다....   
         ******/
        _classContent.text = element.ClassName;
        if (GUI.Button(classFieldRect, _classContent))
        {
            float width     = position.width;
            float height    = position.height;
            float wndWidth  = 600f;
            float wndHeight = 600f;
            Rect  wndRect   = new Rect(0f, 0f, wndWidth, 600f);
            Event curr      = Event.current;

            if (width < wndWidth){
                wndWidth      = (width*.8f);
                wndRect.width = wndWidth;
            }

            if((height * .9f) <= wndHeight)
            {
                wndHeight      = (height*.4f);
                wndRect.height = wndHeight;
            }

            /**마우스 위치로 윈도우를 생성시킨다....*/
            if (curr!=null){
                wndRect.x = Mathf.Clamp(curr.mousePosition.x-50f, 0f, (width-wndWidth));
                wndRect.y = Mathf.Clamp((curr.mousePosition.y-30f), 0f, (height-wndHeight));
            }

            _showWnd              = true;
            _wndIndexStr          = "";
            _wndScrollPos         = Vector2.zero;
            _wndRect              = wndRect;
            _lastSelectedClassIdx = index;
             EditorGUI.FocusTextInControl(_FocusName);
        }

        #endregion
    }

    private void GUI_DrawWindowContent(int id)
    {
        #region Omit

        /**클래스 목록들이 없다면 갱신한다.....**/
        if(_classList==null){
            _classList = GetScriptTypes();
        }

        /*******************************************************
         *     검색창을 표시한다.....
         * ******/
        float oldLabelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 20f;
        {
            GUI.SetNextControlName(_FocusName);
            _wndIndexStr = EditorGUILayout.TextField(_searchContent, _wndIndexStr, GUILayout.Width(_wndRect.width * .9f));
        }
        EditorGUIUtility.labelWidth = oldLabelWidth;



        /***************************************************
         *    클래스 목록들을 표시한다......
         * ****/
        using (var slider = new EditorGUILayout.ScrollViewScope(_wndScrollPos, false, true, GUILayout.Height(_wndRect.height * .9f)))
        {
            _wndScrollPos = slider.scrollPosition;

            int count = _classList.Count;
            for (int i = 0; i < count; i++) {

                _strBuilder.Clear();
                string name = _classList[i].Name;
                string space = _classList[i].Namespace;
                string finalStr;

                /**유효한 이름들을 추가한다...**/
                if (space != null) _strBuilder.Append(space).Append(".");
                if (name != null) _strBuilder.Append(name);

                /**검색 조건에서 벗어난다면 제외한다.....*/
                if ((finalStr = _strBuilder.ToString()).IndexOf(_wndIndexStr) == -1) {
                    continue;
                }

                _classContent.text = finalStr;
                if (GUILayout.Button(_classContent, GUILayout.Width(_wndRect.width * .9f), GUILayout.Height(20f)))
                {
                    _showWnd = false;
                    _asset.IconList[_lastSelectedClassIdx].ClassName = finalStr;
                    _lastSelectedClassIdx = -1;
                    HierarchyIconDrawer.RefreshCacheData();
                    EditorApplication.RepaintHierarchyWindow();
                }
            }
        }

        GUI.DragWindow();
        #endregion
    }



    //========================================================
    //////////         Utility methods...           //////////
    ///=======================================================
    private string GetAssetPath()
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

    private List<System.Type> GetScriptTypes()
    {
        #region Omit
        return AppDomain.CurrentDomain
                    .GetAssemblies()
                    .SelectMany(assembly => assembly.GetTypes())
                    .Where(type => type.IsSubclassOf(typeof(MonoBehaviour)))
                    .ToList();
        #endregion
    }

}

#endif