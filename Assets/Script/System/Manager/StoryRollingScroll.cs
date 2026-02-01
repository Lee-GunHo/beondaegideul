using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StoryRollingScroll : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private RectTransform viewport; // Viewport (마스크 창)
    [SerializeField] private RectTransform content;  // Content (텍스트 부모)
    [SerializeField] private CanvasGroup contentCanvasGroup; // (선택) 시작 깜빡임 방지

    [Header("Speed")]
    [SerializeField] private float baseSpeed = 30f;

    [Header("Start / End Padding")]
    [SerializeField] private float startPadding = 120f;
    [SerializeField] private float endPadding = 40f;

    [Header("Optional")]
    [SerializeField] private bool resetOnEnable = true;
    [Tooltip("끝까지 도달하면 자동으로 멈춤")]
    [SerializeField] private bool stopAtEnd = true;

    [Header("Auto Start Game On End")]
    [Tooltip("끝나면 StoryCanvasToggle.FinishStory() 호출")]
    [SerializeField] private bool autoStartGameOnEnd = true;

    [Tooltip("끝 도달 후 자동 시작까지 딜레이(연출용)")]
    [SerializeField] private float autoStartDelay = 0.3f;

    [Tooltip("비워두면 씬에서 자동으로 찾음")]
    [SerializeField] private StoryCanvasToggle storyToggle;

    private bool reachedEnd;
    private bool autoStarted;
    private Coroutine resetRoutine;

    private void OnEnable()
    {
        reachedEnd = false;
        autoStarted = false;

        if (contentCanvasGroup != null)
            contentCanvasGroup.alpha = 0f;

        if (!resetOnEnable) return;

        if (resetRoutine != null) StopCoroutine(resetRoutine);
        resetRoutine = StartCoroutine(ResetToBottomRoutine());
    }

    private IEnumerator ResetToBottomRoutine()
    {
        yield return null;
        yield return null;

        if (viewport == null || content == null) yield break;

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);

        ResetToBottomOutside();

        if (contentCanvasGroup != null)
            contentCanvasGroup.alpha = 1f;
    }

    private void Update()
    {
        if (viewport == null || content == null) return;

        if (reachedEnd)
        {
            if (autoStartGameOnEnd && !autoStarted)
            {
                autoStarted = true;
                StartCoroutine(AutoStartRoutine());
            }

            if (stopAtEnd) return;
        }

        // 위로 이동 (TimeScale=0이어도 움직이도록 unscaled)
        content.localPosition += Vector3.up * baseSpeed * Time.unscaledDeltaTime;

        CheckEndAndClamp();
    }

    private IEnumerator AutoStartRoutine()
    {
        if (autoStartDelay > 0f)
            yield return new WaitForSecondsRealtime(autoStartDelay);

        if (storyToggle == null)
            storyToggle = FindFirstObjectByType<StoryCanvasToggle>();

        if (storyToggle != null)
        {
            storyToggle.FinishStory();
        }
        else
        {
            Debug.LogWarning("[StoryRollingScroll] StoryCanvasToggle not found. Can't auto start game.");
        }
    }

    public void ResetRoll()
    {
        reachedEnd = false;
        autoStarted = false;

        if (resetRoutine != null) StopCoroutine(resetRoutine);
        resetRoutine = StartCoroutine(ResetToBottomRoutine());
    }

    public bool IsReachedEnd() => reachedEnd;

    private void ResetToBottomOutside()
    {
        Bounds b = RectTransformUtility.CalculateRelativeRectTransformBounds(viewport, content);

        float viewportHalfH = viewport.rect.height * 0.5f;
        float bottomEdgeY = -viewportHalfH;

        float targetTopY = bottomEdgeY - startPadding;

        float deltaY = targetTopY - b.max.y;
        content.localPosition += new Vector3(0f, deltaY, 0f);
    }

    private void CheckEndAndClamp()
    {
        Bounds b = RectTransformUtility.CalculateRelativeRectTransformBounds(viewport, content);

        float viewportHalfH = viewport.rect.height * 0.5f;
        float topEdgeY = viewportHalfH;

        float endLineY = topEdgeY + endPadding;

        if (b.min.y >= endLineY)
        {
            float deltaY = endLineY - b.min.y;
            content.localPosition += new Vector3(0f, deltaY, 0f);
            reachedEnd = true;
        }
    }
}