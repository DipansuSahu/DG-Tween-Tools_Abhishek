using DG.Tweening;
using TMPro;
using UnityEngine;

namespace AbhishekSahu.DGTweening
{
    /// <summary>
    /// Extension methods for easier access to TMPTextAnimator
    /// </summary>
    public static class TextExtensions
    {
        #region Text Animation Extensions

        public static TextEffect Animate(this TMP_Text text)
        {
            return new TextEffect(text);
        }

        public static TextEffect AnimateScale(this TMP_Text text, float scaleTo = 1.1f, float duration = 0.2f)
        {
            return TextEffect.Scale(text, scaleTo, duration);
        }

        public static TextEffect AnimatePulse(this TMP_Text text, Color color, float scaleTo = 1.05f, float duration = 0.3f)
        {
            return TextEffect.Pulse(text, color, scaleTo, duration);
        }

        public static TextEffect AnimateFadeIn(this TMP_Text text, float duration = 0.5f)
        {
            return TextEffect.FadeIn(text, duration);
        }

        public static TextEffect AnimateFadeOut(this TMP_Text text, float duration = 0.5f)
        {
            return TextEffect.FadeOut(text, duration);
        }

        public static TextEffect AnimateRainbow(this TMP_Text text, float duration = 2f, bool loop = true)
        {
            return TextEffect.Rainbow(text, duration, loop);
        }

        public static TextEffect AnimateTypewriter(this TMP_Text text, float charDelay = 0.05f)
        {
            return TextEffect.Typewriter(text, charDelay);
        }

        public static TextEffect AnimateShake(this TMP_Text text, float intensity = 5f, float duration = 0.5f)
        {
            return TextEffect.Shake(text, intensity, duration);
        }

        public static TextEffect AnimateBounce(this TMP_Text text, float scaleTo = 1.2f, Vector3 positionOffset = default)
        {
            return TextEffect.Bounce(text, scaleTo, positionOffset);
        }

        public static TextEffect AnimateWobble(this TMP_Text text, float intensity = 10f, float duration = 0.5f)
        {
            return TextEffect.Wobble(text, intensity, duration);
        }

        public static TextEffect AnimateFlip(this TMP_Text text, bool horizontal = true, float duration = 0.5f)
        {
            return TextEffect.Flip(text, horizontal, duration);
        }

        public static TextEffect AnimatePop(this TMP_Text text, float scaleTo = 1.3f, float duration = 0.3f)
        {
            return TextEffect.Pop(text, scaleTo, duration);
        }

        public static TextEffect AnimateHeartbeat(this TMP_Text text, float intensity = 0.1f, float duration = 0.6f)
        {
            return TextEffect.Heartbeat(text, intensity, duration);
        }

        public static TextEffect AnimateExplode(this TMP_Text text, float maxScale = 3f, float duration = 0.8f)
        {
            return TextEffect.Explode(text, maxScale, duration);
        }

        public static TextEffect AnimateNeonFlicker(this TMP_Text text, Color neonColor, float intensity = 0.5f)
        {
            return TextEffect.NeonFlicker(text, neonColor, intensity);
        }

        public static TextEffect AnimateDigitalRain(this TMP_Text text, float duration = 3f, float speed = 0.1f)
        {
            return TextEffect.DigitalRain(text, duration, speed);
        }

        public static TextEffect AnimateSpiral(this TMP_Text text, float radius = 50f, float duration = 2f, int rotations = 1)
        {
            return TextEffect.Spiral(text, radius, duration, rotations);
        }

        public static TextEffect AnimateWave(this TMP_Text text, float amplitude = 10f, float frequency = 1f, float duration = 2f)
        {
            return TextEffect.WaveText(text, amplitude, frequency, duration);
        }

        public static TextEffect AnimateBreathe(this TMP_Text text, float scaleAmount = 0.05f, float duration = 2f)
        {
            return TextEffect.Breathe(text, scaleAmount, duration);
        }

        public static TextEffect AnimateGradient(this TMP_Text text, Color startColor, Color endColor, float duration = 1f, bool loop = true)
        {
            return TextEffect.GradientColor(text, startColor, endColor, duration, loop);
        }

        public static TextEffect AnimateSlideIn(this TMP_Text text, Vector3 direction, float duration = 0.5f, Ease ease = Ease.OutQuad)
        {
            return TextEffect.SlideIn(text, direction, duration, ease);
        }

        public static TextEffect AnimateSlideOut(this TMP_Text text, Vector3 direction, float duration = 0.5f, Ease ease = Ease.InQuad)
        {
            return TextEffect.SlideOut(text, direction, duration, ease);
        }

        public static TextEffect AnimateElastic(this TMP_Text text, float scaleTo = 1.2f, float duration = 0.8f)
        {
            return TextEffect.ElasticScale(text, scaleTo, duration);
        }

        #endregion Text Animation Extensions

        #region Color Animation Extensions

        public static TextColor ChangeColor(this TMP_Text text, Color color)
        {
            return TextColor.ChangeColor(text, color);
        }

        public static TextColor ChangeColor(this TMP_Text text, Color color, float duration)
        {
            return TextColor.ChangeColor(text, color, duration);
        }

        public static TextColor ChangeColor(this TMP_Text text, Color color, float duration, Ease ease)
        {
            return TextColor.ChangeColor(text, color, duration, ease);
        }

        public static TextColor AnimateMultipleColors(this TMP_Text text, Color[] colors, float duration = 1f, bool loop = false)
        {
            return TextColor.AnimateMultipleColors(text, colors, duration, loop);
        }

        public static TextColor AnimateMultipleColors(this TMP_Text text, Color[] colors, float[] durations, bool loop = false)
        {
            return TextColor.AnimateMultipleColors(text, colors, durations, loop);
        }

        public static TextColor AnimateMultipleColors(this TMP_Text text, Color[] colors, float[] durations, Ease[] eases, bool loop = false)
        {
            return TextColor.AnimateMultipleColors(text, colors, durations, eases, loop);
        }

        public static TextColor AnimateLerpColor(this TMP_Text text, Color fromColor, Color toColor, float duration = 1f, bool pingPong = false)
        {
            return TextColor.LerpColor(text, fromColor, toColor, duration, pingPong);
        }

        public static TextColor AnimateRandomColor(this TMP_Text text, float intervalDuration = 0.5f, bool loop = true)
        {
            return TextColor.RandomColor(text, intervalDuration, loop);
        }

        public static TextColor AnimateHSVCycle(this TMP_Text text, float saturation = 1f, float value = 1f, float duration = 3f, bool loop = true)
        {
            return TextColor.HSVCycle(text, saturation, value, duration, loop);
        }

        public static TextColor AnimateFadeToColor(this TMP_Text text, Color targetColor, float duration = 1f)
        {
            return TextColor.FadeToColor(text, targetColor, duration);
        }

        public static TextColor AnimateFlashColor(this TMP_Text text, Color flashColor, float flashDuration = 0.1f, int flashCount = 3)
        {
            return TextColor.FlashColor(text, flashColor, flashDuration, flashCount);
        }

        public static TextColor AnimateStrobeColor(this TMP_Text text, Color[] strobeColors, float strobeSpeed = 0.1f, bool loop = true)
        {
            return TextColor.StrobeColor(text, strobeColors, strobeSpeed, loop);
        }

        public static TextColor RestoreColor(this TMP_Text text, float duration = 0.5f)
        {
            return TextColor.RestoreColor(text, duration);
        }

        #endregion Color Animation Extensions
    }
}