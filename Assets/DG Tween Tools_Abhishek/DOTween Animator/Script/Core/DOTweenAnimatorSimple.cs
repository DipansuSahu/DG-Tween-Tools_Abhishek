using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace DGTweenTools.AbS
{
    /// <summary>
    /// Reusable, Inspector-driven DOTween animation component.
    /// Supports Move / Rotate / Scale (any combination, played together or in sequence),
    /// configurable looping (once / fixed count / infinite), and UnityEvents for
    /// OnStart, OnLoopComplete, OnComplete and OnKill.
    /// Drop this on any GameObject - no code required unless you want to trigger
    /// Play()/Pause()/Stop()/Restart() manually from your own scripts.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("DGTweenTools/DOTween Animator Simple")]
    public class DOTweenAnimatorSimple : MonoBehaviour
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
            // scale defaults to absolute target since "relative scale" is rarely what people want
        }

        // ---------------------------------------------------------------
        // Inspector fields
        // ---------------------------------------------------------------

        [Header("Play Trigger")]
        public PlayTrigger playTrigger = PlayTrigger.OnStart;

        [Header("Animations")]
        public MoveStep move = new MoveStep();
        public RotateStep rotate = new RotateStep();
        public ScaleStep scale = new ScaleStep { relative = false, value = Vector3.one };

        [Header("Sequence")]
        [Tooltip("ON: Move/Rotate/Scale play simultaneously.\nOFF: they play one after another in the order Move -> Rotate -> Scale.")]
        public bool playTogether = true;

        [Header("Loop")]
        public LoopMode loopMode = LoopMode.Once;
        [Min(1)] public int loopCount = 1;
        public LoopType loopType = LoopType.Restart;

        [Header("Events")]
        public UnityEvent onStart;
        public UnityEvent onLoopComplete;
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

        private void Awake()
        {
            CacheInitialTransform();
        }

        private void OnEnable()
        {
            if (playTrigger == PlayTrigger.OnEnable) Play();
        }

        private void Start()
        {
            if (playTrigger == PlayTrigger.OnStart) Play();
        }

        private void OnDestroy()
        {
            KillSequence(false);
        }

        // ---------------------------------------------------------------
        // Public API
        // ---------------------------------------------------------------

        [ContextMenu("Play")]
        public void Play()
        {
            KillSequence(false);

            var steps = new List<Tween>();
            if (move.enabled) steps.Add(BuildTween(move, BuildMoveTweener(move)));
            if (rotate.enabled) steps.Add(BuildTween(rotate, BuildRotateTweener(rotate)));
            if (scale.enabled) steps.Add(BuildTween(scale, BuildScaleTweener(scale)));

            if (steps.Count == 0)
            {
                Debug.LogWarning($"[DOTweenAnimatorSimple] '{name}' has no animation enabled (Move/Rotate/Scale).", this);
                return;
            }

            _sequence = DOTween.Sequence().SetTarget(transform);
            foreach (var tween in steps)
            {
                if (playTogether) _sequence.Join(tween);
                else _sequence.Append(tween);
            }

            switch (loopMode)
            {
                case LoopMode.Once:
                    _sequence.SetLoops(1, loopType);
                    break;
                case LoopMode.FixedCount:
                    _sequence.SetLoops(Mathf.Max(1, loopCount), loopType);
                    break;
                case LoopMode.Infinite:
                    _sequence.SetLoops(-1, loopType);
                    break;
            }

            _sequence.OnStart(() => onStart?.Invoke());
            _sequence.OnStepComplete(() => onLoopComplete?.Invoke()); // fires once per loop cycle
            _sequence.OnComplete(() => onComplete?.Invoke());
            _sequence.OnKill(() => onKill?.Invoke());

            _sequence.Play();
        }

        /// <summary>Snaps the transform back to its values from Awake, then plays. Useful for repeat-triggered effects (e.g. button pulses).</summary>
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

        /// <summary>Kills the running tween. Pass true to also invoke OnComplete-style callbacks via DOTween's own complete-on-kill.</summary>
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

        private Tweener BuildMoveTweener(MoveStep s)
        {
            return s.isLocal
                ? transform.DOLocalMove(s.value, s.duration)
                : transform.DOMove(s.value, s.duration);
        }

        private Tweener BuildRotateTweener(RotateStep s)
        {
            return transform.DORotate(s.value, s.duration, s.rotateMode);
        }

        private Tweener BuildScaleTweener(ScaleStep s)
        {
            return transform.DOScale(s.value, s.duration);
        }

        private Tween BuildTween(AnimStep settings, Tweener tweener)
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
