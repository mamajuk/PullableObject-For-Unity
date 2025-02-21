using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

/***********************************************
 *   당겨질 수 있는 효과를 제공하는 컴포넌트입니다...
 * **/
public sealed class PullableObject : MonoBehaviour
{
    #region Editor_Extension
    /****************************************
     *   에디터 확장을 위한 private class...
     * ***/
#if UNITY_EDITOR
    [CanEditMultipleObjects]
    [CustomPropertyDrawer(typeof(BoneData))]
    private sealed class BoneDataDrawer : PropertyDrawer
    {
        //====================================
        /////           Fields            ////
        //====================================
        private SerializedProperty TargetProperty;



        //=====================================================
        /////         Override and Magic methods          /////
        //=====================================================
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if(GUI_Initialized(property)==false) return;

            /**프로퍼티를 표시한다....*/
            GUI_ShowObjField(position);
        }



        //======================================================
        /////           GUI and Utility methods             ////
        //======================================================
        private float GetBaseHeight()
        {
            return GUI.skin.textField.CalcSize(GUIContent.none).y;
        }

        private bool GUI_Initialized(SerializedProperty property)
        {
            return (TargetProperty = property.FindPropertyRelative("Tr")) != null;
        }

        private void GUI_ShowObjField(Rect header)
        {
            #region Omit
            if (TargetProperty == null) return;

            using (var changedScope = new EditorGUI.ChangeCheckScope()){

                Transform curr = (Transform)EditorGUI.ObjectField(header, "bone Transform", TargetProperty.objectReferenceValue, typeof(Transform), true);

                /**값이 변경되었다면 갱신한다...*/
                if(changedScope.changed)
                {
                    TargetProperty.objectReferenceValue = curr;
                }

            }


            #endregion
        }
    }

    [CustomEditor(typeof(PullableObject))]
    private sealed class PullableObjectEditor : Editor
    {
        private enum ToggleStyle
        {
            Left,
            Middle,
            Right
        }

        //====================================
        //////         Fields             ////
        //====================================
        private const int                         BONE_QUAD_SIZE  = 12;
        private static readonly UnityEngine.Color BONE_QUAD_COLOR = new UnityEngine.Color(.8f, .8f, .8f);
        private static readonly UnityEngine.Color GRAB_QUAD_COLOR = new UnityEngine.Color(0f, .8f, 0f);

        private static GUIStyle    NodeButtonStyle, GrabNodeButtonStyle;
        private GUIStyle[]         toggleOptions;

        /**select target...*/
        private PullableObject     targetObj;
        private Transform          selectionData;
        private Tool               toolType = Tool.Move;

        /**Serialized Properties...*/
        private SerializedProperty dataListProperty;
        private SerializedProperty GrabTargetProperty;

        private SerializedProperty OnPullReleaseProperty;
        private SerializedProperty OnFullyExtendedProperty;
        private SerializedProperty OnPullStartProperty;
        private SerializedProperty OnBreakProperty;

        private SerializedProperty BreakLengthRatioProperty;
        private SerializedProperty StrechVibePowProperty;
        private SerializedProperty UseFixedVibeProperty;
        private SerializedProperty ApplyUpdateProperty;
        private SerializedProperty UseFixedMaxLengthProperty;
        private SerializedProperty FixedMaxLengthProperty;
        private SerializedProperty EventFlagsProperty;
        private SerializedProperty LineRendererProperty;


        //=====================================================
        /////          Magic and Override methods         /////
        //=====================================================
        private void OnSceneGUI()
        {
            #region Omit
            if (targetObj == null) return;
            if (targetObj._BoneData==null) return;
            if (targetObj._BoneData.Length==0) return;


            /**********************************************
             *   모든 본들을 표시한다.
             * ***/
            BoneData[]  datas = targetObj._BoneData;
            int         Count = datas.Length;

            Handles.BeginGUI();
            {
                Vector3 prevPos = (datas[0].Tr!=null? HandleUtility.WorldToGUIPoint(datas[0].Tr.position):Vector3.zero);

                /**잡힌 오브젝트가 있다면 트랜스폼을 편집할 수 있도록 한다...*/
                if (targetObj.HoldingPoint != null)
                {
                    Vector3 grabPos = targetObj.HoldingPoint.transform.position;
                    Vector3 grabGUIPos = HandleUtility.WorldToGUIPoint(grabPos);

                    if (GUI_ShowBoneButton(grabGUIPos, GrabNodeButtonStyle))
                    {
                        selectionData = targetObj.HoldingPoint.transform;
                    }
                }

                /**GUI를 표시한다....*/
                for (int i = 0; i < Count; i++){

                    if (datas[i].Tr == null) continue;
                    Vector3 pos    = datas[i].Tr.position;
                    Vector3 guiPos = HandleUtility.WorldToGUIPoint(pos);

                    Handles.color = BONE_QUAD_COLOR;
                    Handles.DrawLine( prevPos, guiPos, 1f );

                    prevPos = guiPos;

                    /**해당 GUI버튼을 누르면 트랜스폼을 편집할 수 있도록 한다..*/
                    if (GUI_ShowBoneButton(guiPos, NodeButtonStyle))
                    {
                        selectionData = datas[i].Tr;
                    }
                }

                /**잡힌 오브젝트가 있다면 트랜스폼을 편집할 수 있도록 한다...*/
                if (targetObj.HoldingPoint != null){

                    Vector3 grabPos = targetObj.HoldingPoint.transform.position;
                    Vector3 grabGUIPos = HandleUtility.WorldToGUIPoint(grabPos);

                    if (GUI_ShowBoneButton(grabGUIPos, GrabNodeButtonStyle))
                    {
                        selectionData = targetObj.HoldingPoint.transform;
                    }
                }
            }
            Handles.EndGUI();


            /*****************************************************
             *   선택한 본이 있다면 트랜스폼을 편집할 수 있도록 한다...
             * ***/
            if(selectionData!=null){

                /*********************************************
                 *   해당 오브젝트가 비활성화 되었다면 스킵한다...
                 * ***/
                Tools.current = Tool.None;
                using (var scope = new EditorGUI.ChangeCheckScope())
                {
                    Vector3     newPos  = selectionData.position;
                    Vector3     guiPos = HandleUtility.WorldToGUIPoint(newPos);
                    Quaternion  newQuat = selectionData.rotation;

                   /*********************************************
                    *   도구를 변경한다...
                    * ***/
                   Event curr = Event.current;
                    if(curr.type==EventType.KeyDown)
                    {
                        switch(curr.keyCode){

                                case (KeyCode.W):
                                {
                                    toolType = Tool.Move;
                                    break;
                                }

                                case (KeyCode.E):
                                {
                                    toolType = Tool.Rotate;
                                    break;
                                }
                        }
                    }

                    /**********************************************
                     *   현재 선택된 도구에 따라 트랜스폼 변경을 한다..
                     * ***/
                    switch (toolType){

                            /**이동 도구일 경우...*/
                            case (Tool.Move):
                            {
                                newPos = Handles.PositionHandle(selectionData.position, newQuat);
                                break;
                            }

                            /**회전 도구일 경우...*/
                            case (Tool.Rotate):
                            {
                                newQuat = Handles.RotationHandle(selectionData.rotation, newPos);
                                break;
                            }
                    }

                    /**값이 바뀌었다면 갱신한다...*/
                    if (scope.changed){

                        Undo.RecordObject(selectionData, $"Changed transform of {selectionData.name}.");
                        selectionData.position = newPos;
                        selectionData.rotation = newQuat;
                    }

                    Handles.BeginGUI();
                    {
                        GUI_ShowBoneTransform(guiPos, selectionData);
                    }
                    Handles.EndGUI();
                }
            }

            #endregion
        }

        private void OnEnable()
        {
            GUI_Initialized();
            selectionData = null;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            /*****************************************
             *   모든 프로퍼티들을 표시한다...
             * ***/
            GUI_Initialized();

            GUI_ShowGrabTarget();

            GUI_ShowExtendedRatio();

            GUI_ShowBreakLengthRatio();

            GUI_ShowStrechVibe();

            //GUI_ShowFixedMaxLength();

            EditorGUILayout.Space(10f);


            GUI_ShowResetRotateButton();
            GUI_ShowAutoSetDatasButton();

            GUI_ShowDataList();

            EditorGUILayout.Space(10f);


            GUI_ShowEvents();

            if(GUI.changed){

                serializedObject.ApplyModifiedProperties();
            }
        }



        //==========================================
        //////          GUI methods             ////
        //==========================================
        private void GUI_Initialized()
        {
            #region Omit
            /****************************************
             *   모든 프로퍼티와 스타일들을 초기화한다..
             * ***/

            /**스타일 초기화....*/
            Texture2D t = new Texture2D(1, 1);
            t.SetPixel(0, 0, BONE_QUAD_COLOR);
            t.Apply();

            Texture2D t2 = new Texture2D(1, 1);
            t2.SetPixel(0, 0, GRAB_QUAD_COLOR);
            t2.Apply();

            NodeButtonStyle = new GUIStyle();
            NodeButtonStyle.normal.background = t;

            GrabNodeButtonStyle= new GUIStyle();
            GrabNodeButtonStyle.normal.background = t2;

            /**직렬화 프로퍼티 초기화...*/
            if(targetObj==null){

                targetObj                       = (target as PullableObject);
                dataListProperty                = serializedObject.FindProperty("_BoneData");
                GrabTargetProperty              = serializedObject.FindProperty("_GrabTarget");
                BreakLengthRatioProperty        = serializedObject.FindProperty("MaxScale");
                ApplyUpdateProperty             = serializedObject.FindProperty("ApplyUpdate");
                StrechVibePowProperty           = serializedObject.FindProperty("StrechVibePow");
                UseFixedVibeProperty            = serializedObject.FindProperty("UseFixedVibe");
                UseFixedMaxLengthProperty       = serializedObject.FindProperty("UseFixedMaxLength");
                FixedMaxLengthProperty          = serializedObject.FindProperty("FixedMaxLength");
                EventFlagsProperty              = serializedObject.FindProperty("_eventFlags");

                OnPullReleaseProperty            = serializedObject.FindProperty("OnPullRelease");
                OnFullyExtendedProperty          = serializedObject.FindProperty("OnFullyExtended");
                OnPullStartProperty              = serializedObject.FindProperty("OnPullStart");
                OnBreakProperty                  = serializedObject.FindProperty("OnBreak");
                LineRendererProperty             = serializedObject.FindProperty("_LineRenderer");
            }

            /**스타일 룩업테이블 초기화....*/
            if (toggleOptions == null){

                toggleOptions = new GUIStyle[]
                {
                    EditorStyles.miniButtonLeft,
                    EditorStyles.miniButtonMid,
                    EditorStyles.miniButtonRight
                };
            }
            #endregion
        }

        private bool GUI_ShowBoneButton(Vector2 pos, GUIStyle style)
        {
            #region Omit
            float halfQuadSize  = (BONE_QUAD_SIZE * .5f);
            Rect btnRect        = new Rect(
                pos - new Vector2( halfQuadSize, halfQuadSize ),
                new Vector3( BONE_QUAD_SIZE, BONE_QUAD_SIZE )
            );

            return GUI.Button(btnRect, GUIContent.none, style);
            #endregion
        }

        private void GUI_ShowExtendedRatio()
        {
            #region Omit
            if (targetObj == null) return;

            EditorGUILayout.TextField("Current Length", $"{targetObj.Length} ({targetObj.NormalizedLength}%)");

            #endregion
        }

        private void GUI_ShowBreakLengthRatio()
        {
            #region Omit
            if (BreakLengthRatioProperty == null) return;

            using (var scope = new EditorGUI.ChangeCheckScope())
            {
                float value = EditorGUILayout.FloatField("Max Scale", BreakLengthRatioProperty.floatValue);

                /**값이 바뀌었다면 갱신한다..*/
                if(scope.changed){

                    BreakLengthRatioProperty.floatValue = value;
                }
            }

            #endregion
        }

        private void GUI_ShowBoneTransform(Vector2 pos, Transform selectionData)
        {
            #region Omit
            Rect fieldRect = new Rect(
                pos - new Vector2(100f, -80f),
                new Vector3(300f, 40f)
            );

            /**이동 도구일 경우...*/
            if (toolType == Tool.Move){

                EditorGUI.Vector3Field(fieldRect, "", selectionData.position);
            }

            /**회전 도구일 경우...*/
            else
            {
                Quaternion.Euler(EditorGUI.Vector3Field(fieldRect, "", selectionData.localEulerAngles));
            }
            #endregion
        }

        private void GUI_ShowResetRotateButton()
        {
            #region Omit
            if (targetObj == null || targetObj._BoneData==null) return;

            /*************************************
             *   모든 본의 회전량을 초기화한다.
             * ***/
            if(GUILayout.Button("Reset all bones rotation"))
            {
                int Count        = targetObj._BoneData.Length;
                BoneData[] datas = targetObj._BoneData;
                if (targetObj._BoneData.Length == 0 || targetObj._BoneData[0].Tr==null) return;

                Undo.RegisterChildrenOrderUndo(targetObj._BoneData[0].Tr, "Changed All bones transform");
                for (int i=1; i<Count; i++){

                    if (datas[i].Tr == null) continue;
                    datas[i].Tr.localRotation = Quaternion.identity;
                }
            }

            #endregion
        }

        private void GUI_ShowAutoSetDatasButton()
        {
            #region Omit
            if (dataListProperty == null) return;

            if(GUILayout.Button("Add all child bones under the RootBone"))
            {
                if(dataListProperty.arraySize==0){

                    Debug.LogError("RootBone is not exist!");
                    return;
                }

                if (targetObj._BoneData[0].Tr==null){

                    Debug.LogError("RootBone transform is not valid!");
                    return;
                }


                /*****************************************************
                 *   계층구조가 존재하는 대상만 모두 bone으로 추가한다...
                 * ***/
                int index = 1;
                dataListProperty.arraySize = 1;


                Transform parent = targetObj._BoneData[0].Tr;
                Transform result;

                /**나머지 자식 본들을 추가한다....*/
                while ((result=FindChildManyChildren(parent))!=null)
                {
                    dataListProperty.arraySize++;

                    SerializedProperty newData      = dataListProperty.GetArrayElementAtIndex(index++);
                    SerializedProperty newDataTr    = newData.FindPropertyRelative("Tr");
                    newDataTr.objectReferenceValue  = result;
                    parent = result;
                }

                return;
            }
            #endregion
        }

        private void GUI_ShowDataList()
        {
            #region Omit
            if (dataListProperty == null) return;

            EditorGUILayout.PropertyField(dataListProperty);

            #endregion
        }

        private void GUI_ShowGrabTarget()
        {
            #region Omit
            if (GrabTargetProperty == null || LineRendererProperty==null) return;

            /**IK적용 여부...*/
            ApplyUpdateProperty.boolValue = EditorGUILayout.Toggle("Apply Update", ApplyUpdateProperty.boolValue);


            /**********************************************************
             *   잡는 지점을 나타내는 GameObject의 참조필드를 표시한다...
             * ****/
            EditorGUILayout.BeginHorizontal();
            {
                /**GrabTarget 참조필드...*/
                using (var scope = new EditorGUI.ChangeCheckScope()){

                    GameObject value = (GameObject)EditorGUILayout.ObjectField("Holding Point", GrabTargetProperty.objectReferenceValue, typeof(GameObject), true);
                    if (scope.changed) GrabTargetProperty.objectReferenceValue = value;
                }

            }
            EditorGUILayout.EndHorizontal();


            /***************************************************
             *   라인 랜더러의 참조필드를 표시한다....
             * ****/
            using (var scope = new EditorGUI.ChangeCheckScope()){

                Object value = EditorGUILayout.ObjectField("Line Renderer", LineRendererProperty.objectReferenceValue, typeof(LineRenderer), true);
                if (scope.changed) LineRendererProperty.objectReferenceValue = value;
            }


            #endregion
        }

        private void GUI_ShowEvents()
        {
            #region Omit
            if (OnPullStartProperty==null || OnPullReleaseProperty==null || OnFullyExtendedProperty==null || OnBreakProperty==null) 
                return;

            /*****************************************
             *   대리자 토글을 표시한다..
             * ****/
            GUILayout.BeginHorizontal();
            {
                GUI_ShowUseEventSelectToggle(0, "OnPullStart", ToggleStyle.Left);
                GUI_ShowUseEventSelectToggle(1, "OnPullRelease");
                GUI_ShowUseEventSelectToggle(2, "OnFullyExtended");
                GUI_ShowUseEventSelectToggle(3, "OnBreak", ToggleStyle.Right);
            }
            GUILayout.EndHorizontal();

            /******************************************
             *   현재 사용할 대리자들을 표시한다...
             * *****/
            GUI_ShowEvent(0, OnPullStartProperty);
            GUI_ShowEvent(1, OnPullReleaseProperty);
            GUI_ShowEvent(2, OnFullyExtendedProperty);
            GUI_ShowEvent(3, OnBreakProperty);

            #endregion
        }

        private void GUI_ShowStrechVibe()
        {
            #region Omit
            if (StrechVibePowProperty == null || UseFixedVibeProperty == null) return;

            /******************************************************
             *   줄이 완전히 펴졌을 때의 프로퍼티들을 모두 표시한다...
             * ***/
            EditorGUILayout.BeginHorizontal();
            {
                /**진동의 힘을 결정하는 프로퍼티를 표시한다...*/
                using (var scope = new EditorGUI.ChangeCheckScope()){

                    float value = EditorGUILayout.FloatField("Strech Vibration Pow", StrechVibePowProperty.floatValue);
                    if (scope.changed) 
                        StrechVibePowProperty.floatValue = Mathf.Clamp(value, 0f, float.MaxValue);
                }

                /**고정 힘을 사용하는지에 대한 여부를 결정한다....*/
                UseFixedVibeProperty.boolValue = EditorGUILayout.ToggleLeft("Use Fixed Pow", UseFixedVibeProperty.boolValue, GUILayout.Width(140f));
            }
            EditorGUILayout.EndHorizontal();
            #endregion
        }

        private void GUI_ShowFixedMaxLength()
        {
            #region Omit
            if (FixedMaxLengthProperty == null || UseFixedMaxLengthProperty == null) return;

            /**********************************************************
             *   d고정된 최대길이를 사용하는지에 대한 프로퍼티를 표시한다...
             * ***/
            EditorGUILayout.BeginHorizontal();
            {
                /**진동의 힘을 결정하는 프로퍼티를 표시한다...*/
                using (var scope = new EditorGUI.ChangeCheckScope()){

                    float value = EditorGUILayout.FloatField("Fixed Max Length", FixedMaxLengthProperty.floatValue);
                    if (scope.changed)
                        FixedMaxLengthProperty.floatValue = Mathf.Clamp(value, 0f, float.MaxValue);
                }

                /**고정 힘을 사용하는지에 대한 여부를 결정한다....*/
                UseFixedMaxLengthProperty.boolValue = EditorGUILayout.ToggleLeft("Use Fixed MaxLength", UseFixedMaxLengthProperty.boolValue, GUILayout.Width(140f));
            }
            EditorGUILayout.EndHorizontal();
            #endregion
        }

        private void GUI_ShowEvent(int index, SerializedProperty property)
        {
            #region Omit
            if (EventFlagsProperty == null || property == null) return;

            /************************************************
             *   해당 플래그가 유효하면 대리자를 표시한다.....
             * *****/
            int flags = EventFlagsProperty.intValue;
            if( (flags & (1<<index))!=0 )
            {
                EditorGUILayout.PropertyField(property);
            }

            #endregion
        }

        private void GUI_ShowUseEventSelectToggle(int eventIndex, string eventName, ToggleStyle style = ToggleStyle.Middle)
        {
            #region Omit
            if (EventFlagsProperty == null) return;
            int layer           = (1 << eventIndex);
            bool eventIsUsed    = (EventFlagsProperty.intValue & layer) != 0;
            Color returnBgColor = GUI.backgroundColor;

            /****************************************************
             *   지정한 버튼을 표시한다....
             * ***/
            using (var scope = new EditorGUI.ChangeCheckScope()){

                GUI.backgroundColor = (eventIsUsed ? Color.red : Color.green);
                {
                    eventIsUsed = GUILayout.Toggle(eventIsUsed, eventName, toggleOptions[(int)style]);
                }
                GUI.backgroundColor = returnBgColor;

                /**값이 바뀌었을 경우 갱신한다...*/
                if (scope.changed)
                {
                    if (eventIsUsed) EventFlagsProperty.intValue |= layer;
                    else EventFlagsProperty.intValue &= ~layer;
                }
            }
            #endregion
        }



        //============================================
        /////           Utility methods           ////
        //============================================
        private Transform FindChildManyChildren(Transform parent)
        {
            #region Omit
            int childrenNum = parent.childCount;
            int       maxCount    = -1;
            Transform result      = null;

            for(int i=0; i<childrenNum; i++)
            {
                Transform child = parent.GetChild(i);
                
                /**더 자식수가 많은 대상만을 추가한다...*/
                if(maxCount<child.childCount){

                    maxCount = child.childCount;
                    result = child;
                }
            }
            return result;
            #endregion
        }

    }
