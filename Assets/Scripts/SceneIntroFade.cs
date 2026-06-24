using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Waits for the intro subtitle animation to finish, then fades a black overlay in.
/// Wire subtitle Animator/Animation and the black overlay in the Inspector.
/// You can also call <see cref="OnSubtitleAnimationFinished"/> from an Animation Event.
/// </summary>
public class SceneIntroFade : MonoBehaviour
{
    [Header("Subtitle")]
    [Tooltip("Animator that auto-plays the scrolling subtitle on scene enter.")]
    public Animator subtitleAnimator;

    [Tooltip("Optional. Leave empty to wait for whatever state is playing on layer 0.")]
    public string subtitleStateName;

    [Tooltip("Legacy Animation component instead of Animator.")]
    public Animation subtitleAnimation;

    [Tooltip("Used when no animator/animation is assigned.")]
    public float subtitleFallbackDuration = 5f;

    [Header("Black overlay")]
    public GameObject blackOverlay;

    [Tooltip("Optional. Resolved from blackOverlay if empty.")]
    public CanvasGroup blackCanvasGroup;

    [Tooltip("Optional. Resolved from blackOverlay if empty.")]
    public Image blackImage;

    [Tooltip("Optional. For world-space black sprites.")]
    public SpriteRenderer blackSprite;

    public float fadeDuration = 2f;

    [Range(0f, 1f)]
    public float targetAlpha = 1f;

    [Header("Events")]
    public UnityEvent onFadeComplete;

    [Header("Meta dialogue")]
    [Tooltip("Starts when the screen is fully black.")]
    public MetaDialogueManager metaDialogueManager;

    public DialogueAsset metaDialogue;

    bool subtitleFinished;
    bool fadeStarted;

    void Awake()
    {
        ResolveOverlayReferences();
        SetOverlayAlpha(0f);

        if (blackOverlay != null)
            blackOverlay.SetActive(true);
    }

    void Start()
    {
        StartCoroutine(RunSequence());
    }

    IEnumerator RunSequence()
    {
        yield return WaitForSubtitle();

        if (!fadeStarted)
            yield return FadeInOverlay();
    }

    IEnumerator WaitForSubtitle()
    {
        if (subtitleAnimator != null)
        {
            yield return null;
            yield return WaitForAnimatorComplete(subtitleAnimator, subtitleStateName);
            yield break;
        }

        if (subtitleAnimation != null)
        {
            while (subtitleAnimation.isPlaying)
                yield return null;
            yield break;
        }

        if (subtitleFallbackDuration > 0f)
            yield return new WaitForSeconds(subtitleFallbackDuration);
    }

    IEnumerator WaitForAnimatorComplete(Animator animator, string stateName)
    {
        const int layer = 0;

        while (animator.IsInTransition(layer))
            yield return null;

        while (true)
        {
            if (subtitleFinished)
                yield break;

            var info = animator.GetCurrentAnimatorStateInfo(layer);

            if (!string.IsNullOrEmpty(stateName) && !info.IsName(stateName))
            {
                yield return null;
                continue;
            }

            if (info.normalizedTime >= 1f && !animator.IsInTransition(layer))
                yield break;

            yield return null;
        }
    }

    /// <summary>Call from an Animation Event at the end of the subtitle clip.</summary>
    public void OnSubtitleAnimationFinished()
    {
        subtitleFinished = true;

        if (!fadeStarted && isActiveAndEnabled)
            StartCoroutine(FadeInOverlay());
    }

    IEnumerator FadeInOverlay()
    {
        if (fadeStarted)
            yield break;

        fadeStarted = true;

        if (blackOverlay != null && !blackOverlay.activeSelf)
            blackOverlay.SetActive(true);

        ResolveOverlayReferences();

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = fadeDuration > 0f ? Mathf.Clamp01(elapsed / fadeDuration) : 1f;
            SetOverlayAlpha(Mathf.Lerp(0f, targetAlpha, t));
            yield return null;
        }

        SetOverlayAlpha(targetAlpha);

        if (metaDialogueManager != null && metaDialogue != null)
            metaDialogueManager.StartDialogue(metaDialogue);

        onFadeComplete?.Invoke();
    }

    void ResolveOverlayReferences()
    {
        if (blackOverlay == null)
            return;

        if (blackCanvasGroup == null)
            blackCanvasGroup = blackOverlay.GetComponent<CanvasGroup>();
        if (blackImage == null)
            blackImage = blackOverlay.GetComponent<Image>();
        if (blackSprite == null)
            blackSprite = blackOverlay.GetComponent<SpriteRenderer>();
    }

    void SetOverlayAlpha(float alpha)
    {
        if (blackCanvasGroup != null)
            blackCanvasGroup.alpha = alpha;

        if (blackImage != null)
        {
            Color c = blackImage.color;
            c.a = alpha;
            blackImage.color = c;
        }

        if (blackSprite != null)
        {
            Color c = blackSprite.color;
            c.a = alpha;
            blackSprite.color = c;
        }
    }
}
