using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace DGTweenTools.AbS
{
    /// <summary>
    /// Reusable, Inspector-driven DOTween animation component.
    /// Plays an ordered CHAIN of steps (e.g. Rotate once -> Scale x3 -> Move forever).
    /// Each step can move/rotate/scale (any combination, simultaneous or sequential
    /// within the step) and loop independently before the chain advances.
    /// Also supports looping the WHOLE chain, and exposes UnityEvents at both the
    /// step level and the chain level.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("DGTweenTools/DOTween Animator")]
    public class DOTweenAnimator : MonoBehaviour
    {
        // ---------------------------------------------------------------
        // Data types
        // ---------------------------------------------------------------

        public enum PlayTrigger { None, OnStart, OnEnable }
        public enum LoopMode { Once, FixedCount, Infinite }

        [Serializable]
        public class AnimStep
        {
            public bool enabled;
            public Vector3 value;
            [Min(0f)] public float duration = 1f;
            [Min(0f)] public float delay = 0f;

            public bool relative = true;

            public bool useCustomCurve = false;
            public Ease ease = Ease.OutQuad;
            public AnimationCurve customCurve = AnimationCurve.Linear(0, 0, 1, 1);
        }

        [Serializable]
        public class MoveStep : AnimStep
        {
            public bool isLocal = true;
        }

        [Serializable]
        public class RotateStep : AnimStep
        {
            public RotateMode rotateMode = RotateMode.Fast;
        }

        [Serializable]
        public class ScaleStep : AnimStep
        {
        }

        /// <summary>
        /// One "beat" in the animation chain. Only the LAST step should use Infinite
        /// looping - an infinite step anywhere else would block every step after it,
        /// since a Sequence can't move past a child that never finishes.
        /// </summary>
        [Serializable]
        public class AnimationStep
        {
            public string stepName = "Step";

            public MoveStep move = new MoveStep();
            public RotateStep rotate = new RotateStep();
            public ScaleStep scale = new ScaleStep { relative = false, value = Vector3.one };

            [Tooltip("ON: Move/Rotate/Scale within this step play simultaneously.\nOFF: they play one after another (Move -> Rotate -> Scale) within this step.")]
            public bool playTogether = true;

            public LoopMode loopMode = LoopMode.Once;
            [Min(1)] public int loopCount = 1;
            public LoopType loopType = LoopType.Restart;

            public UnityEvent onStepStart;
            public UnityEvent onStepLoopComplete;
            public UnityEvent onStepComplete;
        }

        // ---------------------------------------------------------------
        // Inspector fields
        // ---------------------------------------------------------------

        [Header("Play Trigger")]
        public PlayTrigger playTrigger = PlayTrigger.OnStart;

        [Header("Animation Chain")]
        [Tooltip("Steps play in order, top to bottom. Each step can loop independently before moving to the next.")]
        public List<AnimationStep> steps = new List<AnimationStep> { new AnimationStep() };

        [Header("Whole-Sequence Loop")]
        [Tooltip("Repeats the ENTIRE chain of steps. Leave as Once if a step in the chain (usually the last) already loops Infinitely.")]
        public LoopMode sequenceLoopMode = LoopMode.Once;
        [Min(1)] public int sequenceLoopCount = 1;
        public LoopType sequenceLoopType = LoopType.Restart;

        [Header("Chain Events")]
        public UnityEvent onStart;
        public UnityEvent onSequenceLoopComplete;
        public UnityEvent onComplete;
        public UnityEvent onKill;

        // ---------------------------------------------------------------
        // Runtime state
        // ---------------------------------------------------------------

        private Sequence _sequence;

        private Vector3 _initialPosition;
        private Vector3 _initialLocalPosition;
        private Vector3 _initialEulerAngles;
        private Vector3 _initialScale;

        public bool IsPlaying => _sequence != null && _sequence.IsActive() && _sequence.IsPlaying();

        // ---------------------------------------------------------------
        // Unity lifecycle
        // ---------------------------------------------------------------

        private void Awake() => CacheInitialTransform();

        private void OnEnable()
        {
            if (playTrigger == PlayTrigger.OnEnable) Play();
        }

        private void Start()
        {
            if (playTrigger == PlayTrigger.OnStart) Play();
        }

        private void OnDestroy() => KillSequence(false);

        // ---------------------------------------------------------------
        // Public API
        // ---------------------------------------------------------------

        [ContextMenu("Play")]
        public void Play()
        {
            KillSequence(false);

            if (steps == null || steps.Count == 0)
            {
                Debug.LogWarning($"[DOTweenAnimator] '{name}' has no steps configured.", this);
                return;
            }

            _sequence = DOTween.Sequence().SetTarget(transform);

            for (int i = 0; i < steps.Count; i++)
            {
                var step = steps[i];
                bool isLast = i == steps.Count - 1;

                if (step.loopMode == LoopMode.Infinite && !isLast)
                {
                    Debug.LogWarning($"[DOTweenAnimator] '{name}': step \"{step.stepName}\" (#{i}) loops Infinitely but isn't the last step. " +
                                      "Everything after it would never play, so the chain stops here.", this);
                    AppendStep(_sequence, step);
                    break;
                }

                AppendStep(_sequence, step);
            }

            switch (sequenceLoopMode)
            {
                case LoopMode.Once: _sequence.SetLoops(1, sequenceLoopType); break;
                case LoopMode.FixedCount: _sequence.SetLoops(Mathf.Max(1, sequenceLoopCount), sequenceLoopType); break;
                case LoopMode.Infinite: _sequence.SetLoops(-1, sequenceLoopType); break;
            }

            _sequence.OnStart(() => onStart?.Invoke());
            _sequence.OnStepComplete(() => onSequenceLoopComplete?.Invoke()); // fires once per whole-chain loop cycle
            _sequence.OnComplete(() => onComplete?.Invoke());
            _sequence.OnKill(() => onKill?.Invoke());

            _sequence.Play();
        }

        /// <summary>Snaps the transform back to its values from Awake, then plays the chain from the top.</summary>
        public void Restart()
        {
            ResetToInitial();
            Play();
        }

        public void Pause() => _sequence?.Pause();

        public void Resume() => _sequence?.Play();

        public void TogglePause()
        {
            if (_sequence == null) return;
            if (_sequence.IsPlaying()) _sequence.Pause();
            else _sequence.Play();
        }

        public void Stop(bool complete = false) => KillSequence(complete);

        public void ResetToInitial()
        {
            transform.position = _initialPosition;
            transform.localPosition = _initialLocalPosition;
            transform.eulerAngles = _initialEulerAngles;
            transform.localScale = _initialScale;
        }

        // ---------------------------------------------------------------
        // Internal helpers
        // ---------------------------------------------------------------

        private void AppendStep(Sequence master, AnimationStep step)
        {
            var tweens = new List<Tween>();
            if (step.move.enabled) tweens.Add(Configure(step.move, BuildMoveTweener(step.move)));
            if (step.rotate.enabled) tweens.Add(Configure(step.rotate, BuildRotateTweener(step.rotate)));
            if (step.scale.enabled) tweens.Add(Configure(step.scale, BuildScaleTweener(step.scale)));

            if (tweens.Count == 0)
            {
                Debug.LogWarning($"[DOTweenAnimator] '{name}': step \"{step.stepName}\" has no Move/Rotate/Scale enabled, skipping.", this);
                return;
            }

            var stepSequence = DOTween.Sequence();
            foreach (var t in tweens)
            {
                if (step.playTogether) stepSequence.Join(t);
                else stepSequence.Append(t);
            }

            switch (step.loopMode)
            {
                case LoopMode.Once: stepSequence.SetLoops(1, step.loopType); break;
                case LoopMode.FixedCount: stepSequence.SetLoops(Mathf.Max(1, step.loopCount), step.loopType); break;
                case LoopMode.Infinite: stepSequence.SetLoops(-1, step.loopType); break;
            }

            stepSequence.OnStart(() => step.onStepStart?.Invoke());
            stepSequence.OnStepComplete(() => step.onStepLoopComplete?.Invoke());
            stepSequence.OnComplete(() => step.onStepComplete?.Invoke());

            master.Append(stepSequence);
        }

        private void CacheInitialTransform()
        {
            _initialPosition = transform.position;
            _initialLocalPosition = transform.localPosition;
            _initialEulerAngles = transform.eulerAngles;
            _initialScale = transform.localScale;
        }

        private void KillSequence(bool complete)
        {
            if (_sequence != null && _sequence.IsActive())
                _sequence.Kill(complete);
            _sequence = null;
        }

        private Tweener BuildMoveTweener(MoveStep s) =>
            s.isLocal ? transform.DOLocalMove(s.value, s.duration) : transform.DOMove(s.value, s.duration);

        private Tweener BuildRotateTweener(RotateStep s) =>
            transform.DORotate(s.value, s.duration, s.rotateMode);

        private Tweener BuildScaleTweener(ScaleStep s) =>
            transform.DOScale(s.value, s.duration);

        private Tween Configure(AnimStep settings, Tweener tweener)
        {
            if (settings.relative) tweener.SetRelative(true);
            tweener.SetDelay(settings.delay);

            if (settings.useCustomCurve && settings.customCurve != null && settings.customCurve.length > 0)
                tweener.SetEase(settings.customCurve);
            else
                tweener.SetEase(settings.ease);

            return tweener;
        }
    }
}
