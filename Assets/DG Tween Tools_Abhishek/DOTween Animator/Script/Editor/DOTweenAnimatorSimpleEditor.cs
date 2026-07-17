using UnityEditor;
using UnityEngine;

namespace DGTweenTools.AbS.Editor
{
    [CustomEditor(typeof(DOTweenAnimatorSimple))]
    [CanEditMultipleObjects]
    public class DOTweenAnimatorEditorSimple : UnityEditor.Editor
    {
        private SerializedProperty _playTrigger;
        private SerializedProperty _move;
        private SerializedProperty _rotate;
        private SerializedProperty _scale;
        private SerializedProperty _playTogether;
        private SerializedProperty _loopMode;
        private SerializedProperty _loopCount;
        private SerializedProperty _loopType;
        private SerializedProperty _onStart;
        private SerializedProperty _onLoopComplete;
        private SerializedProperty _onComplete;
        private SerializedProperty _onKill;

        private void OnEnable()
        {
            _playTrigger = serializedObject.FindProperty("playTrigger");
            _move = serializedObject.FindProperty("move");
            _rotate = serializedObject.FindProperty("rotate");
            _scale = serializedObject.FindProperty("scale");
            _playTogether = serializedObject.FindProperty("playTogether");
            _loopMode = serializedObject.FindProperty("loopMode");
            _loopCount = serializedObject.FindProperty("loopCount");
            _loopType = serializedObject.FindProperty("loopType");
            _onStart = serializedObject.FindProperty("onStart");
            _onLoopComplete = serializedObject.FindProperty("onLoopComplete");
            _onComplete = serializedObject.FindProperty("onComplete");
            _onKill = serializedObject.FindProperty("onKill");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_playTrigger);

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Animations", EditorStyles.boldLabel);
            DrawStep("Move", _move, drawIsLocal: true);
            DrawStep("Rotate", _rotate, drawRotateMode: true);
            DrawStep("Scale", _scale);

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Sequence & Loop", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_playTogether, new GUIContent("Play Together (Join)",
                "ON: all enabled animations run simultaneously.\nOFF: they run one after another (Move -> Rotate -> Scale)."));
            EditorGUILayout.PropertyField(_loopMode);

            var loopModeValue = (DOTweenAnimatorSimple.LoopMode)_loopMode.enumValueIndex;
            EditorGUI.indentLevel++;
            if (loopModeValue == DOTweenAnimatorSimple.LoopMode.FixedCount)
                EditorGUILayout.PropertyField(_loopCount, new GUIContent("Loop Count"));
            if (loopModeValue != DOTweenAnimatorSimple.LoopMode.Once)
                EditorGUILayout.PropertyField(_loopType);
            EditorGUI.indentLevel--;

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_onStart);
            EditorGUILayout.PropertyField(_onLoopComplete, new GUIContent("On Loop Complete",
                "Fires once every time a loop cycle finishes (also fires once for non-looping animations)."));
            EditorGUILayout.PropertyField(_onComplete);
            EditorGUILayout.PropertyField(_onKill);

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space(10);
            DrawRuntimeControls();
        }

        private void DrawStep(string label, SerializedProperty root, bool drawIsLocal = false, bool drawRotateMode = false)
        {
            var enabledProp = root.FindPropertyRelative("enabled");

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            enabledProp.boolValue = EditorGUILayout.ToggleLeft(label, enabledProp.boolValue, EditorStyles.boldLabel);

            if (enabledProp.boolValue)
            {
                EditorGUI.indentLevel++;

                if (drawIsLocal)
                    EditorGUILayout.PropertyField(root.FindPropertyRelative("isLocal"), new GUIContent("Local Space"));

                EditorGUILayout.PropertyField(root.FindPropertyRelative("relative"),
                    new GUIContent("Relative", "ON: value is added on top of the current transform value each Play().\nOFF: value is treated as the absolute target."));
                EditorGUILayout.PropertyField(root.FindPropertyRelative("value"));
                EditorGUILayout.PropertyField(root.FindPropertyRelative("duration"));
                EditorGUILayout.PropertyField(root.FindPropertyRelative("delay"));

                if (drawRotateMode)
                    EditorGUILayout.PropertyField(root.FindPropertyRelative("rotateMode"));

                var useCurve = root.FindPropertyRelative("useCustomCurve");
                EditorGUILayout.PropertyField(useCurve, new GUIContent("Use Custom Curve"));
                if (useCurve.boolValue)
                    EditorGUILayout.PropertyField(root.FindPropertyRelative("customCurve"), new GUIContent("Curve"));
                else
                    EditorGUILayout.PropertyField(root.FindPropertyRelative("ease"));

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawRuntimeControls()
        {
            var animator = (DOTweenAnimatorSimple)target;

            EditorGUILayout.LabelField("Runtime Controls", EditorStyles.boldLabel);

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to test Play / Pause / Stop / Restart here.", MessageType.Info);
                return;
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Play")) animator.Play();
            if (GUILayout.Button("Pause")) animator.Pause();
            if (GUILayout.Button("Resume")) animator.Resume();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Stop")) animator.Stop();
            if (GUILayout.Button("Restart")) animator.Restart();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Status", animator.IsPlaying ? "Playing" : "Idle / Stopped");
        }
    }
}
