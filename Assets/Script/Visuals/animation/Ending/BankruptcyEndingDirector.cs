using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class BankruptcyEndingDirector : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string titleSceneName = "Title";

    [Header("Background Images")]
    [SerializeField] private GameObject bankruptcyImageRoot; // 파산 배경(켜짐)
    [SerializeField] private GameObject disposedImageRoot;   // 폐기 배경(꺼짐)

    [Header("UI")]
    [SerializeField] private Image fadeImage;        // 검은 화면(알파 0→1)
    [SerializeField] private TMP_Text systemText;    // 시스템 문구
    [SerializeField] private TMP_Text stampText;     // [폐기]

    [Header("Audio")]
    [SerializeField] private AudioSource warningSource; // 루프 경고음
    [SerializeField] private AudioSource sfxSource;     // 단발(심정지/도장)
    [SerializeField] private AudioClip warningClip;
    [SerializeField] private AudioClip heartStopClip;
    [SerializeField] private AudioClip stampClip;

    [Header("Timing")]
    [SerializeField] private float textDelay = 0.2f;
    [SerializeField] private float warningFadeDuration = 1.2f;
    [SerializeField] private float darkenDuration = 1.6f;
    [SerializeField] private float afterBlackDelay = 0.25f;
    [SerializeField] private float disposedHoldDelay = 0.15f;

    [Header("System Text Typing")]
    [SerializeField] private float systemCharInterval = 0.04f; // 글자 간격
    [SerializeField] private float systemTextEndDelay = 3.0f;  // 전부 출력 후 딜레이

    private bool canClick;

    private void Start()
    {
        Time.timeScale = 1f;

        // 초기 배경 상태
        if (bankruptcyImageRoot != null) bankruptcyImageRoot.SetActive(true);
        if (disposedImageRoot != null) disposedImageRoot.SetActive(false);

        // 페이드 초기화
        if (fadeImage != null)
        {
            var c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
        }

        // 텍스트 초기화
        if (systemText != null) systemText.text = "";
        if (stampText != null)
        {
            stampText.text = "";
            stampText.gameObject.SetActive(false);
        }

        StartCoroutine(PlaySequence());
    }

    private void Update()
    {
        if (!canClick) return;

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            SceneManager.LoadScene(titleSceneName);
        }
    }

    private IEnumerator PlaySequence()
    {
        Debug.Log("[Ending] Bankruptcy triggered.");

        // 1) 파산 화면(이미 켜져있음) + 경고음 시작
        if (warningSource != null)
        {
            warningSource.clip = warningClip;
            warningSource.loop = true;
            warningSource.volume = 1f;
            warningSource.Play();
        }

        yield return new WaitForSecondsRealtime(textDelay);

        // 2) 시스템 문구 출력 (파산 화면에서)
        if (systemText != null)
        {
            string systemMessage =
    "[검문관의 파산 여부 확인.\n" +
    "계약 파기에 따른 폐기 기능을 실행합니다.]";

            yield return StartCoroutine(
                TypeText(
                    systemText,
                    systemMessage,
                    systemCharInterval,
                    systemTextEndDelay
                )
            );
        }

        // 3) 심정지 소리(단발) + 어두워짐 + 경고음 페이드
        if (sfxSource != null && heartStopClip != null)
            sfxSource.PlayOneShot(heartStopClip);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / Mathf.Max(0.01f, darkenDuration);

            // 화면 페이드(0→1)
            if (fadeImage != null)
            {
                var c = fadeImage.color;
                c.a = Mathf.Clamp01(t);
                fadeImage.color = c;
            }

            // 경고음 페이드(1→0) (경고 페이드가 더 빠를 수 있게)
            if (warningSource != null)
            {
                float wT = Mathf.Clamp01(t * (darkenDuration / Mathf.Max(0.01f, warningFadeDuration)));
                warningSource.volume = 1f - wT;
            }

            yield return null;
        }

        // 4) 완전 암전 + 모든 사운드 끊김
        if (warningSource != null) warningSource.Stop();
        if (sfxSource != null) sfxSource.Stop();

        yield return new WaitForSecondsRealtime(afterBlackDelay);

        // 5) "폐기 화면"으로 전환 (이때도 아직 화면은 암전 상태)
        if (bankruptcyImageRoot != null) bankruptcyImageRoot.SetActive(false);
        if (disposedImageRoot != null) disposedImageRoot.SetActive(true);

        // 폐기 화면이 바뀐 뒤 아주 잠깐 텀 (도장 치기 전)
        yield return new WaitForSecondsRealtime(disposedHoldDelay);

        // 6) 암전 상태 유지한 채로 [폐기] 도장 + 도장소리
        if (stampText != null)
        {
            stampText.gameObject.SetActive(true);
            stampText.text = "[폐기]";
        }

        if (sfxSource != null && stampClip != null)
            sfxSource.PlayOneShot(stampClip);

        // 7) 클릭 대기
        canClick = true;
    }

    private IEnumerator TypeText(
    TMP_Text targetText,
    string fullText,
    float charInterval,
    float endDelay
)
    {
        targetText.text = "";

        foreach (char c in fullText)
        {
            targetText.text += c;
            yield return new WaitForSecondsRealtime(charInterval);
        }

        // 마지막 글자 출력 후 딜레이
        if (endDelay > 0f)
            yield return new WaitForSecondsRealtime(endDelay);
    }
}