using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class DayClearEndingDirector : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string titleSceneName = "Title";

    [Header("Panels")]
    [SerializeField] private GameObject panelNarration;
    [SerializeField] private GameObject panelEnding;
    [SerializeField] private GameObject panelClear;

    [Header("Texts")]
    [SerializeField] private TMP_Text narrationText;   // PanelNarration
    [SerializeField] private TMP_Text endingLineText;  // PanelEnding (LINE3)
    [SerializeField] private TMP_Text clearText;       // PanelClear ([CLEAR])

    [Header("Audio")]
    [SerializeField] private AudioSource sfxSource;    // 단발 SFX 재생용
    [SerializeField] private AudioClip gunshotClip;    // 총성

    [Header("Typing (LINE3)")]
    [SerializeField] private float charInterval = 0.04f;

    [Header("Clear Timing")]
    [SerializeField] private float clearAppearDelay = 3.0f;

    private bool waitingClick;
    private bool canExit;

    private const string LINE1 =
        "[마지막 검문을 마치자마자 두 명의 경호유닛이 나에게 찾아왔다.]";

    private const string LINE2 =
        "[난 경호유닛들의 인솔을 받으며 이동했지만 방향은 도시가 아니였다.]";

    private const string LINE3 =
        "[해당 검문관의 퇴직 절차를 진행합니다.]";

    private void Start()
    {
        Time.timeScale = 1f;

        // 초기 패널 상태
        SetActivePanel(panelNarration);

        // 텍스트 초기화
        if (narrationText != null) narrationText.text = "";
        if (endingLineText != null) endingLineText.text = "";
        if (clearText != null)
        {
            clearText.text = "";
            clearText.gameObject.SetActive(false);
        }

        StartCoroutine(Sequence());
    }

    private void Update()
    {
        bool pressed =
            Input.GetMouseButtonDown(0) ||
            Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.Return);

        if (!pressed) return;

        // CLEAR 이후 클릭 = 타이틀 복귀
        if (canExit)
        {
            SceneManager.LoadScene(titleSceneName);
            return;
        }

        // 단계 진행 클릭
        if (waitingClick)
            waitingClick = false;
    }

    private IEnumerator Sequence()
    {
        // --- PanelNarration: LINE1 -> 클릭 -> LINE1+LINE2(누적)
        if (narrationText != null) narrationText.text = LINE1;
        yield return WaitForClick();

        if (narrationText != null) narrationText.text = LINE1 + "\n" + LINE2;

        // 2번째 나레이션에서 클릭하면 엔딩 이미지로
        yield return WaitForClick();

        // --- PanelEnding: 엔딩 이미지 + LINE3 자동 타이핑
        SetActivePanel(panelEnding);

        if (endingLineText != null)
            yield return StartCoroutine(TypeText(endingLineText, LINE3, charInterval));

        // 타이핑 끝나면 클릭 → PanelClear
        yield return WaitForClick();

        // --- PanelClear: 즉시 전환 + 총성 즉시 + 3초 후 CLEAR
        SetActivePanel(panelClear);

        // 총성(즉시)
        if (sfxSource != null && gunshotClip != null)
            sfxSource.PlayOneShot(gunshotClip);

        // CLEAR 텍스트 숨김 상태로 시작
        if (clearText != null)
        {
            clearText.text = "";
            clearText.gameObject.SetActive(false);
        }

        // 3초 후 CLEAR 표시
        yield return new WaitForSecondsRealtime(clearAppearDelay);

        if (clearText != null)
        {
            clearText.gameObject.SetActive(true);
            clearText.text = "[CLEAR]";
        }

        // CLEAR 나온 뒤 클릭하면 타이틀
        canExit = true;
    }

    private void SetActivePanel(GameObject panel)
    {
        if (panelNarration != null) panelNarration.SetActive(false);
        if (panelEnding != null) panelEnding.SetActive(false);
        if (panelClear != null) panelClear.SetActive(false);

        if (panel != null) panel.SetActive(true);
    }

    private IEnumerator WaitForClick()
    {
        waitingClick = true;

        // 직전 클릭이 다음 단계로 바로 새는 것 방지
        yield return null;

        while (waitingClick)
            yield return null;
    }

    private IEnumerator TypeText(TMP_Text target, string fullText, float interval)
    {
        target.text = "";
        foreach (char c in fullText)
        {
            target.text += c;
            yield return new WaitForSecondsRealtime(interval);
        }
    }
}