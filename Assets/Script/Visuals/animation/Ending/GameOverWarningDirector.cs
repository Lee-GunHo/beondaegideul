using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverWarningDirector : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string humanEndingSceneName = "Ending_Human";

    [Header("UI")]
    [SerializeField] private CanvasGroup group;   // 없으면 자동 생성/대체 가능
    [SerializeField] private TMP_Text warningText;

    [Header("Audio")]
    [SerializeField] private AudioSource sirenSource;
    [SerializeField] private AudioClip sirenClip;

    [Header("Typing")]
    [SerializeField] private float charInterval = 0.04f;
    [SerializeField] private float endDelay = 3.0f;

    [Header("Flow")]
    [SerializeField] private float beforeTypingDelay = 0.1f;

    private bool running;

    private const string MESSAGE_1 =
        "[인간 개체의 검문소 통과 확인.\n" +
        "검문 중단. 검문소를 임시 폐쇄합니다.]";

    private void Awake()
    {
        // UI 기본 숨김
        if (group != null) { group.alpha = 0f; group.blocksRaycasts = false; group.interactable = false; }
        if (warningText != null) warningText.text = "";
    }

    private void OnEnable()
    {
        EntrantManager.OnGameOver += HandleGameOver;
    }

    private void OnDisable()
    {
        EntrantManager.OnGameOver -= HandleGameOver;
    }

    private void HandleGameOver()
    {
        if (running) return;
        running = true;
        StartCoroutine(Sequence());
    }

    private IEnumerator Sequence()
    {
        // UI 표시
        if (group != null)
        {
            group.alpha = 1f;
            group.blocksRaycasts = true;
            group.interactable = false;
        }

        // 사이렌 시작
        if (sirenSource != null && sirenClip != null)
        {
            sirenSource.clip = sirenClip;
            sirenSource.loop = true;
            sirenSource.volume = 1f;
            sirenSource.Play();
        }

        yield return new WaitForSecondsRealtime(beforeTypingDelay);

        // 1차 경고 문구 타이핑
        if (warningText != null)
            yield return StartCoroutine(TypeText(warningText, MESSAGE_1, charInterval, endDelay));

        // 엔딩씬 전환
        SceneManager.LoadScene(humanEndingSceneName);
    }

    private IEnumerator TypeText(TMP_Text target, string fullText, float interval, float finalDelay)
    {
        target.text = "";
        foreach (char c in fullText)
        {
            target.text += c;
            yield return new WaitForSecondsRealtime(interval);
        }
        if (finalDelay > 0f) yield return new WaitForSecondsRealtime(finalDelay);
    }
}