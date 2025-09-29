using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace AbhishekSahu.DGTweening
{
    [RequireComponent(typeof(RectTransform))]
    public class UiAnimator : MonoBehaviour
    {
        #region Variables

        [Header("Features")]
        [SerializeField] private bool enableIdleAnimation = true;

        [SerializeField] private bool animateOnEnable = true;
        [SerializeField] private bool animateOnClick = true;

        [Header("Animation Settings")]
        [SerializeField] private float animationDuration = 0.5f; // Duration of the appear animation

        [SerializeField] private float animationDelay = 0f; // Delay before the appear animation starts
        [SerializeField] private bool useRandomDelay = true; // Whether to add a random delay top of animationDelay before the animation starts
        [SerializeField] private float maxRandomDelay = 0.3f; // Maximum random delay if useRandomDelay is true
        [SerializeField] private Ease animationEase = Ease.OutBack;

        private Transform target;
        private Button button;
        private bool hasAnimated = false;

        #endregion Variables

        #region MonoBehaviour Callbacks

        private void Awake()
        {
            target = transform;
            button = GetComponent<Button>();

            if (animateOnEnable)
                target.localScale = Vector3.zero; // Set initial scale to zero

            if (animateOnClick && button != null)
                button.onClick.AddListener(PlayClickAnimation); // Add method to play click animation
        }

        private void OnEnable()
        {
            if (animateOnEnable && !hasAnimated)
            {
                hasAnimated = true;
                PlayAppearAnimation();
            }
        }

        private void OnDisable()
        {
            if (animateOnEnable && hasAnimated)
                hasAnimated = false;
        }

        #endregion MonoBehaviour Callbacks

        #region Animation Methods

        public void PlayAppearAnimation()
        {

            float delay = animationDelay;
            if (useRandomDelay)
                delay += Random.Range(0f, maxRandomDelay);

            target.DOKill();
            target.localScale = Vector3.zero;

            target.DOScale(Vector3.one, animationDuration)
                  .SetDelay(delay)
                  .SetEase(animationEase)
                  .OnComplete(() =>
                  {
                      if (enableIdleAnimation) PlayIdleAnimation();
                  });
        }

        public void PlayClickAnimation()
        {
            target.DOPunchScale(Vector3.one * 0.1f, 0.2f, 10, 1f);
        }

        public void PlayIdleAnimation()
        {
            target.DOScale(1.1f, 2f)
                  .SetLoops(-1, LoopType.Yoyo)
                  .SetEase(Ease.InOutSine);
        }

        #endregion Animation Methods
    }
}