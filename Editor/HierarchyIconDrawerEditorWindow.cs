using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

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

        EditorGUI.BeginDisabledGroup(_showWnd);
        {
            using (var scroll = new EditorGUILayout.ScrollViewScope(_scrollPos, false, true))
            {
                _scrollPos = scroll.scrollPosition;

                /***아이콘 표시/정렬 방식에 대한 변화를 감지한다.....**/
                using (var scope = new EditorGUI.ChangeCheckScope())
                {
                    _asset.ShowIcon = EditorGUILayout.Toggle("Show Icon", _asset.ShowIcon);
                    _asset.Aligment = (HierarchyIConDrawerAsset.AligmentType)EditorGUILayout.EnumPopup("aligment", _asset.Aligment, GUILayout.Width(300f));

                    if (scope.changed){
                        EditorUtility.SetDirty(_asset);
                        EditorApplication.RepaintHierarchyWindow();
                    }
                }


                /**처음 사용하는이를 위한 헬프박스를 표시한다....**/
                if (_asset.IconList.Count == 0){
                    EditorGUILayout.HelpBox("Please specify the types of icons to be displayed in the Hierarchy window and assign each icon to its corresponding Component.", MessageType.Info);
                }


                /**아이콘 리스트를 표시한다...**/
                GUI_ApplyPicker();

                _list.DoLayoutList();
            }

            /**-------------------------------------**/
        }
        EditorGUI.EndDisabledGroup();



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
                    _asset = ScriptableObject.CreateInstance<HierarchyIConDrawerAsset>();
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
            _list.onRemoveCallback    = GUI_RemoveElement;
            _list.onReorderCallback   = GUI_ReorderElement;
            _list.onCanRemoveCallback = GUI_CanRemove;
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

        if(_lastSelectedIconIdx < 0 || commandName!= "ObjectSelectorSelectionDone"){
            return;
        }

        HierarchyIConDrawerAsset.IconData data = _asset.IconList[_lastSelectedIconIdx];

        bool prevIsNull = (data.Icon==null);
        Texture2D tex;
        if ((tex = (EditorGUIUtility.GetObjectPickerObject() as Texture2D))!=null){

            data.Icon = tex;
            if (prevIsNull && System.Type.GetType(data.ClassName) != null)
            {
                HierarchyIconDrawer.RefereshTypeCache();
                HierarchyIconDrawer.RefreshDrawCache();
            }

            EditorUtility.SetDirty(_asset);
            EditorApplication.RepaintHierarchyWindow();
            Repaint();

            _lastSelectedIconIdx = -1;
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
        float   fieldHeight     = (rect.height*.9f);
        float   fieldY          = (rect.y);
        Rect    labelRect       = new Rect(rect.x, fieldY, 60f, fieldHeight);
        Rect    iconRect        = new Rect(rect.x+62f, fieldY, 25f, fieldHeight);
        Rect    classFieldRect  = new Rect(rect.x+62f+27f, fieldY, (rect.width-62f-27f), fieldHeight);
        Color   oldContentColor = GUI.contentColor; 


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
        _classContent.text  = element.DisplayName;
        GUI.backgroundColor = (System.Type.GetType(element.ClassName)==null ? Color.red : oldContentColor);

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

        GUI.backgroundColor = oldContentColor;
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

                string name  = _classList[i].FullName;
                string assem = _classList[i].Assembly.GetName().Name;


                /**검색 조건에서 벗어난다면 제외한다.....*/
                if (name==null || name.IndexOf(_wndIndexStr) == -1) {
                    continue;
                }

                _classContent.text = name;
                if (GUILayout.Button(_classContent, GUILayout.Width(_wndRect.width * .9f), GUILayout.Height(20f)))
                {
                    HierarchyIConDrawerAsset.IconData data = _asset.IconList[_lastSelectedClassIdx];
                    data.ClassName        = $"{name}, {assem}";
                    data.DisplayName      = name;
                    _lastSelectedClassIdx = -1;
                    _showWnd              = false;

                    EditorUtility.SetDirty(_asset);

                    //아이콘도 유효하다면 갱신한다...
                    if (data.Icon!=null){
                        HierarchyIconDrawer.RefereshTypeCache();
                        HierarchyIconDrawer.RefreshDrawCache();
                        EditorApplication.RepaintHierarchyWindow();
                    }
                }
            }
        }

        GUI.DragWindow();
        #endregion
    }

    private void GUI_RemoveElement(ReorderableList list)
    {
        #region Omit
        List<HierarchyIConDrawerAsset.IconData>                data    = _asset.IconList;
        System.Collections.ObjectModel.ReadOnlyCollection<int> selects = list.selectedIndices;

        int selectsCount = selects.Count;
        int removeCount  = 0;

        /**선택된 것들이 있을 경우.....**/
        if (selectsCount > 0)
        {
            for (int i = 0; i < selects.Count; i++){
                data.RemoveAt(selects[i] - (removeCount++));
            }
        }


        /***아니라면 가장 마지막 원소를 삭제한다...**/
        else data.RemoveAt(data.Count-1);


        /**갱신한다....**/
        HierarchyIconDrawer.RefereshTypeCache();
        HierarchyIconDrawer.RefreshDrawCache();
        EditorApplication.RepaintHierarchyWindow();
        #endregion
    }

    private void GUI_ReorderElement(ReorderableList list)
    {
        #region Omit
        HierarchyIconDrawer.RefereshTypeCache();
        HierarchyIconDrawer.RefreshDrawCache();
        EditorApplication.RepaintHierarchyWindow();
        #endregion
    }

    private bool GUI_CanRemove(ReorderableList list)
    {
        return (_asset.IconList.Count > 0);
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
                    .Where(type => type.IsSubclassOf(typeof(Component)))
                    .ToList();
        #endregion
    }

}

#endif