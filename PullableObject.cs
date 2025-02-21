using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

/***********************************************
 *   ����� �� �ִ� ȿ���� �����ϴ� ������Ʈ�Դϴ�...
 * **/
public sealed class PullableObject : MonoBehaviour
{
    #region Editor_Extension
    /****************************************
     *   ������ Ȯ���� ���� private class...
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

            /**������Ƽ�� ǥ���Ѵ�....*/
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

                /**���� ����Ǿ��ٸ� �����Ѵ�...*/
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
             *   ��� ������ ǥ���Ѵ�.
             * ***/
            BoneData[]  datas = targetObj._BoneData;
            int         Count = datas.Length;

            Handles.BeginGUI();
            {
                Vector3 prevPos = (datas[0].Tr!=null? HandleUtility.WorldToGUIPoint(datas[0].Tr.position):Vector3.zero);

                /**���� ������Ʈ�� �ִٸ� Ʈ�������� ������ �� �ֵ��� �Ѵ�...*/
                if (targetObj.HoldingPoint != null)
                {
                    Vector3 grabPos = targetObj.HoldingPoint.transform.position;
                    Vector3 grabGUIPos = HandleUtility.WorldToGUIPoint(grabPos);

                    if (GUI_ShowBoneButton(grabGUIPos, GrabNodeButtonStyle))
                    {
                        selectionData = targetObj.HoldingPoint.transform;
                    }
                }

                /**GUI�� ǥ���Ѵ�....*/
                for (int i = 0; i < Count; i++){

                    if (datas[i].Tr == null) continue;
                    Vector3 pos    = datas[i].Tr.position;
                    Vector3 guiPos = HandleUtility.WorldToGUIPoint(pos);

                    Handles.color = BONE_QUAD_COLOR;
                    Handles.DrawLine( prevPos, guiPos, 1f );

                    prevPos = guiPos;

                    /**�ش� GUI��ư�� ������ Ʈ�������� ������ �� �ֵ��� �Ѵ�..*/
                    if (GUI_ShowBoneButton(guiPos, NodeButtonStyle))
                    {
                        selectionData = datas[i].Tr;
                    }
                }

                /**���� ������Ʈ�� �ִٸ� Ʈ�������� ������ �� �ֵ��� �Ѵ�...*/
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
             *   ������ ���� �ִٸ� Ʈ�������� ������ �� �ֵ��� �Ѵ�...
             * ***/
            if(selectionData!=null){

                /*********************************************
                 *   �ش� ������Ʈ�� ��Ȱ��ȭ �Ǿ��ٸ� ��ŵ�Ѵ�...
                 * ***/
                Tools.current = Tool.None;
                using (var scope = new EditorGUI.ChangeCheckScope())
                {
                    Vector3     newPos  = selectionData.position;
                    Vector3     guiPos = HandleUtility.WorldToGUIPoint(newPos);
                    Quaternion  newQuat = selectionData.rotation;

                   /*********************************************
                    *   ������ �����Ѵ�...
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
                     *   ���� ���õ� ������ ���� Ʈ������ ������ �Ѵ�..
                     * ***/
                    switch (toolType){

                            /**�̵� ������ ���...*/
                            case (Tool.Move):
                            {
                                newPos = Handles.PositionHandle(selectionData.position, newQuat);
                                break;
                            }

                            /**ȸ�� ������ ���...*/
                            case (Tool.Rotate):
                            {
                                newQuat = Handles.RotationHandle(selectionData.rotation, newPos);
                                break;
                            }
                    }

                    /**���� �ٲ���ٸ� �����Ѵ�...*/
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
             *   ��� ������Ƽ���� ǥ���Ѵ�...
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
             *   ��� ������Ƽ�� ��Ÿ�ϵ��� �ʱ�ȭ�Ѵ�..
             * ***/

            /**��Ÿ�� �ʱ�ȭ....*/
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

            /**����ȭ ������Ƽ �ʱ�ȭ...*/
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

            /**��Ÿ�� ������̺� �ʱ�ȭ....*/
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

                /**���� �ٲ���ٸ� �����Ѵ�..*/
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

            /**�̵� ������ ���...*/
            if (toolType == Tool.Move){

                EditorGUI.Vector3Field(fieldRect, "", selectionData.position);
            }

            /**ȸ�� ������ ���...*/
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
             *   ��� ���� ȸ������ �ʱ�ȭ�Ѵ�.
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
                 *   ���������� �����ϴ� ��� ��� bone���� �߰��Ѵ�...
                 * ***/
                int index = 1;
                dataListProperty.arraySize = 1;


                Transform parent = targetObj._BoneData[0].Tr;
                Transform result;

                /**������ �ڽ� ������ �߰��Ѵ�....*/
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

            /**IK���� ����...*/
            ApplyUpdateProperty.boolValue = EditorGUILayout.Toggle("Apply Update", ApplyUpdateProperty.boolValue);


            /**********************************************************
             *   ��� ������ ��Ÿ���� GameObject�� �����ʵ带 ǥ���Ѵ�...
             * ****/
            EditorGUILayout.BeginHorizontal();
            {
                /**GrabTarget �����ʵ�...*/
                using (var scope = new EditorGUI.ChangeCheckScope()){

                    GameObject value = (GameObject)EditorGUILayout.ObjectField("Holding Point", GrabTargetProperty.objectReferenceValue, typeof(GameObject), true);
                    if (scope.changed) GrabTargetProperty.objectReferenceValue = value;
                }

            }
            EditorGUILayout.EndHorizontal();


            /***************************************************
             *   ���� �������� �����ʵ带 ǥ���Ѵ�....
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
             *   �븮�� ����� ǥ���Ѵ�..
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
             *   ���� ����� �븮�ڵ��� ǥ���Ѵ�...
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
             *   ���� ������ ������ ���� ������Ƽ���� ��� ǥ���Ѵ�...
             * ***/
            EditorGUILayout.BeginHorizontal();
            {
                /**������ ���� �����ϴ� ������Ƽ�� ǥ���Ѵ�...*/
                using (var scope = new EditorGUI.ChangeCheckScope()){

                    float value = EditorGUILayout.FloatField("Strech Vibration Pow", StrechVibePowProperty.floatValue);
                    if (scope.changed) 
                        StrechVibePowProperty.floatValue = Mathf.Clamp(value, 0f, float.MaxValue);
                }

                /**���� ���� ����ϴ����� ���� ���θ� �����Ѵ�....*/
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
             *   d������ �ִ���̸� ����ϴ����� ���� ������Ƽ�� ǥ���Ѵ�...
             * ***/
            EditorGUILayout.BeginHorizontal();
            {
                /**������ ���� �����ϴ� ������Ƽ�� ǥ���Ѵ�...*/
                using (var scope = new EditorGUI.ChangeCheckScope()){

                    float value = EditorGUILayout.FloatField("Fixed Max Length", FixedMaxLengthProperty.floatValue);
                    if (scope.changed)
                        FixedMaxLengthProperty.floatValue = Mathf.Clamp(value, 0f, float.MaxValue);
                }

                /**���� ���� ����ϴ����� ���� ���θ� �����Ѵ�....*/
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
             *   �ش� �÷��װ� ��ȿ�ϸ� �븮�ڸ� ǥ���Ѵ�.....
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
             *   ������ ��ư�� ǥ���Ѵ�....
             * ***/
            using (var scope = new EditorGUI.ChangeCheckScope()){

                GUI.backgroundColor = (eventIsUsed ? Color.red : Color.green);
                {
                    eventIsUsed = GUILayout.Toggle(eventIsUsed, eventName, toggleOptions[(int)style]);
                }
                GUI.backgroundColor = returnBgColor;

                /**���� �ٲ���� ��� �����Ѵ�...*/
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
                
                /**�� �ڽļ��� ���� ����� �߰��Ѵ�...*/
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
             *   �� ������ ������ ������ ���Ѵ�.
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

            /**�ִ���� �����ڰ� �ʱ�ȭ�����ʾҴٸ� �ʱ�ȭ.*/
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

                /**���� �ִ���̸� ����� ���...*/
                if(UseFixedMaxLength){

                    _fullyExtendedLen = 5f;
                    _fullyExtendedDiv = (1f / _fullyExtendedLen);
                }
            }
            else
            {
                OnPullRelease?.Invoke();

                /**���� �ִ���̸� ����� ���...*/
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
            
            /**���� �������� ��ȿ�� ��� ������ �����Ѵ�....*/
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


    /**���� �ݵ��� ���õ� �ʵ�...*/
    private float  _lastExtendedLen = 0f;
    private float  _Yspeed = 2f;
    private float  _boundTime  = 0f;


    /**���� �������� ���õ� �ʵ�...*/
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
         *   �ܺηκ��� ����� ����� ó���� �Ѵ�..
         * ***/
        if (HoldingPoint!=null){

            /**�����ϰ� ������� ���� ó��...*/
            if (!UpdateFullExtendedVibration())
            {
                /**������ ������� �ʾ��� ����� ó��...*/
                UpdateLookAtTarget(HoldingPoint.transform.position);
            }

            UpdateLineRenderer();
            OnLateUpdate?.Invoke();
            return;
        }


        /****************************************
         *   ������ٰ� �������� ���� ó���� �Ѵ�...
         * ***/

        /**�������� ���� ó��...*/
        if(!UpdateBreakRestore()){

            /**õõ�� ���󺹱͵Ǵ� ����� ó��...*/
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
         *   FABRIK �˰�������, ����� ����Ű���� �Ѵ�...
         * ***/

        /**������ ������ ��Ʈ������ ���ʴ�� Ʈ�������� �����Ѵ�...*/
        ApplyForwardIK(_dataCount - 2, targetPos);

        for (int i=_dataCount-3; i >= 0; i--){

            ref BoneData next = ref _BoneData[i + 1];
            ApplyForwardIK(i, next.Tr.position);
        }


        /****************************************************
         *   ��Ʈ���� ���� ��ġ�� �ִ��� ������ �̵��ϵ��� ����...
         * ***/
        ref BoneData rootBone     = ref _BoneData[0];
        Transform    targetTr     = HoldingPoint.transform;

        int   leftCount     = _fabrikLimit;
        float root2forward  = 2f;

        /**��Ʈ���� ��ġ�� �ִ��� ������ ��ġ��Ų��...*/
        while (leftCount-- > 0 && root2forward > .05f)
        {
            /**��Ʈ���� ���� ��ġ�� �ٿ����´�...*/
            rootBone.Tr.position = rootBone.OriginPos;

            for(int i=1; i<_dataCount-1; i++){

                ref BoneData prev = ref _BoneData[i-1];
                ApplyBackwardIK(i, prev.Tr.position);
            }

            root2forward = (rootBone.OriginPos - rootBone.Tr.position).sqrMagnitude;
        }

        LastBone2GrabSolver();

        /**������ ��ġ�� ����Ѵ�...*/
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
         *   BackwardIK�� ���� ���� �κп� ������ ���� ���� �ʴ�
         *   �������� �ذ��Ѵ�...
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
         *   ���� �Ѱ�ġ�� �Ѿ�ٸ� �ı��ȴ�...
         * ***/
        if(!IsBroken && extendedRatio>=MaxScale)
        {
            IsBroken = true;
            _GrabTarget = null;
            _brokenTime *= .5f;
            return true;
        }


        /****************************************
         *   ������ ������� ���� ó���� �Ѵ�...
         * ***/
        else if (extendedRatio>=1f)
        {
            /**���� ������� ������ ������ ���� ó��...*/
            if(_lastExtendedLen>0){

                _Yspeed = StrechVibePow * (!UseFixedVibe? (root2TargetLen - _lastExtendedLen): 1f);
               _lastExtendedLen = 0;
                OnFullyExtended?.Invoke();
            }


            /******************************************
             *   ��꿡 �ʿ��� ��ҵ��� ��� ���Ѵ�...
             * ***/

            /**��꿡 ������ ������ ������ ���Ѵ�...*/
            ref BoneData root    = ref _BoneData[0];
            ref BoneData last    = ref _BoneData[_dataCount-1];

            /**������� ������ �����͸� �̿��Ͽ� ������ ���������� ���Ѵ�...*/
            Vector3 forward       = (HoldingPoint.transform.position - root.OriginPos);
            Vector3 forwardNormal = forward.normalized;
            Vector3 right         = Vector3.Cross(Vector3.up, forwardNormal).normalized;
            Vector3 up            = Vector3.Cross(forwardNormal, right).normalized;

            Vector3 a  = root.OriginPos;
            Vector3 cp = root.OriginPos + (forward*.5f) + (up*_Yspeed);
            Vector3 b  = HoldingPoint.transform.position;



            /**********************************************
             *   ������ ��� ��������Ͽ� ���� ������Ʈ �Ѵ�....
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

            /**�ݵ��� ���� �����ϴ� ȿ���� �����Ѵ�...*/
            if((_boundTime-=Time.deltaTime)<=0f)
            {
                _Yspeed = (-_Yspeed * .7f);
                _boundTime = .05f;
            }

            return true;
        }

        /**������ ��ġ�� ����Ѵ�...*/
        else _lastExtendedLen = (HoldingPoint.transform.position - _BoneData[0].Tr.position).magnitude;

        return false;
        #endregion
    }

    private void UpdateExtendedRestore()
    {
        #region Omit

        /************************************
         *   �� ������ ���� ��ġ�� ���ư���...
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
         *   ������ �����ϸ鼭 ���� ���̰� �پ���...
         * ***/
        if (_brokenTime <= 0) return true;

        _brokenTime -= Time.deltaTime;
        float progressRatio = (_brokenTime * _brokenDiv);

        /**��� ������ ũ�⸦ ���δ�.....*/
        for( int i=0; i<_dataCount-1; i++ )
        {
            ref BoneData curr = ref _BoneData[i];
            ref BoneData next = ref _BoneData[i + 1];

            Vector3 currDir = (next.Tr.position - curr.Tr.position).normalized;
            next.Tr.position = curr.Tr.position + (currDir * curr.originLength * progressRatio);
        }

        /**��� �پ����� ����� ó���� �Ѵ�...*/
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

        /**�� ������ ������ ��ġ�� ����Ѵ�...*/
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
         *  �� ���� �ʱ�ȭ....
         * ***/
        if (_BoneData == null) {

            _BoneData = new BoneData[10];
        }

        _dataCount = _BoneData.Length;
        for (int i = 0; i < _dataCount - 1; i++){

            ref BoneData data = ref _BoneData[i];
            ref BoneData next = ref _BoneData[i + 1];

            /**������ �����ִٸ� ������...*/
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
         *   �־��� �� ���ͻ����� ���ʹϾ� ���� ����Ѵ�...
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
         *   ���η������� ������ ��ȿ�� ��� ��ġ�� �����մϴ�.
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

        /**�����̳ʰ� ��ȿ���� �ʴٸ� �Ҵ��Ѵ�....*/
        if(_BoneData==null){

            _BoneData = new BoneData[10];
        }

        /**�����̳��� ������ �����ϸ� ��� �Ҵ��Ѵ�....*/
        if(_BoneData.Length < _dataCount+1){

            BoneData[] newDatas = new BoneData[_dataCount*2];
            _BoneData.CopyTo(newDatas, 0);
            _BoneData = newDatas;
        }

        /*********************************************
         *   ���ο� ���� �����Ѵ�....
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
         *   ���� ������ ���� �˻��Ѵ�......
         * *****/
        for (int i = 0; i < _dataCount - 1; i++)
        {

            ref BoneData data = ref _BoneData[i];
            float dst = (position - data.Tr.position).sqrMagnitude;

            /**���� ������ �ε����� �����Ѵ�...*/
            if (dst < selectDst)
            {
                selectIdx = i;
                selectDst = dst;
            }
        }

        /**���� ������ ��ġ�� ���� ��ȯ�Ѵ�...*/
        ref BoneData result = ref _BoneData[selectIdx];

        return result.Tr.position;
        #endregion
    }

    public Vector3 GetNearBoneDir(Vector3 position)
    {
        #region Omit
        /**�� ������ ��ȿ���� �ʴٸ� ��ŵ�Ѵ�....*/
        if (_dataCount < 2)
            return Vector3.zero; 

        int   selectIdx   = 0;
        float selectDst   = float.MaxValue;

        /****************************************
         *   ���� ������ ���� �˻��Ѵ�......
         * *****/
        for (int i = 0; i < _dataCount - 1; i++){

            ref BoneData data = ref _BoneData[i];
            float dst         = (position - data.Tr.position).sqrMagnitude;

            /**���� ������ �ε����� �����Ѵ�...*/
            if (dst < selectDst)
            {
                selectIdx = i;
                selectDst = dst;
            }
        }

        /**���� ������ ��ġ�� ���� ��ȯ�Ѵ�...*/
        ref BoneData result = ref _BoneData[selectIdx];
        ref BoneData resultDir = ref _BoneData[selectIdx + 1];

        return (resultDir.Tr.position - result.Tr.position).normalized;
        #endregion
    }

    public void GetNearBonePositionAndDir(Vector3 position, out Vector3 outPos, out Vector3 outDir)
    {
        #region Omit
        /**�� ������ ��ȿ���� �ʴٸ� ��ŵ�Ѵ�....*/
        if(_dataCount<2){

            outPos = outDir = Vector3.zero;
            return;
        }

        int   selectIdx = 0;
        float selectDst = float.MaxValue;

        /****************************************
         *   ���� ������ ���� �˻��Ѵ�......
         * *****/
        for(int i=0; i<_dataCount-1; i++){

            ref BoneData data = ref _BoneData[i];
            float        dst  = (position - data.Tr.position).sqrMagnitude;

            /**���� ������ �ε����� �����Ѵ�...*/
            if(dst < selectDst)
            {
                selectIdx = i;
                selectDst = dst;
            }
        }

        /**���� ������ ��ġ�� ���� ��ȯ�Ѵ�...*/
        ref BoneData result    = ref _BoneData[selectIdx];
        ref BoneData resultDir = ref _BoneData[selectIdx+1];

        outPos = result.Tr.position;
        outDir = (resultDir.Tr.position - outPos).normalized;
        #endregion
    }
}
