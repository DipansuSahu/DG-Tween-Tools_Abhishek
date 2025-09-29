using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace AbhishekSahu.DGTweening
{
    public class UiSpriteAnimator : MonoBehaviour
    {
        #region Variables Fields

        [Header("Animation Settings")]
        [Tooltip("Time delay between frames.")]
        public float frameRate = 0.1f;

        [Tooltip("Play the animation automatically when enabled.")]
        public bool playOnStart = true;

        [Tooltip("Repeat the animation in a loop.")]
        public bool loopAnimation = true;

        [Tooltip("Play animation in reverse order.")]
        public bool playInReverse = false;

        [Header("Required Components")]
        [Tooltip("UI Image component where frames will be shown.")]
        public Image uiImage;

        [Tooltip("Array of sprites used as animation frames.")]
        public Sprite[] frames;

        [Header("Events")]
        [Tooltip("Event called when non-looping animation completes.")]
        public UnityEvent onAnimationComplete;

        private Coroutine animationCoroutine;

        #endregion Variables Fields

        #region Monobehaviour Callbacks

        private void OnEnable()
        {
            if (frames.Length > 0 && playOnStart)
            {
                Play();
            }
        }

        private void OnDisable()
        {
            Stop();
        }

        #endregion Monobehaviour Callbacks

        #region Public Methods

        /// <summary>
        /// Starts the sprite animation.
        /// </summary>
        public void Play()
        {
            if (animationCoroutine == null)
            {
                animationCoroutine = StartCoroutine(PlayAnimation());
            }
        }

        /// <summary>
        /// Stops the sprite animation.
        /// </summary>
        public void Stop()
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
                animationCoroutine = null;
            }
        }

        #endregion Public Methods

        #region Animation Methods

        /// <summary>
        /// Coroutine to play the sprite animation based on the current settings.
        /// </summary>
        private IEnumerator PlayAnimation()
        {
            int currentFrame = playInReverse ? frames.Length - 1 : 0;

            while (true)
            {
                uiImage.sprite = frames[currentFrame];
                yield return new WaitForSeconds(frameRate);

                currentFrame += playInReverse ? -1 : 1;

                bool animationEnded = playInReverse ? currentFrame < 0 : currentFrame >= frames.Length;

                if (animationEnded)
                {
                    if (loopAnimation)
                    {
                        currentFrame = playInReverse ? frames.Length - 1 : 0;
                    }
                    else
                    {
                        onAnimationComplete?.Invoke(); // Fire the event if animation completes and is not looping
                        break;
                    }
                }
            }

            animationCoroutine = null;
        }

        #endregion Animation Methods
    }
}