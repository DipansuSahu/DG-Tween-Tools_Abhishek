using System.Collections.Generic;
using DG.Tweening;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace DGTweenTools.AbS.Editor
{
    [CustomEditor(typeof(DOTweenAnimator))]
    [CanEditMultipleObjects]
    public class DOTweenAnimatorEditor : UnityEditor.Editor
    {
        private SerializedProperty _playTrigger;
        private SerializedProperty _steps;
        private SerializedProperty _sequenceLoopMode;
        private SerializedProperty _sequenceLoopCount;
        private SerializedProperty _sequenceLoopType;
        private SerializedProperty _onStart;
        private SerializedProperty _onSequenceLoopComplete;
        private SerializedProperty _onComplete;
        private SerializedProperty _onKill;

        private ReorderableList _list;
        private readonly Dictionary<int, bool> _expanded = new Dictionary<int, bool>();

        private void OnEnable()
        {
            _playTrigger = serializedObject.FindProperty("playTrigger");
            _steps = serializedObject.FindProperty("steps");
            _sequenceLoopMode = serializedObject.FindProperty("sequenceLoopMode");
            _sequenceLoopCount = serializedObject.FindProperty("sequenceLoopCount");
            _sequenceLoopType = serializedObject.FindProperty("sequenceLoopType");
            _onStart = serializedObject.FindProperty("onStart");
            _onSequenceLoopComplete = serializedObject.FindProperty("onSequenceLoopComplete");
            _onComplete = serializedObject.FindProperty("onComplete");
            _onKill = serializedObject.FindProperty("onKill");

            _list = new ReorderableList(serializedObject, _steps, true, true, true, true)
            {
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Animation Chain (plays top to bottom)"),
                elementHeightCallback = ElementHeight,
                drawElementCallback = DrawElement,
                onAddCallback = list =>
                {
                    int newIndex = _steps.arraySize;
                    _steps.arraySize++;
                    var el = _steps.GetArrayElementAtIndex(newIndex);
                    el.FindPropertyRelative("stepName").stringValue = "Step " + (newIndex + 1);
                    // Reset move/rotate/scale enabled flags so new steps don't inherit the previous step's setup
                    el.FindPropertyRelative("move.enabled").boolValue = false;
                    el.FindPropertyRelative("rotate.enabled").boolValue = false;
                    el.FindPropertyRelative("scale.enabled").boolValue = false;
                    el.FindPropertyRelative("loopMode").enumValueIndex = 0;
                }
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_playTrigger);
            EditorGUILayout.Space(6);

            _list.DoLayoutList();

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Whole-Sequence Loop", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_sequenceLoopMode, new GUIContent("Loop"));
            var seqLoop = (DOTweenAnimator.LoopMode)_sequenceLoopMode.enumValueIndex;
            EditorGUI.indentLevel++;
            if (seqLoop == DOTweenAnimator.LoopMode.FixedCount)
                EditorGUILayout.PropertyField(_sequenceLoopCount, new GUIContent("Loop Count"));
            if (seqLoop != DOTweenAnimator.LoopMode.Once)
                EditorGUILayout.PropertyField(_sequenceLoopType, new GUIContent("Loop Type"));
            EditorGUI.indentLevel--;

            if (seqLoop != DOTweenAnimator.LoopMode.Once)
            {
                bool anyStepInfinite = false;
                for (int i = 0; i < _steps.arraySize; i++)
                {
                    var lm = (DOTweenAnimator.LoopMode)_steps.GetArrayElementAtIndex(i)
                        .FindPropertyRelative("loopMode").enumValueIndex;
                    if (lm == DOTweenAnimator.LoopMode.Infinite) anyStepInfinite = true;
                }
                if (anyStepInfinite)
                    EditorGUILayout.HelpBox(
                        "A step in the chain already loops Infinitely, so this whole-sequence loop will never be reached. Consider leaving it as \"Once\".",
                        MessageType.Warning);
            }

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Chain Events", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_onStart);
            EditorGUILayout.PropertyField(_onSequenceLoopComplete, new GUIContent("On Sequence Loop Complete"));
            EditorGUILayout.PropertyField(_onComplete);
            EditorGUILayout.PropertyField(_onKill);

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space(10);
            DrawRuntimeControls();
        }

        // ---------------------------------------------------------------
        // Reorderable list: element drawing
        // ---------------------------------------------------------------

        private float ElementHeight(int index)
        {
            float line = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            float h = line + spacing * 2; // header (foldout) row

            bool expanded = _expanded.TryGetValue(index, out var e) && e;
            if (!expanded) return h;

            var element = _steps.GetArrayElementAtIndex(index);

            h += line + spacing; // step name
            h += line + spacing; // playTogether

            h += StepAnimHeight(element.FindPropertyRelative("move"), true, false);
            h += StepAnimHeight(element.FindPropertyRelative("rotate"), false, true);
            h += StepAnimHeight(element.FindPropertyRelative("scale"), false, false);

            var loopMode = (DOTweenAnimator.LoopMode)element.FindPropertyRelative("loopMode").enumValueIndex;
            h += line + spacing; // loop mode row
            if (loopMode == DOTweenAnimator.LoopMode.FixedCount) h += line + spacing;
            if (loopMode != DOTweenAnimator.LoopMode.Once) h += line + spacing;

            h += EditorGUI.GetPropertyHeight(element.FindPropertyRelative("onStepStart")) + spacing;
            h += EditorGUI.GetPropertyHeight(element.FindPropertyRelative("onStepLoopComplete")) + spacing;
            h += EditorGUI.GetPropertyHeight(element.FindPropertyRelative("onStepComplete")) + spacing;

            return h + spacing * 2;
        }

        private float StepAnimHeight(SerializedProperty anim, bool isMove, bool isRotate)
        {
            float line = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            float h = line + spacing; // toggle header row

            if (!anim.FindPropertyRelative("enabled").boolValue) return h;

            // relative, value, duration, delay, useCustomCurve, curve-or-ease = 6 rows, plus optional isLocal / rotateMode
            int rows = 6 + (isMove ? 1 : 0) + (isRotate ? 1 : 0);
            h += rows * (line + spacing);
            return h;
        }

        private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = _steps.GetArrayElementAtIndex(index);
            var nameProp = element.FindPropertyRelative("stepName");
            float line = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;

            rect.y += spacing;
            bool expanded = _expanded.TryGetValue(index, out var e) && e;
            string label = string.IsNullOrEmpty(nameProp.stringValue) ? $"Step {index}" : $"{index}: {nameProp.stringValue}";

            var foldRect = new Rect(rect.x + 10, rect.y, rect.width - 10, line);
            bool newExpanded = EditorGUI.Foldout(foldRect, expanded, label, true);
            _expanded[index] = newExpanded;
            if (!newExpanded) return;

            float y = rect.y + line + spacing;

            EditorGUI.PropertyField(new Rect(rect.x, y, rect.width, line), nameProp, new GUIContent("Step Name"));
            y += line + spacing;

            EditorGUI.PropertyField(new Rect(rect.x, y, rect.width, line),
                element.FindPropertyRelative("playTogether"), new GUIContent("Play Together (Join)"));
            y += line + spacing;

            y = DrawAnim(rect, y, element.FindPropertyRelative("move"), "Move", true, false);
            y = DrawAnim(rect, y, element.FindPropertyRelative("rotate"), "Rotate", false, true);
            y = DrawAnim(rect, y, element.FindPropertyRelative("scale"), "Scale", false, false);

            var loopModeProp = element.FindPropertyRelative("loopMode");
            EditorGUI.PropertyField(new Rect(rect.x, y, rect.width, line), loopModeProp, new GUIContent("Step Loop"));
            y += line + spacing;

            var loopMode = (DOTweenAnimator.LoopMode)loopModeProp.enumValueIndex;
            if (loopMode == DOTweenAnimator.LoopMode.FixedCount)
            {
                EditorGUI.PropertyField(new Rect(rect.x, y, rect.width, line),
                    element.FindPropertyRelative("loopCount"), new GUIContent("Loop Count"));
                y += line + spacing;
            }
            if (loopMode != DOTweenAnimator.LoopMode.Once)
            {
                EditorGUI.PropertyField(new Rect(rect.x, y, rect.width, line),
                    element.FindPropertyRelative("loopType"), new GUIContent("Loop Type"));
                y += line + spacing;
            }

            var onStepStart = element.FindPropertyRelative("onStepStart");
            float hStart = EditorGUI.GetPropertyHeight(onStepStart);
            EditorGUI.PropertyField(new Rect(rect.x, y, rect.width, hStart), onStepStart);
            y += hStart + spacing;

            var onStepLoopComplete = element.FindPropertyRelative("onStepLoopComplete");
            float hLoop = EditorGUI.GetPropertyHeight(onStepLoopComplete);
            EditorGUI.PropertyField(new Rect(rect.x, y, rect.width, hLoop), onStepLoopComplete);
            y += hLoop + spacing;

            var onStepComplete = element.FindPropertyRelative("onStepComplete");
            float hComp = EditorGUI.GetPropertyHeight(onStepComplete);
            EditorGUI.PropertyField(new Rect(rect.x, y, rect.width, hComp), onStepComplete);
        }

        private float DrawAnim(Rect rect, float y, SerializedProperty anim, string label, bool isMove, bool isRotate)
        {
            float line = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;

            var enabledProp = anim.FindPropertyRelative("enabled");
            var toggleRect = new Rect(rect.x + 20, y, rect.width - 20, line);
            enabledProp.boolValue = EditorGUI.ToggleLeft(toggleRect, label, enabledProp.boolValue, EditorStyles.boldLabel);
            y += line + spacing;

            if (!enabledProp.boolValue) return y;

            EditorGUI.indentLevel++;

            if (isMove)
            {
                EditorGUI.PropertyField(new Rect(rect.x, y, rect.width, line),
                    anim.FindPropertyRelative("isLocal"), new GUIContent("Local Space"));
                y += line + spacing;
            }

            EditorGUI.PropertyField(new Rect(rect.x, y, rect.width, line), anim.FindPropertyRelative("relative"));
            y += line + spacing;

            EditorGUI.PropertyField(new Rect(rect.x, y, rect.width, line), anim.FindPropertyRelative("value"));
            y += line + spacing;

            EditorGUI.PropertyField(new Rect(rect.x, y, rect.width, line), anim.FindPropertyRelative("duration"));
            y += line + spacing;

            EditorGUI.PropertyField(new Rect(rect.x, y, rect.width, line), anim.FindPropertyRelative("delay"));
            y += line + spacing;

            if (isRotate)
            {
                EditorGUI.PropertyField(new Rect(rect.x, y, rect.width, line), anim.FindPropertyRelative("rotateMode"));
                y += line + spacing;
            }

            var useCurveProp = anim.FindPropertyRelative("useCustomCurve");
            EditorGUI.PropertyField(new Rect(rect.x, y, rect.width, line), useCurveProp, new GUIContent("Use Custom Curve"));
            y += line + spacing;

            if (useCurveProp.boolValue)
                EditorGUI.PropertyField(new Rect(rect.x, y, rect.width, line),
                    anim.FindPropertyRelative("customCurve"), new GUIContent("Curve"));
            else
                EditorGUI.PropertyField(new Rect(rect.x, y, rect.width, line), anim.FindPropertyRelative("ease"));
            y += line + spacing;

            EditorGUI.indentLevel--;
            return y;
        }

        // ---------------------------------------------------------------
        // Runtime controls
        // ---------------------------------------------------------------

        private void DrawRuntimeControls()
        {
            var animator = (DOTweenAnimator)target;
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