#endif
    #endregion

    #region Define
    public delegate void PullableDelegate();

    [System.Serializable]
    public sealed class PullableObjEvent : UnityEvent
    {
    }

    [System.Serializable]
    private struct BoneData
    {
        public Transform  Tr;

        [System.NonSerialized] public Vector3    OriginPos;
        [System.NonSerialized] public Quaternion OriginQuat;
        [System.NonSerialized] public Quaternion PrevQuat;
        [System.NonSerialized] public Vector3    PrevPos;
        [System.NonSerialized] public Vector3    originDir;
        [System.NonSerialized] public Vector3    LastPos;
        [System.NonSerialized] public Vector3    LastDir;
        [System.NonSerialized] public float     originLength;
        [System.NonSerialized] public float     originLengthDiv;
        [System.NonSerialized] public float     lengthRatio;
    }
    #endregion

    //=========================================
    /////            Property             /////
    //=========================================
    public float        MaxLength
    {
        get
        {
            /*************************************
             *   각 본들의 길이의 총합을 구한다.
             * ***/
            if(_fullyExtendedLen>0f && IsValid) return _fullyExtendedLen;

            float len = 0f;
            int Count = (!_awakeInit? _BoneData.Length:_dataCount);

            for (int i = 1; i < Count; i++){

                len += (_BoneData[i].Tr.position - _BoneData[i - 1].Tr.position).magnitude;
            }
            _cachedMaxLength = _fullyExtendedLen = len;

            return len;
        }
    }
    public float        NormalizedLength 
    {
        get
        {
            if (HoldingPoint == null || _BoneData == null || _BoneData.Length == 0 || _BoneData[0].Tr == null){

                return 0f;
            }

            float Root2Target = (HoldingPoint.transform.position - _BoneData[0].Tr.position).magnitude;

            /**최대길이 연산자가 초기화되지않았다면 초기화.*/
            if(_fullyExtendedDiv<0) {

                _fullyExtendedDiv = (1f / MaxLength);
            }

            return ( Root2Target * _fullyExtendedDiv );
        }
    }
    public float        Length
    {
        get
        {
            if (HoldingPoint == null || _BoneData == null || _BoneData.Length == 0 || _BoneData[0].Tr == null){

                return 0f;
            }

            return (HoldingPoint.transform.position - _BoneData[0].Tr.position).magnitude;
        }
    }
    public bool         IsBroken            { get; private set; } = false;
    public bool         IsValid             { get; private set; } = false;
    public bool         IsDestroy           { get; private set; } = false;
    public int          BoneCount           { get { return _dataCount; } }
    public Vector3      StartPosition
    {
        get
        {
            if(_BoneData == null || _BoneData.Length == 0 || _BoneData[0].Tr == null) 
                return Vector3.zero;

            return _BoneData[0].Tr.position;
        }
    }
    public Vector3      EndPosition
    {
        get
        {
            bool dataIsNull = (_BoneData == null);
            bool dataIsEmpty = (_BoneData.Length == 0);
            bool IndexOneIsNull = (_BoneData[_dataCount - 1].Tr == null);

            if (_BoneData == null || _BoneData.Length == 0 || _BoneData[_dataCount - 2].Tr == null) return Vector3.zero;

            return _BoneData[_dataCount-2].Tr.position;
        }
    }
    public GameObject   HoldingPoint 
    { 
        get { return _GrabTarget; } 
        set
        {
            _GrabTarget = value;
            if (_GrabTarget != null)
            {
                ApplyUpdate = true;
                OnPullStart?.Invoke();

                /**고정 최대길이를 사용할 경우...*/
                if(UseFixedMaxLength){

                    _fullyExtendedLen = 5f;
                    _fullyExtendedDiv = (1f / _fullyExtendedLen);
                }
            }
            else
            {
                OnPullRelease?.Invoke();

                /**고정 최대길이를 사용할 경우...*/
                if (UseFixedMaxLength){

                    _fullyExtendedLen = _cachedMaxLength;
                    _fullyExtendedDiv = (1f / _fullyExtendedLen);
                }
            }
        }
    }
    public LineRenderer LineRenderer
    {
        get { return _LineRenderer; }
        set
        {
            _LineRenderer = value;
            
            /**라인 랜더러가 유효할 경우 개수를 갱신한다....*/
            if(_LineRenderer!=null)
            {
                _LineRenderer.positionCount = _dataCount;
                _LineRenderer.useWorldSpace = true;
            }
        }
    }

    [SerializeField] public float            MaxScale          = 1.5f;
    [SerializeField] public float            StrechVibePow     = 3f;
    [SerializeField] public float            FixedMaxLength    = 1.5f;
    [SerializeField] public bool             UseFixedVibe      = false;
    [SerializeField] public bool             UseFixedMaxLength = false;
    [SerializeField] public bool             ApplyUpdate       = true;
    [SerializeField] private GameObject       _GrabTarget;
    [SerializeField] private LineRenderer     _LineRenderer;
    [SerializeField] public  PullableObjEvent OnPullRelease;
    [SerializeField] public  PullableObjEvent OnBreak;
    [SerializeField] public  PullableObjEvent OnPullStart;
    [SerializeField] public  PullableObjEvent OnFullyExtended;

    public PullableDelegate OnLateUpdate;



    //=======================================
    //////            Fields            /////
    //=======================================
    [SerializeField, HideInInspector] 
    private BoneData[] _BoneData;

    [SerializeField, HideInInspector]
    private int        _dataCount = -1;

    [SerializeField, HideInInspector]
    private int        _eventFlags = 0;

    private const int   _fabrikLimit       = 100;
    private float       _fullyExtendedLen = -1f;
    private float       _fullyExtendedDiv  = -1f;
    private float       _boneCountDiv      = 1f;
    private bool        _unpackParents     = false;
    private bool        _awakeInit         = false;


    /**줄의 반동에 관련된 필드...*/
    private float  _lastExtendedLen = 0f;
    private float  _Yspeed = 2f;
    private float  _boundTime  = 0f;


    /**줄의 끊어짐과 관련된 필드...*/
    private float       _brokenTime = .1f;
    private float       _brokenDiv  = 0f;
    private float       _cachedMaxLength = 0f;



    //===========================================
    /////          Magic methods            /////
    //===========================================
    private void Awake()
    {
        #region Omit
        gameObject.layer = LayerMask.NameToLayer("Interactable");
            
        if (OnPullStart == null)     OnPullStart     = new PullableObjEvent();
        if (OnFullyExtended == null) OnFullyExtended = new PullableObjEvent();
        if (OnPullRelease == null)   OnPullRelease   = new PullableObjEvent();
        if (OnBreak == null)         OnBreak         = new PullableObjEvent();

        Init(true, ApplyUpdate);

        LineRenderer = _LineRenderer;
        _awakeInit   = true;
        #endregion
    }

    private void LateUpdate()
    {
        #region Omit
        if (_dataCount < 2 || ApplyUpdate==false || IsValid==false) return;

        /*************************************
         *   외부로부터 당겨질 경우의 처리를 한다..
         * ***/
        if (HoldingPoint!=null){

            /**팽팽하게 당겨졌을 때의 처리...*/
            if (!UpdateFullExtendedVibration())
            {
                /**완전히 당겨지지 않았을 경우의 처리...*/
                UpdateLookAtTarget(HoldingPoint.transform.position);
            }

            UpdateLineRenderer();
            OnLateUpdate?.Invoke();
            return;
        }


        /****************************************
         *   당겨졌다가 놓아졌을 때의 처리를 한다...
         * ***/

        /**끊어졌을 때의 처리...*/
        if(!UpdateBreakRestore()){

            /**천천히 원상복귀되는 경우의 처리...*/
            UpdateExtendedRestore();
        }

        UpdateLineRenderer();
        OnLateUpdate?.Invoke();
        #endregion
    }

    private void OnDrawGizmos()
    {
        #region Omit
        if (HoldingPoint == null) return;

        Vector3 grabPos = HoldingPoint.transform.position;
        Vector3 rootPos = StartPosition;

        Vector3 root2GrabDir = (grabPos-rootPos).normalized;
        Gizmos.color = UnityEngine.Color.blue;
        Gizmos.DrawLine(rootPos, rootPos + root2GrabDir * _fullyExtendedLen);

        Gizmos.color = UnityEngine.Color.white;
        Gizmos.DrawLine(rootPos, grabPos);
        #endregion
    }



    //===========================================
    /////          Core methods             /////
    //===========================================
    private void UpdateLookAtTarget(Vector3 targetPos)
    {
        #region Omit
        if (_dataCount<2) return;

        /********************************************
         *   FABRIK 알고리즘으로, 대상을 가리키도록 한다...
         * ***/

        /**마지막 본에서 루트본까지 차례대로 트랜스폼을 변경한다...*/
        ApplyForwardIK(_dataCount - 2, targetPos);

        for (int i=_dataCount-3; i >= 0; i--){

            ref BoneData next = ref _BoneData[i + 1];
            ApplyForwardIK(i, next.Tr.position);
        }


        /****************************************************
         *   루트본이 원래 위치에 최대한 가깝게 이동하도록 보간...
         * ***/
        ref BoneData rootBone     = ref _BoneData[0];
        Transform    targetTr     = HoldingPoint.transform;

        int   leftCount     = _fabrikLimit;
        float root2forward  = 2f;

        /**루트본의 위치에 최대한 가깝게 배치시킨다...*/
        while (leftCount-- > 0 && root2forward > .05f)
        {
            /**루트본을 원래 위치에 붙여놓는다...*/
            rootBone.Tr.position = rootBone.OriginPos;

            for(int i=1; i<_dataCount-1; i++){

                ref BoneData prev = ref _BoneData[i-1];
                ApplyBackwardIK(i, prev.Tr.position);
            }

            root2forward = (rootBone.OriginPos - rootBone.Tr.position).sqrMagnitude;
        }

        LastBone2GrabSolver();

        /**마지막 위치를 기록한다...*/
        _lastExtendedLen = (HoldingPoint.transform.position - _BoneData[0].Tr.position).magnitude;
        #endregion
    }

    private void ApplyForwardIK(int applyIndex, Vector3 target)
    {
        #region Omit
        ref BoneData bone     = ref _BoneData[applyIndex];
        ref BoneData nextBone = ref _BoneData[applyIndex+1];

        Vector3 bone2Target = (target - bone.OriginPos).normalized;
        Quaternion rotQuat  = GetQuatBetweenVector(bone.originDir, bone2Target);

        bone.Tr.position        = target + bone2Target*(-bone.originLength);
        bone.Tr.rotation        = (rotQuat*bone.OriginQuat);
        #endregion
    }

    private void ApplyBackwardIK(int applyIndex, Vector3 target)
    {
        #region Omit
        ref BoneData bone = ref _BoneData[applyIndex];
        ref BoneData next = ref _BoneData[applyIndex+1];

        Vector3 bone2Target = (target-bone.Tr.position).normalized;
        bone.Tr.position    = target + (bone2Target * -bone.originLength);
        #endregion
    }

    private void LastBone2GrabSolver()
    {
        #region Omit
        /****************************************************
         *   BackwardIK로 인한 잡은 부분에 마지막 본이 닿지 않는
         *   문제점을 해결한다...
         * ***/
        if (HoldingPoint == null) return;

        ref BoneData lastData = ref _BoneData[_dataCount - 1];
        Vector3      grabPos  = HoldingPoint.transform.position;
        float last2TargetLen  = (grabPos - lastData.Tr.position).magnitude;
        float        partLen  = (last2TargetLen * _boneCountDiv);   

        for(int i=1; i<_dataCount-1; i++)
        {
            ref BoneData curr = ref _BoneData[i];
            ref BoneData prev = ref _BoneData[i-1];
            ref BoneData next = ref _BoneData[i + 1];

            Vector3 prev2CurrDir = (curr.Tr.position - prev.Tr.position).normalized;
            curr.Tr.position = prev.Tr.position + (prev2CurrDir * (prev.originLength+partLen));
        }

        #endregion
    }

    private bool UpdateFullExtendedVibration()
    {
        #region Omit
        if (HoldingPoint == null) return false;

        float root2TargetLen  = (HoldingPoint.transform.position - _BoneData[0].Tr.position).magnitude;
        float extendedRatio   = (root2TargetLen * _fullyExtendedDiv);

        /****************************************
         *   줄이 한계치를 넘어섰다면 파괴된다...
         * ***/
        if(!IsBroken && extendedRatio>=MaxScale)
        {
            IsBroken = true;
            _GrabTarget = null;
            _brokenTime *= .5f;
            return true;
        }


        /****************************************
         *   완전히 당겨졌을 때의 처리를 한다...
         * ***/
        else if (extendedRatio>=1f)
        {
            /**줄이 당겨져서 완전히 펴졌을 때의 처리...*/
            if(_lastExtendedLen>0){

                _Yspeed = StrechVibePow * (!UseFixedVibe? (root2TargetLen - _lastExtendedLen): 1f);
               _lastExtendedLen = 0;
                OnFullyExtended?.Invoke();
            }


            /******************************************
             *   계산에 필요한 요소들을 모두 구한다...
             * ***/

            /**계산에 참조할 본들의 참조를 구한다...*/
            ref BoneData root    = ref _BoneData[0];
            ref BoneData last    = ref _BoneData[_dataCount-1];

            /**당겨지는 방향의 업벡터를 이용하여 배지어 제어점들을 구한다...*/
            Vector3 forward       = (HoldingPoint.transform.position - root.OriginPos);
            Vector3 forwardNormal = forward.normalized;
            Vector3 right         = Vector3.Cross(Vector3.up, forwardNormal).normalized;
            Vector3 up            = Vector3.Cross(forwardNormal, right).normalized;

            Vector3 a  = root.OriginPos;
            Vector3 cp = root.OriginPos + (forward*.5f) + (up*_Yspeed);
            Vector3 b  = HoldingPoint.transform.position;



            /**********************************************
             *   배지어 곡선을 기반으로하여 본을 업데이트 한다....
             * ***/
            float ratio = 0f;
            int   count = (_dataCount - 1);
            for(int i=0; i<count; i++)
            {
                ref BoneData curr = ref _BoneData[i];
                ref BoneData next = ref _BoneData[i+1];

                Vector3 currBezier = GetBezier(ref a, ref cp, ref b, ratio);
                Vector3 nextBezier = GetBezier(ref a, ref cp, ref b, (ratio += curr.lengthRatio));
                Vector3 curr2Next  = (nextBezier-currBezier).normalized;
                Quaternion rotQuat = GetQuatBetweenVector(curr.originDir, curr2Next);

                next.Tr.position = nextBezier;
                curr.Tr.rotation = (rotQuat * curr.OriginQuat);
            }

            /**반동이 점점 감소하는 효과를 적용한다...*/
            if((_boundTime-=Time.deltaTime)<=0f)
            {
                _Yspeed = (-_Yspeed * .7f);
                _boundTime = .05f;
            }

            return true;
        }

        /**마지막 위치를 기록한다...*/
        else _lastExtendedLen = (HoldingPoint.transform.position - _BoneData[0].Tr.position).magnitude;

        return false;
        #endregion
    }

    private void UpdateExtendedRestore()
    {
        #region Omit

        /************************************
         *   각 본들이 원래 위치로 돌아간다...
         * ***/
        int   Count = (_dataCount-1);
        float delta = (Time.deltaTime * 3f);

        for (int i=0; i<_dataCount-1; i++)
        {
            ref BoneData curr = ref _BoneData[i];
            ref BoneData next = ref _BoneData[i + 1];

            Vector3 currDir    = (next.Tr.position - curr.Tr.position).normalized;
            Quaternion rotQuat = GetQuatBetweenVector(currDir, curr.originDir, delta);

            curr.Tr.position +=  (curr.OriginPos - curr.Tr.position) * delta;
            curr.Tr.rotation =  (rotQuat * curr.Tr.rotation);
        }
        #endregion
    }

    private bool UpdateBreakRestore()
    {
        #region Omit
        if (IsBroken == false) return false;

        /*******************************************
         *   빠르게 복귀하면서 점점 길이가 줄어든다...
         * ***/
        if (_brokenTime <= 0) return true;

        _brokenTime -= Time.deltaTime;
        float progressRatio = (_brokenTime * _brokenDiv);

        /**모든 본들의 크기를 줄인다.....*/
        for( int i=0; i<_dataCount-1; i++ )
        {
            ref BoneData curr = ref _BoneData[i];
            ref BoneData next = ref _BoneData[i + 1];

            Vector3 currDir = (next.Tr.position - curr.Tr.position).normalized;
            next.Tr.position = curr.Tr.position + (currDir * curr.originLength * progressRatio);
        }

        /**모두 줄어들었을 경우의 처리를 한다...*/
        if(progressRatio<=0f){

            OnBreak?.Invoke();
            IsDestroy = true;
            Destroy(gameObject);
        }
        return true;
        #endregion
    }

    private void WriteLastBoneTransform()
    {
        #region Omit

        /**각 본들의 마지막 위치를 기록한다...*/
        for (int i = 0; i < _dataCount - 1; i++)
        {
            ref BoneData curr = ref _BoneData[i];
            ref BoneData next = ref _BoneData[i + 1];

            curr.LastDir = (next.Tr.position - curr.Tr.position).normalized;
            curr.LastPos = curr.Tr.position;
        }
        #endregion
    }

    private void UnpackParent()
    {
        #region Omit
        if (_unpackParents) return;

        int Count = BoneCount;
        for(int i=0; i<Count; i++){

            ref BoneData data = ref _BoneData[i];
            data.Tr.parent    = transform;
        }

        _unpackParents = true;
        #endregion
    }

    private void Init(bool Apply, bool unpackParent=true)
    {
        #region Omit
        if (Apply == false || _awakeInit) return;
        
        _fullyExtendedLen = MaxLength;
        _fullyExtendedDiv = (1f / _fullyExtendedLen);
        _boneCountDiv     = (1f / (_BoneData.Length - 1));
        _brokenDiv        = (1f / _brokenTime);


        /**************************************
         *  본 정보 초기화....
         * ***/
        if (_BoneData == null) {

            _BoneData = new BoneData[10];
        }

        _dataCount = _BoneData.Length;
        for (int i = 0; i < _dataCount - 1; i++){

            ref BoneData data = ref _BoneData[i];
            ref BoneData next = ref _BoneData[i + 1];

            /**연결이 끊겨있다면 마무리...*/
            if (data.Tr == null || next.Tr==null)
            {
                _dataCount = (i + 1);
                IsValid    = (_dataCount >= 2);
                return;
            }

            if(unpackParent) data.Tr.parent = transform;
            data.OriginPos    = data.Tr.position;
            data.OriginQuat   = data.Tr.rotation;
            data.PrevQuat     = data.Tr.rotation;
            data.originDir    = (next.Tr.position - data.Tr.position).normalized;
            data.originLength = (next.Tr.position - data.Tr.position).magnitude;
            data.originLengthDiv = (1f / data.originLength);
            data.lengthRatio     = (data.originLength * _fullyExtendedDiv);
        }

        IsValid = true;
        #endregion
    }

    private Vector3 GetBezier(ref Vector3 s, ref Vector3 c, ref Vector3 d, float w = 0f)
    {
        #region Omit
        Vector3 sc = (c - s);
        Vector3 cd = (d - c);

        Vector3 a2 = s + (sc * w);
        Vector3 b2 = c + (cd * w);
        Vector3 c2 = (b2 - a2);

        return a2 + (c2 * w);
        #endregion
    }

    private Quaternion GetQuatBetweenVector(Vector3 from, Vector3 to, float ratio = 1f)
    {
        #region Omit

        /********************************************
         *   주어진 두 벡터사이의 쿼터니언 값을 계산한다...
         * ***/
        float angle   = Vector3.Angle(from, to) * ratio;
        Vector3 cross = Vector3.Cross(from, to);
        return Quaternion.AngleAxis(angle, cross);
        #endregion
    }

    private void UpdateLineRenderer()
    {
        #region Omit

        /***************************************************
         *   라인랜더러의 참조가 유효할 경우 위치를 갱신합니다.
         * ****/
        if (LineRenderer == null) return;

        LineRenderer.positionCount = _dataCount;

        for(int i=0; i<_dataCount; i++){

            ref BoneData data = ref _BoneData[i];
            LineRenderer.SetPosition(i, data.Tr.position);
        }

        #endregion
    }



    //============================================
    //////          Public methods           /////
    //============================================
    public void AddBone(Transform newBoneTr)
    {
        #region Omit
        if (newBoneTr == null) return;

        /**컨테이너가 유효하지 않다면 할당한다....*/
        if(_BoneData==null){

            _BoneData = new BoneData[10];
        }

        /**컨테이너의 공간이 부족하면 배로 할당한다....*/
        if(_BoneData.Length < _dataCount+1){

            BoneData[] newDatas = new BoneData[_dataCount*2];
            _BoneData.CopyTo(newDatas, 0);
            _BoneData = newDatas;
        }

        /*********************************************
         *   새로운 본을 삽입한다....
         * ****/
        ref BoneData newBone = ref _BoneData[_dataCount++];
        newBone.Tr = newBoneTr;

        IsValid = false;
        #endregion
    }

    public void RemoveBone(int removeBoneIndex)
    {
        #region Omit
        if (removeBoneIndex < 0 || removeBoneIndex >= _dataCount) 
                return;

        ref BoneData last   = ref _BoneData[_dataCount-1];
        ref BoneData remove = ref _BoneData[removeBoneIndex];
        remove = last;
        _dataCount--;

        IsValid = false;
        #endregion
    }

    public void RecalculateBoneDatas()
    {
        #region Omit

        Init(true);

        #endregion
    }

    public void UsePullableUpdate(bool apply=true)
    {
        ApplyUpdate = apply;
    }

    public void StretchFull()
    {
        #region Omit
        if (_dataCount < 2) return;

        ref BoneData rootBone    = ref _BoneData[0];
        ref BoneData rootDirBone = ref _BoneData[1];

        Vector3 rootDir = (rootDirBone.Tr.position - rootBone.Tr.position).normalized;
        for (int i = 1; i < _dataCount-1; i++)
        {
            if (_BoneData[i].Tr == null) continue;
            
            ref BoneData currBone = ref _BoneData[i];
            Quaternion   rotQut   = GetQuatBetweenVector(currBone.originDir, rootDir);
            currBone.Tr.rotation  = (rotQut * currBone.OriginQuat);
        }
        #endregion
    }

    public Vector3 GetBonePosition( int index )
    {
        #region Omit
        if (_dataCount==0 || _BoneData==null) return Vector3.zero;  

        index = Mathf.Clamp(index, 0, _dataCount - 1);
        return _BoneData[index].Tr.position;
        #endregion
    }

    public Transform GetBoneTransform( int index )
    {
        #region Omit
        if (_dataCount == 0 || _BoneData == null) return null;

        index = Mathf.Clamp(index, 0, _dataCount - 1);
        return _BoneData[index].Tr;
        #endregion
    }

    public Vector3 GetBoneDir(int index)
    {
        #region Omit
        if (_dataCount == 0 || _BoneData == null) return Vector3.zero;

        index = Mathf.Clamp(index, 0, _dataCount - 2);
        ref BoneData curr = ref _BoneData[index];
        ref BoneData next = ref _BoneData[index+1];

        if (curr.Tr == null || next.Tr == null) return Vector3.zero;
        return (next.Tr.position - curr.Tr.position).normalized;
        #endregion
    }

    public float GetBoneLength(int index)
    {
        #region Omit
        if (_dataCount == 0 || _BoneData == null) return 0f;

        return _BoneData[index].originLength;
        #endregion
    }

    public Vector3 GetNearBonePosition(Vector3 position)
    {
        #region Omit
        int   selectIdx = 0;
        float selectDst = float.MaxValue;

        /****************************************
         *   가장 근접한 본을 검색한다......
         * *****/
        for (int i = 0; i < _dataCount - 1; i++)
        {

            ref BoneData data = ref _BoneData[i];
            float dst = (position - data.Tr.position).sqrMagnitude;

            /**가장 근접한 인덱스를 선택한다...*/
            if (dst < selectDst)
            {
                selectIdx = i;
                selectDst = dst;
            }
        }

        /**가장 근접한 위치의 본을 반환한다...*/
        ref BoneData result = ref _BoneData[selectIdx];

        return result.Tr.position;
        #endregion
    }

    public Vector3 GetNearBoneDir(Vector3 position)
    {
        #region Omit
        /**본 정보가 유효하지 않다면 스킵한다....*/
        if (_dataCount < 2)
            return Vector3.zero; 

        int   selectIdx   = 0;
        float selectDst   = float.MaxValue;

        /****************************************
         *   가장 근접한 본을 검색한다......
         * *****/
        for (int i = 0; i < _dataCount - 1; i++){

            ref BoneData data = ref _BoneData[i];
            float dst         = (position - data.Tr.position).sqrMagnitude;

            /**가장 근접한 인덱스를 선택한다...*/
            if (dst < selectDst)
            {
                selectIdx = i;
                selectDst = dst;
            }
        }

        /**가장 근접한 위치의 본을 반환한다...*/
        ref BoneData result = ref _BoneData[selectIdx];
        ref BoneData resultDir = ref _BoneData[selectIdx + 1];

        return (resultDir.Tr.position - result.Tr.position).normalized;
        #endregion
    }

    public void GetNearBonePositionAndDir(Vector3 position, out Vector3 outPos, out Vector3 outDir)
    {
        #region Omit
        /**본 정보가 유효하지 않다면 스킵한다....*/
        if(_dataCount<2){

            outPos = outDir = Vector3.zero;
            return;
        }

        int   selectIdx = 0;
        float selectDst = float.MaxValue;

        /****************************************
         *   가장 근접한 본을 검색한다......
         * *****/
        for(int i=0; i<_dataCount-1; i++){

            ref BoneData data = ref _BoneData[i];
            float        dst  = (position - data.Tr.position).sqrMagnitude;

            /**가장 근접한 인덱스를 선택한다...*/
            if(dst < selectDst)
            {
                selectIdx = i;
                selectDst = dst;
            }
        }

        /**가장 근접한 위치의 본을 반환한다...*/
        ref BoneData result    = ref _BoneData[selectIdx];
        ref BoneData resultDir = ref _BoneData[selectIdx+1];

        outPos = result.Tr.position;
        outDir = (resultDir.Tr.position - outPos).normalized;
        #endregion
    }
}
