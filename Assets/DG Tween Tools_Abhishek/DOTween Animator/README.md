# DOTween Animator — Usage Guide

**Author:** Abhishek Sahu
**Namespace:** `DGTweenTools.AbS`

A reusable, Inspector-driven component for animating Move / Rotate / Scale with DOTween — no code required for basic use, full C# API for when you need it.

---

## 1. Requirements & Setup

- DOTween must already be installed and initialized in your project (Tools > DOTween Utility Panel > Setup DOTween, if you haven't already).
- Copy both files into your project, **keeping the folder structure**:

```
Assets/
  YourFolder/
    DOTweenAnimator.cs
    Editor/
      DOTweenAnimatorEditor.cs
```

The `Editor` folder name is mandatory — Unity uses it to know `DOTweenAnimatorEditor.cs` (which references `UnityEditor`) should be excluded from builds. `DOTweenAnimator.cs` itself must **not** be inside an `Editor` folder, or you won't be able to add it as a component in builds.

- Add the component: select a GameObject → **Add Component → DGTweenTools → DOTween Animator**.

---

## 2. Core Concept: The Animation Chain

The component plays an ordered **list of Steps**, top to bottom, like a playlist:

```
Step 1: Rotate  (loop: Once)
Step 2: Scale   (loop: Fixed Count = 3)
Step 3: Move    (loop: Infinite)
```

Each step:
- Can enable **any combination** of Move / Rotate / Scale.
- Plays its enabled animations **together** (simultaneously) or **sequentially**, via the step's "Play Together (Join)" toggle.
- Loops **independently** before the chain moves to the next step.

Only the **last** step in the chain should use `Infinite` looping. An infinite step blocks everything after it (a sequence can't advance past a child that never finishes) — the component detects this and logs a warning if it happens by mistake.

There's also a **Whole-Sequence Loop** setting below the step list, which repeats the *entire chain* as one unit. Leave it as `Once` if your last step already loops infinitely.

---

## 3. Inspector Walkthrough

### Play Trigger
- `None` — the animation only plays when you call `Play()` from code.
- `OnStart` — plays automatically on `Start()`.
- `OnEnable` — plays every time the GameObject is enabled (e.g. when re-activated from a pool).

### Animation Chain (the step list)
Click **+** to add a step, drag the handle to reorder, click the foldout to expand a step:

| Field | Meaning |
|---|---|
| Step Name | Just a label, purely for your own organization in the list. |
| Play Together (Join) | ON = Move/Rotate/Scale in this step animate simultaneously. OFF = they play one after another in Move → Rotate → Scale order. |
| Move / Rotate / Scale toggle | Enables that animation for this step. Expand to configure: |
| — Local Space *(Move only)* | ON animates `localPosition`, OFF animates world `position`. |
| — Value | The Vector3 target or offset (position delta, euler rotation, or scale). |
| — Relative | ON = value is **added on top of** the transform's current value each time the step plays. OFF = value is the **absolute target**. |
| — Duration / Delay | Standard tween timing, in seconds. |
| — Rotate Mode *(Rotate only)* | DOTween's `RotateMode` (`Fast`, `FastBeyond360`, etc.) — use `FastBeyond360` if you want rotations past 360° to actually spin multiple times instead of taking the shortest path. |
| — Use Custom Curve | OFF = pick a DOTween `Ease`. ON = drive the tween with your own `AnimationCurve` for full custom timing. |
| Step Loop | `Once`, `Fixed Count` (with a Loop Count field), or `Infinite`. |
| Loop Type | `Restart`, `Yoyo`, `Incremental`, etc. — only shown when looping. |
| Step Events | `On Step Start`, `On Step Loop Complete` (fires once per loop cycle), `On Step Complete` (fires once the step's loops are fully done) — wire these in the Inspector like any `UnityEvent`. |

### Whole-Sequence Loop
Repeats the entire step chain. `Once` / `Fixed Count` / `Infinite`, same semantics as a step's loop.

### Chain Events
`On Start`, `On Sequence Loop Complete`, `On Complete`, `On Kill` — fire for the chain as a whole, not any individual step.

### Runtime Controls (Play Mode only)
Buttons at the bottom of the Inspector — **Play / Pause / Resume / Stop / Restart** — let you test the animation live without writing a trigger script.

---

## 4. Example: Rotate once → Scale x3 → Move forever

This is the exact setup for the "spin, then pulse three times, then drift away" pattern:

1. Add 3 steps.
2. **Step 1** — enable Rotate, `value = (0, 360, 0)`, `relative = ON`, duration `1`, Loop = `Once`.
3. **Step 2** — enable Scale, `value = (1.2, 1.2, 1.2)`, `relative = OFF` (absolute target), `loopType = Yoyo` so it pulses out and back, Loop = `Fixed Count`, `Loop Count = 3`.
4. **Step 3** — enable Move, `value = (0, 3, 0)`, `relative = ON`, duration `2`, Loop = `Infinite`.
5. Set **Play Trigger = OnStart** (or leave `None` and call `Play()` yourself).

---

## 5. Using It From Code

You don't need any code for the setup above — but the component exposes a full public API for triggering, controlling, and reacting to the animation:

```csharp
using UnityEngine;
using DGTweenTools.AbS;

public class ExampleUsage : MonoBehaviour
{
    [SerializeField] private DOTweenAnimator animator;

    private void OnMouseDown()
    {
        animator.Play();          // starts the chain from the top
    }

    public void OnPauseButtonPressed()
    {
        animator.TogglePause();   // pause if playing, resume if paused
    }

    public void OnResetButtonPressed()
    {
        animator.Stop();               // kills the running tween immediately
        animator.ResetToInitial();     // snaps transform back to its Awake() values
    }

    public void OnRestartButtonPressed()
    {
        animator.Restart();       // ResetToInitial() + Play(), in one call
    }
}
```

### Full API reference

| Method | What it does |
|---|---|
| `Play()` | Kills any running animation, builds the chain fresh from the current Inspector settings, and plays it. |
| `Pause()` | Pauses the active sequence. |
| `Resume()` | Resumes a paused sequence. |
| `TogglePause()` | Pauses if playing, resumes if paused. Safe to call even if nothing is playing. |
| `Stop(bool complete = false)` | Kills the sequence. Pass `true` to snap all tweens to their end values first (DOTween's kill-and-complete behavior). |
| `Restart()` | `ResetToInitial()` then `Play()` — use this for repeat-triggered effects (e.g. a button you can pulse over and over) so relative animations don't drift further each time. |
| `ResetToInitial()` | Snaps position/rotation/scale back to whatever they were in `Awake()`, without playing anything. |
| `IsPlaying` (property) | `true` while the sequence is active and not paused. |

### Wiring events from code instead of the Inspector

Every event field is a standard `UnityEvent`, so you can also hook them up at runtime:

```csharp
private void Start()
{
    animator.onComplete.AddListener(HandleChainComplete);
}

private void HandleChainComplete()
{
    Debug.Log("Whole animation chain finished.");
}
```

---

## 6. Common Recipes

**Idle floating loop (UI icon or pickup item)**
- 1 step, Move enabled, small `value` like `(0, 0.3, 0)`, `relative = ON`, `loopType = Yoyo`, Loop = `Infinite`.

**Button press pulse**
- 1 step, Scale enabled, `value = (0.9, 0.9, 0.9)`, `relative = OFF`, `loopType = Yoyo`, Loop = `Fixed Count = 1`, Play Trigger = `None`, call `animator.Play()` from your button's `onClick`.

**Spinning collectible (coin, gem)**
- 1 step, Rotate enabled, `value = (0, 360, 0)`, `relative = ON`, `rotateMode = FastBeyond360`, Loop = `Infinite`, `loopType = Restart`.

**Entrance animation (pop in, then settle into an idle float)**
- Step 1: Scale from small, `relative = OFF`, target `(1,1,1)`, `loopType = Restart`, Loop = `Once`.
- Step 2: Move small idle bob, Loop = `Infinite`.

---

## 7. Gotchas & Notes

- **Relative accumulates.** If `relative = ON` and you call `Play()` multiple times without `Restart()`/`ResetToInitial()`, the object keeps moving/rotating/scaling further each time from wherever it currently is — that's often what you want (e.g. repeated "nudge" effects), but use `Restart()` instead of `Play()` if you want it to always animate from the same starting point.
- **Only the last step should be Infinite.** The console will warn you and stop building the chain if an earlier step is infinite.
- **Adding a new step in the Inspector duplicates the previous step's values** (standard Unity list behavior), though Move/Rotate/Scale toggles and loop mode are reset to off/Once automatically — double check the new step's other fields (duration, easing, "Play Together") before using it.
- **`OnDestroy` auto-kills the tween**, so you don't need to manually clean up when the GameObject is destroyed mid-animation.

---

## 8. Extending It

The two files are intentionally straightforward to build on:

- Add a new `AnimStep` subclass (like `MoveStep`/`RotateStep`/`ScaleStep`) for things like color tweening (`DOColor`) or fade (`DOFade`), then wire it into `AppendStep()` in `DOTweenAnimator.cs` and add a matching `DrawAnim()` call in the editor.
- The editor's `DrawAnim()` / `StepAnimHeight()` pair is reused for all three animation types — follow that same pattern for any new type so its height and layout logic stays in sync automatically.



------------------------------------------------------------------------------------------------------------------------------------------------------------
------------------------------------------------------------------------------------------------------------------------------------------------------------



# DOTween Animator Simple --- Usage Guide

**Author:** Abhishek Sahu
**Namespace:** `DGTweenTools.AbS` 

`DOTweenAnimatorSimple` is a lightweight version of **DOTweenAnimator**
designed for common Move, Rotate, and Scale animations without creating
animation chains.

## Features

-   Inspector-driven setup
-   Move, Rotate and Scale animations
-   Play together or sequentially
-   Relative or absolute values
-   Custom Ease or AnimationCurve
-   Loop support (Once, Fixed Count, Infinite)
-   UnityEvents
-   Runtime controls (Play, Pause, Resume, Stop, Restart)

## Installation

``` text
Assets/
    YourFolder/
        DOTweenAnimatorSimple.cs
        Editor/
            DOTweenAnimatorSimpleEditor.cs
```

Add the component:

**Add Component → DGTweenTools → DOTween Animator Simple**

## Play Trigger

-   None
-   OnStart
-   OnEnable

## Animations

Each animation supports:

-   Value
-   Duration
-   Delay
-   Relative
-   Ease / Custom Curve

Additional options:

-   Move → Local Space
-   Rotate → Rotate Mode

## Sequence

-   **Play Together** → Move, Rotate and Scale run simultaneously.
-   Disabled → Move → Rotate → Scale.

## Loop

-   Once
-   Fixed Count
-   Infinite

Supports DOTween LoopType.

## Events

-   OnStart
-   OnLoopComplete
-   OnComplete
-   OnKill

## Runtime Controls

-   Play
-   Pause
-   Resume
-   Stop
-   Restart

## Code Example

``` csharp
using DGTweenTools.AbS;

public class Example : MonoBehaviour
{
    public DOTweenAnimatorSimple animator;

    void Start()
    {
        animator.Play();
    }
}
```

## Public API

-   Play()
-   Pause()
-   Resume()
-   TogglePause()
-   Stop(bool complete = false)
-   Restart()
-   ResetToInitial()
-   IsPlaying

## Common Examples

### Floating Object

-   Move
-   Relative ON
-   Loop = Infinite
-   LoopType = Yoyo

### Button Pulse

-   Scale
-   Loop = Fixed Count
-   LoopType = Yoyo

### Rotating Coin

-   Rotate
-   Relative ON
-   RotateMode = FastBeyond360
-   Infinite Loop

## Notes

-   Initial transform is cached in `Awake()`.
-   `Restart()` resets then replays.
-   `Play()` rebuilds the sequence each time.
-   Active tweens are automatically killed in `OnDestroy()`.