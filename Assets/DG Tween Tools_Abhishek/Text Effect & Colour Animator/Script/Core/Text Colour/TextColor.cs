using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace AbhishekSahu.DGTweening
{
    public class TextColor
    {
        private static Dictionary<TMP_Text, TextColor> activeColorAnimators = new Dictionary<TMP_Text, TextColor>();
        private static Dictionary<TMP_Text, Color> originalColors = new Dictionary<TMP_Text, Color>();

        private TMP_Text targetText;
        private Sequence colorSequence;

        public TextColor()
        { 
        }

        public TextColor(TMP_Text text)
        {
            targetText = text;

            if (!originalColors.ContainsKey(text))
                originalColors[text] = text.color;

            activeColorAnimators[text] = this;
        }

        private void SetSequence(Sequence sequence)
        {
            if (targetText == null) return;

            Kill();
            colorSequence = sequence;
        }

        public void Stop()
        {
            if (targetText == null) return;

            Kill();
            if (originalColors.ContainsKey(targetText))
                targetText.color = originalColors[targetText];

            activeColorAnimators.Remove(targetText);
        }

        public void Kill()
        {
            colorSequence?.Kill();
        }

        public void Pause()
        {
            colorSequence?.Pause();
        }

        public void Resume()
        {
            colorSequence?.Play();
        }

        public void SetLoops(int loops, LoopType loopType = LoopType.Restart)
        {
            colorSequence?.SetLoops(loops, loopType);
        }

        public bool IsPlaying() => colorSequence != null && colorSequence.IsPlaying();

        public float GetProgress() => colorSequence?.ElapsedPercentage() ?? 0f;

        public static void StopAnimation(TMP_Text text)
        {
            if (activeColorAnimators.ContainsKey(text))
                activeColorAnimators[text].Stop();
        }

        public static void StopAll()
        {
            foreach (var animator in activeColorAnimators.Values)
                animator.Stop();
            activeColorAnimators.Clear();
        }

        // ---------- COLOR METHODS BELOW ----------

        public static TextColor ChangeColor(TMP_Text text, Color color)
        {
            var animator = new TextColor(text);
            text.color = color;

            var sequence = DOTween.Sequence();
            sequence.AppendCallback(() => { }); // dummy

            animator.SetSequence(sequence);
            return animator;
        }

        public static TextColor ChangeColor(TMP_Text text, Color color, float duration)
        {
            var animator = new TextColor(text);
            var sequence = DOTween.Sequence().Append(text.DOColor(color, duration));
            animator.SetSequence(sequence);
            return animator;
        }

        public static TextColor ChangeColor(TMP_Text text, Color color, float duration, Ease ease)
        {
            var animator = new TextColor(text);
            var sequence = DOTween.Sequence().Append(text.DOColor(color, duration).SetEase(ease));
            animator.SetSequence(sequence);
            return animator;
        }

        public static TextColor AnimateMultipleColors(TMP_Text text, Color[] colors, float duration = 1f, bool loop = false)
        {
            var animator = new TextColor(text);
            if (colors.Length == 0) return animator;

            var sequence = DOTween.Sequence();
            float colorDuration = duration / colors.Length;

            foreach (var color in colors)
                sequence.Append(text.DOColor(color, colorDuration));

            if (loop)
                sequence.SetLoops(-1);

            animator.SetSequence(sequence);
            return animator;
        }

        public static TextColor AnimateMultipleColors(TMP_Text text, Color[] colors, float[] durations, bool loop = false)
        {
            var animator = new TextColor(text);
            if (colors.Length == 0 || durations.Length == 0) return animator;

            int min = Mathf.Min(colors.Length, durations.Length);
            var sequence = DOTween.Sequence();

            for (int i = 0; i < min; i++)
                sequence.Append(text.DOColor(colors[i], durations[i]));

            if (loop)
                sequence.SetLoops(-1);

            animator.SetSequence(sequence);
            return animator;
        }

        public static TextColor AnimateMultipleColors(TMP_Text text, Color[] colors, float[] durations, Ease[] eases, bool loop = false)
        {
            var animator = new TextColor(text);
            if (colors.Length == 0 || durations.Length == 0) return animator;

            int min = Mathf.Min(colors.Length, durations.Length, eases?.Length ?? durations.Length);
            var sequence = DOTween.Sequence();

            for (int i = 0; i < min; i++)
            {
                var tween = text.DOColor(colors[i], durations[i]);
                if (eases != null && i < eases.Length)
                    tween.SetEase(eases[i]);

                sequence.Append(tween);
            }

            if (loop)
                sequence.SetLoops(-1);

            animator.SetSequence(sequence);
            return animator;
        }

        public static TextColor LerpColor(TMP_Text text, Color fromColor, Color toColor, float duration = 1f, bool pingPong = false)
        {
            var animator = new TextColor(text);
            var sequence = DOTween.Sequence();

            sequence.Append(text.DOColor(fromColor, 0f));
            sequence.Append(text.DOColor(toColor, duration));

            if (pingPong)
            {
                sequence.Append(text.DOColor(fromColor, duration));
                sequence.SetLoops(-1);
            }

            animator.SetSequence(sequence);
            return animator;
        }

        public static TextColor RandomColor(TMP_Text text, float interval = 0.5f, bool loop = true)
        {
            var animator = new TextColor(text);
            var sequence = DOTween.Sequence();

            for (int i = 0; i < 10; i++)
            {
                var color = new Color(Random.value, Random.value, Random.value, 1f);
                sequence.Append(text.DOColor(color, interval));
            }

            if (loop)
                sequence.SetLoops(-1);

            animator.SetSequence(sequence);
            return animator;
        }

        public static TextColor HSVCycle(TMP_Text text, float saturation = 1f, float value = 1f, float duration = 3f, bool loop = true)
        {
            var animator = new TextColor(text);
            var sequence = DOTween.Sequence();
            int steps = 36;
            float stepDur = duration / steps;

            for (int i = 0; i < steps; i++)
            {
                float hue = (float)i / steps;
                var color = Color.HSVToRGB(hue, saturation, value);
                sequence.Append(text.DOColor(color, stepDur));
            }

            if (loop)
                sequence.SetLoops(-1);

            animator.SetSequence(sequence);
            return animator;
        }

        public static TextColor FadeToColor(TMP_Text text, Color targetColor, float duration = 1f)
        {
            var animator = new TextColor(text);
            var seq = DOTween.Sequence();

            var current = text.color;
            Color transparentFrom = new Color(current.r, current.g, current.b, 0f);
            Color transparentTo = new Color(targetColor.r, targetColor.g, targetColor.b, 0f);

            seq.Append(text.DOColor(transparentFrom, duration * 0.5f));
            seq.Append(text.DOColor(transparentTo, 0f));
            seq.Append(text.DOColor(targetColor, duration * 0.5f));

            animator.SetSequence(seq);
            return animator;
        }

        public static TextColor FlashColor(TMP_Text text, Color flashColor, float duration = 0.1f, int count = 3)
        {
            var animator = new TextColor(text);
            var sequence = DOTween.Sequence();
            var original = text.color;

            for (int i = 0; i < count; i++)
            {
                sequence.Append(text.DOColor(flashColor, duration));
                sequence.Append(text.DOColor(original, duration));
            }

            animator.SetSequence(sequence);
            return animator;
        }

        public static TextColor StrobeColor(TMP_Text text, Color[] strobeColors, float speed = 0.1f, bool loop = true)
        {
            var animator = new TextColor(text);
            if (strobeColors.Length == 0) return animator;

            var sequence = DOTween.Sequence();
            foreach (var color in strobeColors)
                sequence.Append(text.DOColor(color, speed));

            if (loop)
                sequence.SetLoops(-1);

            animator.SetSequence(sequence);
            return animator;
        }

        public static TextColor RestoreColor(TMP_Text text, float duration = 0.5f)
        {
            var animator = new TextColor(text);

            if (!originalColors.ContainsKey(text))
                originalColors[text] = text.color;

            var sequence = DOTween.Sequence();
            sequence.Append(text.DOColor(originalColors[text], duration));

            animator.SetSequence(sequence);
            return animator;
        }
    }
}