using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace AbhishekSahu.DGTweening
{
    public class TextEffect
    {
        #region Animation Settings Class

        [System.Serializable]
        public class AnimationSettings
        {
            [Header("Scale Animation")]
            public float scaleFrom = 1f;

            public float scaleTo = 1.1f;
            public float scaleDuration = 0.2f;
            public Ease scaleEaseIn = Ease.OutQuad;
            public Ease scaleEaseOut = Ease.InQuad;

            [Header("Color Animation")]
            public Color defaultColor = Color.white;

            public Color animationColor = Color.white;
            public bool animateColor = false;
            public float colorDuration = 0.2f;
            public Ease colorEase = Ease.OutQuad;

            [Header("Position Animation")]
            public bool animatePosition = false;

            public Vector3 positionOffset = Vector3.zero;
            public float positionDuration = 0.2f;
            public Ease positionEase = Ease.OutQuad;

            [Header("Rotation Animation")]
            public bool animateRotation = false;

            public Vector3 rotationOffset = Vector3.zero;
            public float rotationDuration = 0.2f;
            public Ease rotationEase = Ease.OutQuad;

            [Header("Fade Animation")]
            public bool animateFade = false;

            public float fadeFrom = 1f;
            public float fadeTo = 0f;
            public float fadeDuration = 0.5f;
            public Ease fadeEase = Ease.OutQuad;

            [Header("General")]
            public bool loop = true;

            public float totalDuration = -1f; // -1 means infinite
            public float delayBefore = 0f;
            public float delayAfter = 0f;
            public bool yoyo = false;
            public int loopCount = -1; // -1 means infinite
        }

        #endregion Animation Settings Class

        #region Events

        public class AnimationEvents
        {
            public UnityEvent OnStart = new UnityEvent();
            public UnityEvent OnComplete = new UnityEvent();
            public UnityEvent OnLoopComplete = new UnityEvent();
            public UnityEvent OnPause = new UnityEvent();
            public UnityEvent OnResume = new UnityEvent();
            public UnityEvent OnKill = new UnityEvent();
            public UnityEvent OnUpdate = new UnityEvent();
            public UnityEvent<float> OnProgress = new UnityEvent<float>();
        }

        #endregion Events

        #region Private Fields

        private static Dictionary<TMP_Text, TextEffect> activeAnimators = new Dictionary<TMP_Text, TextEffect>();
        private static Dictionary<TMP_Text, Vector3> originalPositions = new Dictionary<TMP_Text, Vector3>();
        private static Dictionary<TMP_Text, Vector3> originalScales = new Dictionary<TMP_Text, Vector3>();
        private static Dictionary<TMP_Text, Color> originalColors = new Dictionary<TMP_Text, Color>();
        private static Dictionary<TMP_Text, Vector3> originalRotations = new Dictionary<TMP_Text, Vector3>();

        private TMP_Text targetText;
        private Sequence currentSequence;
        private AnimationSettings currentSettings;
        private AnimationEvents events = new AnimationEvents();
        private bool isProgressTracking = false;

        #endregion Private Fields

        #region Constructors

        public TextEffect()
        {
        }

        public TextEffect(TMP_Text text)
        {
            targetText = text;
        }

        #endregion Constructors

        #region Event Methods

        public TextEffect OnStart(UnityAction callback)
        {
            events.OnStart.AddListener(callback);
            return this;
        }

        public TextEffect OnComplete(UnityAction callback)
        {
            events.OnComplete.AddListener(callback);
            return this;
        }

        public TextEffect OnLoopComplete(UnityAction callback)
        {
            events.OnLoopComplete.AddListener(callback);
            return this;
        }

        public TextEffect OnPause(UnityAction callback)
        {
            events.OnPause.AddListener(callback);
            return this;
        }

        public TextEffect OnResume(UnityAction callback)
        {
            events.OnResume.AddListener(callback);
            return this;
        }

        public TextEffect OnKill(UnityAction callback)
        {
            events.OnKill.AddListener(callback);
            return this;
        }

        public TextEffect OnUpdate(UnityAction callback)
        {
            events.OnUpdate.AddListener(callback);
            return this;
        }

        public TextEffect OnProgress(UnityAction<float> callback)
        {
            events.OnProgress.AddListener(callback);
            isProgressTracking = true;
            return this;
        }

        #endregion Event Methods

        #region Basic Animation Methods

        /// <summary>
        /// Start animation with default settings
        /// </summary>
        public TextEffect StartAnimation(TMP_Text text)
        {
            return StartAnimation(text, new AnimationSettings());
        }

        /// <summary>
        /// Start animation with duration
        /// </summary>
        public TextEffect StartAnimation(TMP_Text text, float duration)
        {
            var settings = new AnimationSettings { scaleDuration = duration };
            return StartAnimation(text, settings);
        }

        /// <summary>
        /// Start animation with duration and color
        /// </summary>
        public TextEffect StartAnimation(TMP_Text text, float duration, Color color)
        {
            var settings = new AnimationSettings
            {
                scaleDuration = duration,
                animateColor = true,
                animationColor = color,
                colorDuration = duration
            };
            return StartAnimation(text, settings);
        }

        /// <summary>
        /// Start animation with duration, color and loop
        /// </summary>
        public TextEffect StartAnimation(TMP_Text text, float duration, Color color, bool loop)
        {
            var settings = new AnimationSettings
            {
                scaleDuration = duration,
                animateColor = true,
                animationColor = color,
                colorDuration = duration,
                loop = loop
            };
            return StartAnimation(text, settings);
        }

        /// <summary>
        /// Start animation with scale values
        /// </summary>
        public TextEffect StartAnimation(TMP_Text text, float duration, float scaleFrom, float scaleTo)
        {
            var settings = new AnimationSettings
            {
                scaleDuration = duration,
                scaleFrom = scaleFrom,
                scaleTo = scaleTo
            };
            return StartAnimation(text, settings);
        }

        /// <summary>
        /// Start animation with full settings
        /// </summary>
        public TextEffect StartAnimation(TMP_Text text, AnimationSettings settings)
        {
            if (text == null) return this;

            targetText = text;
            currentSettings = settings;

            // Stop any existing animation for this text
            StopAnimation(text);

            // Store original values
            StoreOriginalValues(text);

            // Set initial color if animating color
            if (settings.animateColor)
            {
                text.color = settings.animationColor;
            }

            // Create and start animation sequence
            currentSequence = CreateAnimationSequence(text, settings);
            activeAnimators[text] = this;

            // Handle duration-based stopping
            if (settings.totalDuration > 0)
            {
                TextCoroutineRunner.Instance.StartCoroutine(StopAnimationAfterDuration(text, settings.totalDuration));
            }

            // Start progress tracking if needed
            if (isProgressTracking)
            {
                TextCoroutineRunner.Instance.StartCoroutine(TrackProgress());
            }

            return this;
        }

        #endregion Basic Animation Methods

        #region Enhanced Animation Methods

        /// <summary>
        /// Rainbow color animation
        /// </summary>
        public static TextEffect Rainbow(TMP_Text text, float duration = 2f, bool loop = true)
        {
            var animator = new TextEffect();
            animator.targetText = text;
            StoreOriginalValues(text);

            var sequence = DOTween.Sequence();

            Color[] rainbowColors = {
            Color.red, new Color(1f, 0.5f, 0f), Color.yellow,
            Color.green, Color.blue, new Color(0.5f, 0f, 1f),
            new Color(1f, 0f, 1f)
        };

            float colorDuration = duration / rainbowColors.Length;

            foreach (var color in rainbowColors)
            {
                sequence.Append(text.DOColor(color, colorDuration));
            }

            if (loop)
            {
                sequence.SetLoops(-1);
            }

            animator.currentSequence = sequence;
            activeAnimators[text] = animator;

            return animator;
        }

        /// <summary>
        /// Typewriter effect
        /// </summary>
        public static TextEffect Typewriter(TMP_Text text, float charDelay = 0.05f)
        {
            var animator = new TextEffect();
            animator.targetText = text;
            StoreOriginalValues(text);

            string originalText = text.text;
            text.text = "";

            var sequence = DOTween.Sequence();

            for (int i = 0; i <= originalText.Length; i++)
            {
                int index = i; // Capture for closure
                sequence.AppendCallback(() =>
                {
                    text.text = originalText.Substring(0, index);
                });

                if (i < originalText.Length)
                {
                    sequence.AppendInterval(charDelay);
                }
            }

            animator.currentSequence = sequence;
            activeAnimators[text] = animator;

            return animator;
        }

        /// <summary>
        /// Elastic scale animation
        /// </summary>
        public static TextEffect ElasticScale(TMP_Text text, float scaleTo = 1.2f, float duration = 0.8f)
        {
            var settings = new AnimationSettings
            {
                scaleTo = scaleTo,
                scaleDuration = duration,
                scaleEaseIn = Ease.OutElastic,
                scaleEaseOut = Ease.InElastic,
                loop = false
            };
            return new TextEffect().StartAnimation(text, settings);
        }

        /// <summary>
        /// Slide in from direction
        /// </summary>
        public static TextEffect SlideIn(TMP_Text text, Vector3 direction, float duration = 0.5f, Ease ease = Ease.OutQuad)
        {
            var animator = new TextEffect();
            animator.targetText = text;
            StoreOriginalValues(text);

            Vector3 startPos = originalPositions[text] + direction;
            text.transform.localPosition = startPos;

            var sequence = DOTween.Sequence();
            sequence.Append(text.transform.DOLocalMove(originalPositions[text], duration).SetEase(ease));

            animator.currentSequence = sequence;
            activeAnimators[text] = animator;

            return animator;
        }

        /// <summary>
        /// Slide out to direction
        /// </summary>
        public static TextEffect SlideOut(TMP_Text text, Vector3 direction, float duration = 0.5f, Ease ease = Ease.InQuad)
        {
            var animator = new TextEffect();
            animator.targetText = text;
            StoreOriginalValues(text);

            Vector3 endPos = originalPositions[text] + direction;

            var sequence = DOTween.Sequence();
            sequence.Append(text.transform.DOLocalMove(endPos, duration).SetEase(ease));
            sequence.AppendCallback(() => RestoreOriginalValues(text));

            animator.currentSequence = sequence;
            activeAnimators[text] = animator;

            return animator;
        }

        /// <summary>
        /// Glow effect using shadow
        /// </summary>
        public static TextEffect Glow(TMP_Text text, Color glowColor, float intensity = 2f, float duration = 1f)
        {
            var animator = new TextEffect();
            animator.targetText = text;
            StoreOriginalValues(text);

            // This requires the text to have a shadow/outline component
            var sequence = DOTween.Sequence();

            // Animate the material properties for glow effect
            if (text.fontMaterial != null)
            {
                Material mat = text.fontMaterial;
                if (mat.HasProperty("_GlowPower"))
                {
                    sequence.Append(mat.DOFloat(intensity, "_GlowPower", duration));
                    sequence.Append(mat.DOFloat(0f, "_GlowPower", duration));
                    sequence.SetLoops(-1);
                }
            }

            animator.currentSequence = sequence;
            activeAnimators[text] = animator;

            return animator;
        }

        /// <summary>
        /// Wobble animation
        /// </summary>
        public static TextEffect Wobble(TMP_Text text, float intensity = 10f, float duration = 0.5f)
        {
            var animator = new TextEffect();
            animator.targetText = text;
            StoreOriginalValues(text);

            var sequence = DOTween.Sequence();

            // Rotate wobble
            sequence.Append(text.transform.DORotate(new Vector3(0, 0, intensity), duration * 0.25f).SetEase(Ease.OutQuad));
            sequence.Append(text.transform.DORotate(new Vector3(0, 0, -intensity), duration * 0.25f).SetEase(Ease.InOutQuad));
            sequence.Append(text.transform.DORotate(new Vector3(0, 0, intensity * 0.5f), duration * 0.25f).SetEase(Ease.InOutQuad));
            sequence.Append(text.transform.DORotate(Vector3.zero, duration * 0.25f).SetEase(Ease.InQuad));

            animator.currentSequence = sequence;
            activeAnimators[text] = animator;

            return animator;
        }

        /// <summary>
        /// Flip animation
        /// </summary>
        public static TextEffect Flip(TMP_Text text, bool horizontal = true, float duration = 0.5f)
        {
            var animator = new TextEffect();
            animator.targetText = text;
            StoreOriginalValues(text);

            var sequence = DOTween.Sequence();

            Vector3 flipAxis = horizontal ? new Vector3(0, 180, 0) : new Vector3(180, 0, 0);

            sequence.Append(text.transform.DORotate(flipAxis, duration * 0.5f).SetEase(Ease.InQuad));
            sequence.Append(text.transform.DORotate(Vector3.zero, duration * 0.5f).SetEase(Ease.OutQuad));

            animator.currentSequence = sequence;
            activeAnimators[text] = animator;

            return animator;
        }

        /// <summary>
        /// Breathing animation (subtle scale)
        /// </summary>
        public static TextEffect Breathe(TMP_Text text, float scaleAmount = 0.05f, float duration = 2f)
        {
            var settings = new AnimationSettings
            {
                scaleFrom = 1f,
                scaleTo = 1f + scaleAmount,
                scaleDuration = duration,
                scaleEaseIn = Ease.InOutSine,
                scaleEaseOut = Ease.InOutSine,
                loop = true
            };
            return new TextEffect().StartAnimation(text, settings);
        }

        /// <summary>
        /// Pop animation (quick scale up and down)
        /// </summary>
        public static TextEffect Pop(TMP_Text text, float scaleTo = 1.3f, float duration = 0.3f)
        {
            var animator = new TextEffect();
            animator.targetText = text;
            StoreOriginalValues(text);

            var sequence = DOTween.Sequence();

            sequence.Append(text.transform.DOScale(originalScales[text] * scaleTo, duration * 0.5f).SetEase(Ease.OutBack));
            sequence.Append(text.transform.DOScale(originalScales[text], duration * 0.5f).SetEase(Ease.InBack));

            animator.currentSequence = sequence;
            activeAnimators[text] = animator;

            return animator;
        }

        /// <summary>
        /// Gradient color animation
        /// </summary>
        public static TextEffect GradientColor(TMP_Text text, Color startColor, Color endColor, float duration = 1f, bool loop = true)
        {
            var animator = new TextEffect();
            animator.targetText = text;
            StoreOriginalValues(text);

            var sequence = DOTween.Sequence();

            sequence.Append(text.DOColor(startColor, 0f));
            sequence.Append(text.DOColor(endColor, duration));

            if (loop)
            {
                sequence.Append(text.DOColor(startColor, duration));
                sequence.SetLoops(-1);
            }

            animator.currentSequence = sequence;
            activeAnimators[text] = animator;

            return animator;
        }

        #endregion Enhanced Animation Methods

        #region Control Methods

        /// <summary>
        /// Pause the current animation
        /// </summary>
        public TextEffect Pause()
        {
            currentSequence?.Pause();
            events.OnPause?.Invoke();
            return this;
        }

        /// <summary>
        /// Resume the paused animation
        /// </summary>
        public TextEffect Resume()
        {
            currentSequence?.Play();
            events.OnResume?.Invoke();
            return this;
        }

        /// <summary>
        /// Restart the animation from beginning
        /// </summary>
        public TextEffect Restart()
        {
            currentSequence?.Restart();
            return this;
        }

        /// <summary>
        /// Stop the animation and restore original values
        /// </summary>
        public TextEffect Stop()
        {
            if (targetText != null)
            {
                StopAnimation(targetText);
            }
            return this;
        }

        /// <summary>
        /// Kill the animation without restoring values
        /// </summary>
        public TextEffect Kill()
        {
            currentSequence?.Kill();
            events.OnKill?.Invoke();
            if (targetText != null && activeAnimators.ContainsKey(targetText))
            {
                activeAnimators.Remove(targetText);
            }
            return this;
        }

        /// <summary>
        /// Toggle animation on/off
        /// </summary>
        public TextEffect Toggle(bool show)
        {
            if (show)
            {
                Resume();
            }
            else
            {
                Pause();
            }
            return this;
        }

        /// <summary>
        /// Set animation speed
        /// </summary>
        public TextEffect SetSpeed(float speed)
        {
            if (currentSequence != null)
            {
                currentSequence.timeScale = speed;
            }
            return this;
        }

        /// <summary>
        /// Set animation delay
        /// </summary>
        public TextEffect SetDelay(float delay)
        {
            if (currentSequence != null)
            {
                currentSequence.SetDelay(delay);
            }
            return this;
        }

        #endregion Control Methods

        #region Status Methods

        /// <summary>
        /// Check if animation is currently playing
        /// </summary>
        public bool IsPlaying()
        {
            return currentSequence != null && currentSequence.IsPlaying();
        }

        /// <summary>
        /// Check if animation is active (playing or paused)
        /// </summary>
        public bool IsActive()
        {
            return currentSequence != null && currentSequence.IsActive();
        }

        /// <summary>
        /// Get current animation progress (0-1)
        /// </summary>
        public float GetProgress()
        {
            if (currentSequence == null) return 0f;
            return currentSequence.ElapsedPercentage();
        }

        /// <summary>
        /// Get current animation time
        /// </summary>
        public float GetElapsedTime()
        {
            if (currentSequence == null) return 0f;
            return currentSequence.Elapsed();
        }

        /// <summary>
        /// Get animation duration
        /// </summary>
        public float GetDuration()
        {
            if (currentSequence == null) return 0f;
            return currentSequence.Duration();
        }

        #endregion Status Methods

        #region Static Utility Methods

        /// <summary>
        /// Quick scale animation
        /// </summary>
        public static TextEffect Scale(TMP_Text text, float scaleTo = 1.1f, float duration = 0.2f)
        {
            return new TextEffect().StartAnimation(text, duration, 1f, scaleTo);
        }

        /// <summary>
        /// Quick pulse animation
        /// </summary>
        public static TextEffect Pulse(TMP_Text text, Color color, float scaleTo = 1.05f, float duration = 0.3f)
        {
            return new TextEffect().StartAnimation(text, duration, color, true);
        }

        /// <summary>
        /// Quick bounce animation
        /// </summary>
        public static TextEffect Bounce(TMP_Text text, float scaleTo = 1.2f, Vector3 positionOffset = default)
        {
            var settings = new AnimationSettings
            {
                scaleTo = scaleTo,
                animatePosition = positionOffset != Vector3.zero,
                positionOffset = positionOffset,
                scaleEaseIn = Ease.OutBounce,
                scaleEaseOut = Ease.InBounce
            };
            return new TextEffect().StartAnimation(text, settings);
        }

        /// <summary>
        /// Quick shake animation
        /// </summary>
        public static TextEffect Shake(TMP_Text text, float intensity = 5f, float duration = 0.5f)
        {
            var settings = new AnimationSettings
            {
                animatePosition = true,
                positionOffset = new Vector3(intensity, intensity, 0),
                positionDuration = duration,
                positionEase = Ease.InOutElastic,
                loop = false,
                totalDuration = duration
            };
            return new TextEffect().StartAnimation(text, settings);
        }

        /// <summary>
        /// Quick fade animation
        /// </summary>
        public static TextEffect Fade(TMP_Text text, Color fromColor, Color toColor, float duration = 0.5f)
        {
            var settings = new AnimationSettings
            {
                animateColor = true,
                defaultColor = fromColor,
                animationColor = toColor,
                colorDuration = duration,
                loop = false
            };
            return new TextEffect().StartAnimation(text, settings);
        }

        /// <summary>
        /// Fade in animation
        /// </summary>
        public static TextEffect FadeIn(TMP_Text text, float duration = 0.5f)
        {
            var settings = new AnimationSettings
            {
                animateFade = true,
                fadeFrom = 0f,
                fadeTo = 1f,
                fadeDuration = duration,
                loop = false
            };
            return new TextEffect().StartAnimation(text, settings);
        }

        /// <summary>
        /// Fade out animation
        /// </summary>
        public static TextEffect FadeOut(TMP_Text text, float duration = 0.5f)
        {
            var settings = new AnimationSettings
            {
                animateFade = true,
                fadeFrom = 1f,
                fadeTo = 0f,
                fadeDuration = duration,
                loop = false
            };
            return new TextEffect().StartAnimation(text, settings);
        }

        /// <summary>
        /// Stop all animations globally
        /// </summary>
        public static void StopAllAnimations()
        {
            var textsToStop = new List<TMP_Text>(activeAnimators.Keys);
            foreach (var text in textsToStop)
            {
                StopAnimation(text);
            }
        }

        /// <summary>
        /// Check if any text is being animated
        /// </summary>
        public static bool IsAnimating(TMP_Text text)
        {
            return activeAnimators.ContainsKey(text) && activeAnimators[text].IsActive();
        }

        /// <summary>
        /// Get animator for specific text
        /// </summary>
        public static TextEffect GetAnimator(TMP_Text text)
        {
            return activeAnimators.ContainsKey(text) ? activeAnimators[text] : null;
        }

        #endregion Static Utility Methods

        #region Private Methods

        private static void StoreOriginalValues(TMP_Text text)
        {
            if (!originalPositions.ContainsKey(text))
                originalPositions[text] = text.transform.localPosition;

            if (!originalScales.ContainsKey(text))
                originalScales[text] = text.transform.localScale;

            if (!originalColors.ContainsKey(text))
                originalColors[text] = text.color;

            if (!originalRotations.ContainsKey(text))
                originalRotations[text] = text.transform.localEulerAngles;
        }

        private static void RestoreOriginalValues(TMP_Text text)
        {
            if (originalPositions.ContainsKey(text))
                text.transform.localPosition = originalPositions[text];

            if (originalScales.ContainsKey(text))
                text.transform.localScale = originalScales[text];

            if (originalColors.ContainsKey(text))
                text.color = originalColors[text];

            if (originalRotations.ContainsKey(text))
                text.transform.localEulerAngles = originalRotations[text];
        }

        private static void StopAnimation(TMP_Text text)
        {
            if (text == null) return;

            if (activeAnimators.ContainsKey(text))
            {
                activeAnimators[text].currentSequence?.Kill();
                activeAnimators[text].events.OnComplete?.Invoke();
                activeAnimators.Remove(text);
            }

            RestoreOriginalValues(text);
        }

        private Sequence CreateAnimationSequence(TMP_Text text, AnimationSettings settings)
        {
            Sequence sequence = DOTween.Sequence();

            // Add initial delay
            if (settings.delayBefore > 0)
            {
                sequence.AppendInterval(settings.delayBefore);
            }

            // Reset to original values
            text.transform.localScale = originalScales[text];
            text.transform.localPosition = originalPositions[text];
            text.transform.localEulerAngles = originalRotations[text];

            // Scale animation
            var scaleSequence = DOTween.Sequence();
            scaleSequence.Append(text.transform.DOScale(originalScales[text] * settings.scaleTo, settings.scaleDuration).SetEase(settings.scaleEaseIn));
            scaleSequence.Append(text.transform.DOScale(originalScales[text] * settings.scaleFrom, settings.scaleDuration).SetEase(settings.scaleEaseOut));

            // Position animation (if enabled)
            if (settings.animatePosition)
            {
                var positionSequence = DOTween.Sequence();
                Vector3 targetPos = originalPositions[text] + settings.positionOffset;
                positionSequence.Append(text.transform.DOLocalMove(targetPos, settings.positionDuration).SetEase(settings.positionEase));
                positionSequence.Append(text.transform.DOLocalMove(originalPositions[text], settings.positionDuration).SetEase(settings.positionEase));

                sequence.Insert(0, positionSequence);
            }

            // Rotation animation (if enabled)
            if (settings.animateRotation)
            {
                var rotationSequence = DOTween.Sequence();
                Vector3 targetRot = originalRotations[text] + settings.rotationOffset;
                rotationSequence.Append(text.transform.DOLocalRotate(targetRot, settings.rotationDuration).SetEase(settings.rotationEase));
                rotationSequence.Append(text.transform.DOLocalRotate(originalRotations[text], settings.rotationDuration).SetEase(settings.rotationEase));

                sequence.Insert(0, rotationSequence);
            }

            // Color animation (if enabled)
            if (settings.animateColor)
            {
                var colorSequence = DOTween.Sequence();
                colorSequence.Append(text.DOColor(settings.animationColor, settings.colorDuration).SetEase(settings.colorEase));
                colorSequence.Append(text.DOColor(settings.defaultColor, settings.colorDuration).SetEase(settings.colorEase));

                sequence.Insert(0, colorSequence);
            }

            // Fade animation (if enabled)
            if (settings.animateFade)
            {
                var fadeSequence = DOTween.Sequence();
                Color currentColor = text.color;
                Color fadeFromColor = new Color(currentColor.r, currentColor.g, currentColor.b, settings.fadeFrom);
                Color fadeToColor = new Color(currentColor.r, currentColor.g, currentColor.b, settings.fadeTo);

                fadeSequence.Append(text.DOColor(fadeFromColor, 0f));
                fadeSequence.Append(text.DOColor(fadeToColor, settings.fadeDuration).SetEase(settings.fadeEase));

                sequence.Insert(0, fadeSequence);
            }

            sequence.Insert(0, scaleSequence);

            // Add final delay
            if (settings.delayAfter > 0)
            {
                sequence.AppendInterval(settings.delayAfter);
            }

            // Set loop settings
            if (settings.loop)
            {
                sequence.SetLoops(settings.loopCount, settings.yoyo ? LoopType.Yoyo : LoopType.Restart);
            }

            // Add callbacks
            sequence.OnStart(() => events.OnStart?.Invoke());
            sequence.OnComplete(() => events.OnComplete?.Invoke());
            sequence.OnStepComplete(() => events.OnLoopComplete?.Invoke());
            sequence.OnUpdate(() => events.OnUpdate?.Invoke());

            return sequence;
        }

        private System.Collections.IEnumerator StopAnimationAfterDuration(TMP_Text text, float duration)
        {
            yield return new WaitForSeconds(duration);
            StopAnimation(text);
        }

        private System.Collections.IEnumerator TrackProgress()
        {
            while (currentSequence != null && currentSequence.IsActive())
            {
                events.OnProgress?.Invoke(GetProgress());
                yield return null;
            }
        }

        #endregion Private Methods

        #region Chain Methods for Fluent Interface

        /// <summary>
        /// Chain multiple animations together
        /// </summary>
        public TextEffect Then(System.Action<TextEffect> nextAnimation)
        {
            if (currentSequence != null)
            {
                currentSequence.OnComplete(() => nextAnimation?.Invoke(this));
            }
            return this;
        }

        /// <summary>
        /// Wait for specified duration before next animation
        /// </summary>
        public TextEffect Wait(float duration)
        {
            if (currentSequence != null)
            {
                currentSequence.AppendInterval(duration);
            }
            return this;
        }

        /// <summary>
        /// Set custom ease for next animation
        /// </summary>
        public TextEffect SetEase(Ease ease)
        {
            if (currentSequence != null)
            {
                currentSequence.SetEase(ease);
            }
            return this;
        }

        /// <summary>
        /// Set custom ease curve for next animation
        /// </summary>
        public TextEffect SetEase(AnimationCurve curve)
        {
            if (currentSequence != null)
            {
                currentSequence.SetEase(curve);
            }
            return this;
        }

        /// <summary>
        /// Set loops for current animation
        /// </summary>
        public TextEffect SetLoops(int loops, LoopType loopType = LoopType.Restart)
        {
            if (currentSequence != null)
            {
                currentSequence.SetLoops(loops, loopType);
            }
            return this;
        }

        /// <summary>
        /// Set infinite loops for current animation
        /// </summary>
        public TextEffect SetLoopsInfinite(LoopType loopType = LoopType.Restart)
        {
            if (currentSequence != null)
            {
                currentSequence.SetLoops(-1, loopType);
            }
            return this;
        }

        #endregion Chain Methods for Fluent Interface

        #region Advanced Animation Methods

        /// <summary>
        /// Character-by-character wave animation
        /// </summary>
        public static TextEffect WaveText(TMP_Text text, float amplitude = 10f, float frequency = 1f, float duration = 2f)
        {
            var animator = new TextEffect();
            animator.targetText = text;
            StoreOriginalValues(text);

            // This would require mesh manipulation for proper character-by-character animation
            // For now, we'll do a simpler wave-like position animation
            var sequence = DOTween.Sequence();

            Vector3 originalPos = text.transform.localPosition;

            for (int i = 0; i < 10; i++)
            {
                float offset = Mathf.Sin(i * frequency) * amplitude;
                Vector3 wavePos = originalPos + new Vector3(0, offset, 0);
                sequence.Append(text.transform.DOLocalMove(wavePos, duration / 10f));
            }

            sequence.SetLoops(-1);

            animator.currentSequence = sequence;
            activeAnimators[text] = animator;

            return animator;
        }

        /// <summary>
        /// Spiral animation
        /// </summary>
        public static TextEffect Spiral(TMP_Text text, float radius = 50f, float duration = 2f, int rotations = 1)
        {
            var animator = new TextEffect();
            animator.targetText = text;
            StoreOriginalValues(text);

            var sequence = DOTween.Sequence();
            Vector3 centerPos = originalPositions[text];

            int steps = 36 * rotations; // 36 steps per rotation
            float angleStep = 360f / 36f;

            for (int i = 0; i <= steps; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                float currentRadius = radius * (1f - (float)i / steps); // Spiral inward

                Vector3 spiralPos = centerPos + new Vector3(
                    Mathf.Cos(angle) * currentRadius,
                    Mathf.Sin(angle) * currentRadius,
                    0
                );

                sequence.Append(text.transform.DOLocalMove(spiralPos, duration / steps));
            }

            sequence.Append(text.transform.DOLocalMove(centerPos, 0.2f));

            animator.currentSequence = sequence;
            activeAnimators[text] = animator;

            return animator;
        }

        /// <summary>
        /// Matrix-style digital rain effect
        /// </summary>
        public static TextEffect DigitalRain(TMP_Text text, float duration = 3f, float speed = 0.1f)
        {
            var animator = new TextEffect();
            animator.targetText = text;
            StoreOriginalValues(text);

            string originalText = text.text;
            string chars = "01ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz!@#$%^&*()";

            var sequence = DOTween.Sequence();

            // Randomize text multiple times before revealing original
            for (int i = 0; i < 20; i++)
            {
                sequence.AppendCallback(() =>
                {
                    string randomText = "";
                    for (int j = 0; j < originalText.Length; j++)
                    {
                        if (originalText[j] == ' ')
                            randomText += ' ';
                        else
                            randomText += chars[UnityEngine.Random.Range(0, chars.Length)];
                    }
                    text.text = randomText;
                });
                sequence.AppendInterval(speed);
            }

            // Gradually reveal original text
            for (int i = 0; i < originalText.Length; i++)
            {
                int index = i;
                sequence.AppendCallback(() =>
                {
                    string revealText = "";
                    for (int j = 0; j < originalText.Length; j++)
                    {
                        if (j <= index)
                            revealText += originalText[j];
                        else if (originalText[j] == ' ')
                            revealText += ' ';
                        else
                            revealText += chars[UnityEngine.Random.Range(0, chars.Length)];
                    }
                    text.text = revealText;
                });
                sequence.AppendInterval(speed);
            }

            sequence.AppendCallback(() => text.text = originalText);

            animator.currentSequence = sequence;
            activeAnimators[text] = animator;

            return animator;
        }

        /// <summary>
        /// Neon sign flicker effect
        /// </summary>
        public static TextEffect NeonFlicker(TMP_Text text, Color neonColor, float intensity = 0.5f)
        {
            var animator = new TextEffect();
            animator.targetText = text;
            StoreOriginalValues(text);

            var sequence = DOTween.Sequence();

            // Random flicker pattern
            for (int i = 0; i < 20; i++)
            {
                float flickerIntensity = UnityEngine.Random.Range(0.3f, 1f);
                Color flickerColor = Color.Lerp(originalColors[text], neonColor, flickerIntensity);

                sequence.Append(text.DOColor(flickerColor, UnityEngine.Random.Range(0.05f, 0.15f)));

                // Occasional complete flicker off
                if (UnityEngine.Random.value < 0.3f)
                {
                    sequence.Append(text.DOColor(Color.black, 0.05f));
                    sequence.Append(text.DOColor(neonColor, 0.05f));
                }
            }

            sequence.Append(text.DOColor(neonColor, 0.2f));
            sequence.SetLoops(-1);

            animator.currentSequence = sequence;
            activeAnimators[text] = animator;

            return animator;
        }

        /// <summary>
        /// Explosion effect (scale and fade)
        /// </summary>
        public static TextEffect Explode(TMP_Text text, float maxScale = 3f, float duration = 0.8f)
        {
            var animator = new TextEffect();
            animator.targetText = text;
            StoreOriginalValues(text);

            var sequence = DOTween.Sequence();

            // Quick scale up with rotation
            sequence.Append(text.transform.DOScale(originalScales[text] * maxScale, duration * 0.7f).SetEase(Ease.OutQuad));
            sequence.Join(text.transform.DORotate(new Vector3(0, 0, 360), duration * 0.7f).SetEase(Ease.OutQuad));
            sequence.Join(text.DOFade(0f, duration * 0.7f).SetEase(Ease.OutQuad));

            // Reset
            sequence.AppendCallback(() =>
            {
                text.transform.localScale = originalScales[text];
                text.transform.localRotation = Quaternion.identity;
                text.color = originalColors[text];
            });

            animator.currentSequence = sequence;
            activeAnimators[text] = animator;

            return animator;
        }

        /// <summary>
        /// Heartbeat animation
        /// </summary>
        public static TextEffect Heartbeat(TMP_Text text, float intensity = 0.1f, float duration = 0.6f)
        {
            var animator = new TextEffect();
            animator.targetText = text;
            StoreOriginalValues(text);

            var sequence = DOTween.Sequence();

            // Double beat pattern
            sequence.Append(text.transform.DOScale(originalScales[text] * (1f + intensity), duration * 0.2f).SetEase(Ease.OutQuad));
            sequence.Append(text.transform.DOScale(originalScales[text], duration * 0.2f).SetEase(Ease.InQuad));
            sequence.AppendInterval(duration * 0.1f);
            sequence.Append(text.transform.DOScale(originalScales[text] * (1f + intensity * 0.7f), duration * 0.15f).SetEase(Ease.OutQuad));
            sequence.Append(text.transform.DOScale(originalScales[text], duration * 0.15f).SetEase(Ease.InQuad));
            sequence.AppendInterval(duration * 0.2f);

            sequence.SetLoops(-1);

            animator.currentSequence = sequence;
            activeAnimators[text] = animator;

            return animator;
        }

        #endregion Advanced Animation Methods

        #region Utility Methods

        /// <summary>
        /// Get all active animations
        /// </summary>
        public static Dictionary<TMP_Text, TextEffect> GetActiveAnimations()
        {
            return new Dictionary<TMP_Text, TextEffect>(activeAnimators);
        }

        /// <summary>
        /// Pause all animations
        /// </summary>
        public static void PauseAllAnimations()
        {
            foreach (var animator in activeAnimators.Values)
            {
                animator.Pause();
            }
        }

        /// <summary>
        /// Resume all animations
        /// </summary>
        public static void ResumeAllAnimations()
        {
            foreach (var animator in activeAnimators.Values)
            {
                animator.Resume();
            }
        }

        /// <summary>
        /// Get animation count
        /// </summary>
        public static int GetActiveAnimationCount()
        {
            return activeAnimators.Count;
        }

        /// <summary>
        /// Clear all stored original values (use with caution)
        /// </summary>
        public static void ClearOriginalValues()
        {
            originalPositions.Clear();
            originalScales.Clear();
            originalColors.Clear();
            originalRotations.Clear();
        }

        #endregion Utility Methods
    }
}