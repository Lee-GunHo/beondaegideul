using System.Collections;
using UnityEngine;

public class StoryCanvasToggle : MonoBehaviour
{
    [Header("Canvas")]
    [SerializeField] private GameObject storyCanvas; // Canvas_Story
    [SerializeField] private GameObject gameCanvas;  // Canvas_Game (UI)

    [Header("Pause ONLY Game Side")]
    [SerializeField] private GameObject gameRoot; // 월드 루트(플레이어/타이머/AI 등)

    [Tooltip("gameRoot를 끄기 싫다면, 멈출 스크립트들을 여기에 넣으세요.")]
    [SerializeField] private MonoBehaviour[] gameScriptsToDisable;

    public enum PauseMode
    {
        DisableGameRoot,
        DisableScriptsOnly,
        TimeScaleZero
    }

    [Header("Pause Mode")]
    [SerializeField] private PauseMode pauseMode = PauseMode.DisableScriptsOnly;

    [Header("Fade (Optional)")]
    [SerializeField] private CanvasGroup storyCanvasGroup;
    [SerializeField] private float fadeOutDuration = 0.15f;

    [Header("Disable Story Scripts After End")]
    [SerializeField] private MonoBehaviour[] storyScriptsToDisable;

    [Header("Skip Inputs")]
    [SerializeField] private bool useSpacebar = true;
    [SerializeField] private bool useEnter = true;
    [SerializeField] private bool useLeftClick = true;

    [Header("DEV RESET (REMOVE LATER)")]
    [SerializeField] private KeyCode resetKey = KeyCode.R;

    private bool finished;
    private float prevTimeScale = 1f;

    private void Start()
    {
        if (StoryProgress.HasSeenStory())
        {
            ForceStartGameImmediately();
            return;
        }

        if (storyCanvas != null) storyCanvas.SetActive(true);
        if (gameCanvas != null) gameCanvas.SetActive(false);

        PauseGameSide(true);

        if (storyCanvasGroup != null)
        {
            storyCanvasGroup.alpha = 1f;
            storyCanvasGroup.interactable = true;
            storyCanvasGroup.blocksRaycasts = true;
        }
    }

    private void Update()
    {
        // ===== 언제든 스토리 리셋 (개발용) =====
        if (Input.GetKeyDown(KeyCode.R))
        {
            StoryProgress.ResetStorySeen();
            Debug.Log("[STORY] ResetStorySeen()");
        }

        if (finished) return;
        if (!IsSkipPressed()) return;

        SkipStory();
    }

    private bool IsSkipPressed()
    {
        if (useSpacebar && Input.GetKeyDown(KeyCode.Space)) return true;

        if (useEnter && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
            return true;

        if (useLeftClick && Input.GetMouseButtonDown(0)) return true;

        return false;
    }

    private void ForceStartGameImmediately()
    {
        finished = true;

        if (storyCanvas != null) storyCanvas.SetActive(false);
        if (gameCanvas != null) gameCanvas.SetActive(true);

        PauseGameSide(false);
    }

    // ===== 정상 종료 =====
    public void FinishStory()
    {
        if (finished) return;
        finished = true;

        StartCoroutine(EndRoutine(markSeen: true));
    }

    public void SkipStory()
    {
        if (finished) return;
        finished = true;

        StartCoroutine(EndRoutine(markSeen: true));
    }

    public void CloseStoryWithoutSaving()
    {
        if (finished) return;
        finished = true;

        StartCoroutine(EndRoutine(markSeen: false));
    }

    private IEnumerator EndRoutine(bool markSeen)
    {
        if (markSeen)
            StoryProgress.MarkStorySeen();

        if (storyCanvasGroup != null && fadeOutDuration > 0f)
        {
            float t = 0f;
            float startAlpha = storyCanvasGroup.alpha;

            storyCanvasGroup.interactable = false;
            storyCanvasGroup.blocksRaycasts = false;

            while (t < fadeOutDuration)
            {
                t += Time.unscaledDeltaTime;
                storyCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t / fadeOutDuration);
                yield return null;
            }

            storyCanvasGroup.alpha = 0f;
        }

        if (storyScriptsToDisable != null)
        {
            foreach (var s in storyScriptsToDisable)
                if (s != null) s.enabled = false;
        }

        if (storyCanvas != null) storyCanvas.SetActive(false);
        if (gameCanvas != null) gameCanvas.SetActive(true);

        PauseGameSide(false);
    }

    private void PauseGameSide(bool pause)
    {
        if (pauseMode == PauseMode.TimeScaleZero)
        {
            if (pause)
            {
                prevTimeScale = Time.timeScale;
                Time.timeScale = 0f;
            }
            else
            {
                Time.timeScale = prevTimeScale <= 0f ? 1f : prevTimeScale;
            }
        }
        else if (pauseMode == PauseMode.DisableGameRoot)
        {
            if (gameRoot != null)
                gameRoot.SetActive(!pause);
        }

        if (gameScriptsToDisable != null)
        {
            foreach (var s in gameScriptsToDisable)
                if (s != null) s.enabled = !pause;
        }
    }
}