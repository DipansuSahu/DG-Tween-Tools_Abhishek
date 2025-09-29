using AbhishekSahu.DGTweening;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class TextDemo : MonoBehaviour
{
    [Header("Press Space to see all animation effects on 'Target Text'")]
    [Header("Press C to see all colour effects on 'Target Text'")]
    public TMP_Text targetText;

    private int effectIndex, colorEffectIndex;
    private TextEffect currentAnimator;
    private TextColor currentColorAnimator;

    private readonly string[] effectNames = {
        "Scale", "Pulse", "Bounce", "Pop", "Elastic", "Heartbeat", "FadeIn", "FadeOut",
        "Shake", "Wobble", "Flip", "Breathe", "Rainbow", "Gradient", "SlideIn", "SlideOut",
        "Wave", "Spiral", "Typewriter", "DigitalRain", "NeonFlicker", "Explode"
    };

    private readonly string[] colorEffects = {
        "InstantColor", "FadeColor", "FadeColorWithEase",
        "MultiColor", "MultiColorDurations", "MultiColorEase",
        "LerpColor", "RandomColor", "HSVCycle",
        "FadeToColor", "FlashColor", "StrobeColor", "RestoreColor"
    };

    private void Start()
    {
        if (targetText == null)
        {
            Debug.LogError("TMP Text not assigned!");
            enabled = false;
            return;
        }

        targetText.text = "Colorful Animated Text!";
        PlayNextEffect();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PlayNextEffect();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            PlayNextColorEffect();
        }
    }

    private void PlayNextEffect()
    {
        TextEffect.StopAllAnimations(); // Stop previous
        TextEffect.ClearOriginalValues(); // Clean up

        string effect = effectNames[effectIndex];
        Debug.Log("Playing Effect: " + effect);

        switch (effect)
        {
            case "Scale": currentAnimator = targetText.AnimateScale(); break;
            case "Pulse": currentAnimator = targetText.AnimatePulse(Color.red, 1.1f); break;
            case "Bounce": currentAnimator = targetText.AnimateBounce(); break;
            case "Pop": currentAnimator = targetText.AnimatePop(); break;
            case "Elastic": currentAnimator = targetText.AnimateElastic(); break;
            case "Heartbeat": currentAnimator = targetText.AnimateHeartbeat(); break;
            case "FadeIn": currentAnimator = targetText.AnimateFadeIn(); break;
            case "FadeOut": currentAnimator = targetText.AnimateFadeOut(); break;
            case "Shake": currentAnimator = targetText.AnimateShake(); break;
            case "Wobble": currentAnimator = targetText.AnimateWobble(); break;
            case "Flip": currentAnimator = targetText.AnimateFlip(); break;
            case "Breathe": currentAnimator = targetText.AnimateBreathe(); break;
            case "Rainbow": currentAnimator = targetText.AnimateRainbow(); break;
            case "Gradient": currentAnimator = targetText.AnimateGradient(Color.red, Color.blue); break;
            case "SlideIn": currentAnimator = targetText.AnimateSlideIn(new Vector3(-200f, 0, 0)); break;
            case "SlideOut": currentAnimator = targetText.AnimateSlideOut(new Vector3(200f, 0, 0)); break;
            case "Wave": currentAnimator = targetText.AnimateWave(); break;
            case "Spiral": currentAnimator = targetText.AnimateSpiral(); break;
            case "Typewriter": currentAnimator = targetText.AnimateTypewriter(0.05f); break;
            case "DigitalRain": currentAnimator = targetText.AnimateDigitalRain(); break;
            case "NeonFlicker": currentAnimator = targetText.AnimateNeonFlicker(Color.cyan); break;
            case "Explode": currentAnimator = targetText.AnimateExplode(); break;
        }

        effectIndex = (effectIndex + 1) % effectNames.Length;
    }

    private void PlayNextColorEffect()
    {
        // Stop both motion and color animations to be safe
        TextEffect.StopAllAnimations();
        TextEffect.ClearOriginalValues();
        TextColor.StopAll();

        string effect = colorEffects[colorEffectIndex];
        Debug.Log("Color Effect: " + effect);

        switch (effect)
        {
            case "InstantColor":
                currentColorAnimator = targetText.ChangeColor(Color.yellow);
                break;

            case "FadeColor":
                currentColorAnimator = targetText.ChangeColor(Color.cyan, 1f);
                break;

            case "FadeColorWithEase":
                currentColorAnimator = targetText.ChangeColor(Color.green, 1f, Ease.InOutQuad);
                break;

            case "MultiColor":
                currentColorAnimator = targetText.AnimateMultipleColors(new Color[] { Color.red, Color.green, Color.blue }, 2f, true);
                break;

            case "MultiColorDurations":
                currentColorAnimator = targetText.AnimateMultipleColors(
                    new Color[] { Color.red, Color.yellow, Color.cyan },
                    new float[] { 0.5f, 0.3f, 0.6f }, true);
                break;

            case "MultiColorEase":
                currentColorAnimator = targetText.AnimateMultipleColors(
                    new Color[] { Color.magenta, Color.white, Color.gray },
                    new float[] { 0.4f, 0.4f, 0.4f },
                    new Ease[] { Ease.Linear, Ease.InOutSine, Ease.InOutBack }, true);
                break;

            case "LerpColor":
                currentColorAnimator = targetText.AnimateLerpColor(Color.blue, Color.green, 1.5f, pingPong: true);
                break;

            case "RandomColor":
                currentColorAnimator = targetText.AnimateRandomColor(0.2f, true);
                break;

            case "HSVCycle":
                currentColorAnimator = targetText.AnimateHSVCycle(1f, 1f, 3f, true);
                break;

            case "FadeToColor":
                currentColorAnimator = targetText.AnimateFadeToColor(Color.red, 1.5f);
                break;

            case "FlashColor":
                currentColorAnimator = targetText.AnimateFlashColor(Color.white, 0.1f, 4);
                break;

            case "StrobeColor":
                currentColorAnimator = targetText.AnimateStrobeColor(
                    new Color[] { Color.red, Color.black, Color.white }, 0.1f, true);
                break;

            case "RestoreColor":
                currentColorAnimator = targetText.RestoreColor(0.5f);
                break;
        }

        colorEffectIndex = (colorEffectIndex + 1) % colorEffects.Length;
    }

    /*private void PlayNextColorEffect()
    {
        TextAnimator.StopAllAnimations();
        TextAnimator.ClearOriginalValues();

        string effect = colorEffects[colorEffectIndex];
        Debug.Log("Color Effect: " + effect);

        switch (effect)
        {
            case "InstantColor": currentAnimator = targetText.ChangeColor(Color.yellow); break;

            case "FadeColor": currentAnimator = targetText.ChangeColor(Color.cyan, 1f); break;

            case "FadeColorWithEase": currentAnimator = targetText.ChangeColor(Color.green, 1f, Ease.InOutQuad); break;

            case "MultiColor": currentAnimator = targetText.AnimateMultipleColors(new Color[] { Color.red, Color.green, Color.blue }, 2f, true); break;

            case "MultiColorDurations": currentAnimator = targetText.AnimateMultipleColors(new Color[] { Color.red, Color.yellow, Color.cyan }, new float[] { 0.5f, 0.3f, 0.6f }, true); break;

            case "MultiColorEase":
                currentAnimator = targetText.AnimateMultipleColors(
                    new Color[] { Color.magenta, Color.white, Color.gray },
                    new float[] { 0.4f, 0.4f, 0.4f },
                    new Ease[] { Ease.Linear, Ease.InOutSine, Ease.InOutBack }, true);
                break;

            case "LerpColor": currentAnimator = targetText.AnimateLerpColor(Color.blue, Color.green, 1.5f, pingPong: true); break;

            case "RandomColor": currentAnimator = targetText.AnimateRandomColor(0.2f, true); break;

            case "HSVCycle": currentAnimator = targetText.AnimateHSVCycle(1f, 1f, 3f, true); break;

            case "FadeToColor": currentAnimator = targetText.AnimateFadeToColor(Color.red, 1.5f); break;

            case "FlashColor": currentAnimator = targetText.AnimateFlashColor(Color.white, 0.1f, 4); break;

            case "StrobeColor": currentAnimator = targetText.AnimateStrobeColor(new Color[] { Color.red, Color.black, Color.white }, 0.1f, true); break;

            case "RestoreColor": currentAnimator = targetText.RestoreColor(0.5f); break;
        }

        colorEffectIndex = (colorEffectIndex + 1) % colorEffects.Length;
    }*/
}