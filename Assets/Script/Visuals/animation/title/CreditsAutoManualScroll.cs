using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CreditsAutoManualScroll : MonoBehaviour
{
    // 앞으로 입력이 늘어날 예정이면 "행동(Action)" 단위로 관리하는 게 가장 편함.
    // 필요한 액션이 생기면 여기에만 추가하면 됨.
    public enum InputAction
    {
        ExitToTitle,     // Esc
        SpeedUpFast,     // Space
        SpeedUp,         // UpArrow
        SlowDown,        // DownArrow
        // 예: Pause, SkipToEnd, ToggleUI 등 계속 추가 가능
    }

    [System.Serializable]
    public class InputToggleEntry
    {
        public InputAction action;
        public bool enabled = true;
    }

    [Header("Refs")]
    [SerializeField] private RectTransform viewport; // Viewport (마스크 창)
    [SerializeField] private RectTransform content;  // Content (텍스트 부모)
    [SerializeField] private CanvasGroup contentCanvasGroup; // (선택) 시작 깜빡임 방지

    [Header("Speed")]
    [SerializeField] private float baseSpeed = 30f;
    [SerializeField] private float spaceFastMultiplier = 3f;
    [SerializeField] private float upMultiplier = 2f;
    [SerializeField] private float downMultiplier = 0.5f;

    [Header("Start / End Padding")]
    [SerializeField] private float startPadding = 120f;
    [SerializeField] private float endPadding = 40f;

    [Header("Exit")]
    [SerializeField] private string titleSceneName = "TitleScene";

    [Header("Optional")]
    [SerializeField] private bool resetOnEnable = true;

    [Header("Input Toggles (Expandable)")]
    [Tooltip("여기에 액션을 추가/ONOFF 하세요. 리스트에 없는 액션은 기본적으로 '활성' 처리됩니다.")]
    [SerializeField]
    private List<InputToggleEntry> inputToggles = new List<InputToggleEntry>()
    {
        new InputToggleEntry { action = InputAction.ExitToTitle, enabled = true },
        new InputToggleEntry { action = InputAction.SpeedUpFast, enabled = true },
        new InputToggleEntry { action = InputAction.SpeedUp, enabled = true },
        new InputToggleEntry { action = InputAction.SlowDown, enabled = true },
    };

    private bool reachedEnd;
    private Coroutine resetRoutine;

    // 런타임 조회용 캐시(빠름)
    private Dictionary<InputAction, bool> toggleMap;

    private void Awake()
    {
        BuildToggleMap();
    }

    private void OnValidate()
    {
        // 인스펙터에서 수정될 때 중복/누락 방지용 정리
        NormalizeToggles();
    }

    private void OnEnable()
    {
        reachedEnd = false;

        if (contentCanvasGroup != null)
            contentCanvasGroup.alpha = 0f;

        if (!resetOnEnable) return;

        if (resetRoutine != null) StopCoroutine(resetRoutine);
        resetRoutine = StartCoroutine(ResetToBottomRoutine());
    }

    private IEnumerator ResetToBottomRoutine()
    {
        // 레이아웃/폰트 반영 타이밍을 확실히 기다림 (중요)
        yield return null;
        yield return null;

        if (viewport == null || content == null) yield break;

        // 레이아웃 강제 갱신
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);

        ResetToBottomOutside();

        if (contentCanvasGroup != null)
            contentCanvasGroup.alpha = 1f;
    }

    private void Update()
    {
        if (viewport == null || content == null) return;

        // ESC (나가기)
        if (IsEnabled(InputAction.ExitToTitle) && Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene(titleSceneName);
            return;
        }

        if (reachedEnd) return;

        float speed = baseSpeed;

        // Space (가속)
        if (IsEnabled(InputAction.SpeedUpFast) && Input.GetKey(KeyCode.Space))
            speed *= spaceFastMultiplier;

        // ↑ (더 빠르게)
        if (IsEnabled(InputAction.SpeedUp) && Input.GetKey(KeyCode.UpArrow))
            speed *= upMultiplier;

        // ↓ (느리게)
        if (IsEnabled(InputAction.SlowDown) && Input.GetKey(KeyCode.DownArrow))
            speed *= downMultiplier;

        // 위로 이동
        content.localPosition += Vector3.up * speed * Time.deltaTime;

        CheckEndAndClamp();
    }

    private void ResetToBottomOutside()
    {
        // viewport 기준으로 content bounds 계산
        Bounds b = RectTransformUtility.CalculateRelativeRectTransformBounds(viewport, content);

        // Viewport 아래 경계(y)
        float viewportHalfH = viewport.rect.height * 0.5f;
        float bottomEdgeY = -viewportHalfH;

        // content의 top(b.max.y)이 viewport 아래보다 더 아래에 오도록
        float targetTopY = bottomEdgeY - startPadding;

        float deltaY = targetTopY - b.max.y;
        content.localPosition += new Vector3(0f, deltaY, 0f);
    }

    private void CheckEndAndClamp()
    {
        Bounds b = RectTransformUtility.CalculateRelativeRectTransformBounds(viewport, content);

        float viewportHalfH = viewport.rect.height * 0.5f;
        float topEdgeY = viewportHalfH;

        // content의 bottom이 viewport 위를 지나가면 마지막 줄이 사라짐
        float endLineY = topEdgeY + endPadding;

        if (b.min.y >= endLineY)
        {
            float deltaY = endLineY - b.min.y;
            content.localPosition += new Vector3(0f, deltaY, 0f);
            reachedEnd = true;
        }
    }

    // ------------------------
    // Input Toggle System
    // ------------------------

    /// <summary>
    /// 리스트(inputToggles)를 기반으로 런타임 캐시 맵을 구성
    /// </summary>
    private void BuildToggleMap()
    {
        if (toggleMap == null) toggleMap = new Dictionary<InputAction, bool>();
        toggleMap.Clear();

        if (inputToggles == null) return;

        for (int i = 0; i < inputToggles.Count; i++)
        {
            var e = inputToggles[i];
            toggleMap[e.action] = e.enabled; // 중복이면 마지막 값이 덮어씀
        }
    }

    /// <summary>
    /// 특정 액션이 활성인지 확인. 리스트에 없는 액션은 "활성(true)"로 취급.
    /// </summary>
    public bool IsEnabled(InputAction action)
    {
        if (toggleMap == null) BuildToggleMap();

        if (toggleMap.TryGetValue(action, out bool enabled))
            return enabled;

        return true; // 리스트에 없으면 기본 허용
    }

    /// <summary>
    /// 런타임에서 액션 ON/OFF 변경 (연출/버튼/상태 제어용)
    /// </summary>
    public void SetEnabled(InputAction action, bool enabled)
    {
        // 리스트에도 반영(인스펙터 기반 유지)
        if (inputToggles == null) inputToggles = new List<InputToggleEntry>();

        bool found = false;
        for (int i = 0; i < inputToggles.Count; i++)
        {
            if (inputToggles[i].action == action)
            {
                inputToggles[i].enabled = enabled;
                found = true;
                break;
            }
        }

        if (!found)
        {
            inputToggles.Add(new InputToggleEntry { action = action, enabled = enabled });
        }

        BuildToggleMap();
    }

    /// <summary>
    /// 인스펙터에서 중복 액션이 생겼을 때 정리 (첫 항목만 유지)
    /// + enum에 있는 기본 액션들을 리스트에 자동 보강하고 싶으면 여기서 처리 가능
    /// </summary>
    private void NormalizeToggles()
    {
        if (inputToggles == null) inputToggles = new List<InputToggleEntry>();

        // 중복 제거 (같은 action이 여러 개면 첫 번째만 남김)
        var seen = new HashSet<InputAction>();
        for (int i = inputToggles.Count - 1; i >= 0; i--)
        {
            var a = inputToggles[i].action;
            if (seen.Contains(a))
            {
                inputToggles.RemoveAt(i);
                continue;
            }
            seen.Add(a);
        }

        // Awake 전에 OnValidate에서 map 갱신
        BuildToggleMap();
    }
}