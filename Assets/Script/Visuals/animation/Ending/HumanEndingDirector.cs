using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class HumanEndingDirector : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string titleSceneName = "Title";

    [Header("Background Images")]
    [SerializeField] private GameObject endingImageRoot;    // 인간 엔딩 배경(켜짐)
    [SerializeField] private GameObject disposedImageRoot;  // 폐기 배경(꺼짐)

    [Header("UI")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private TMP_Text systemText;
    [SerializeField] private TMP_Text stampText;

    [Header("Audio")]
    [SerializeField] private AudioSource warningSource; // 여기서는 “경고음(사이렌 잔향)” 같은 걸 넣어도 됨
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip warningClip;
    [SerializeField] private AudioClip heartStopClip;
    [SerializeField] private AudioClip stampClip;

    [Header("Typing")]
    [SerializeField] private float charInterval = 0.04f;
    [SerializeField] private float textEndDelay = 3.0f;

    [Header("Timing")]
    [SerializeField] private float warningFadeDuration = 1.2f;
    [SerializeField] private float darkenDuration = 1.6f;
    [SerializeField] private float afterBlackDelay = 0.25f;
    [SerializeField] private float disposedHoldDelay = 0.15f;

    private bool canClick;

    private const string MESSAGE_2 =
        "[검문관의 계약 즉시 파기.\n" +
        "폐기 기능을 실행합니다.]";

    private void Start()
    {
        Time.timeScale = 1f;

        if (endingImageRoot != null) endingImageRoot.SetActive(true);
        if (disposedImageRoot != null) disposedImageRoot.SetActive(false);

        if (fadeImage != null)
        {
            var c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
        }

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
        // (선택) 경고음 시작
        if (warningSource != null && warningClip != null)
        {
            warningSource.clip = warningClip;
            warningSource.loop = true;
            warningSource.volume = 1f;
            warningSource.Play();
        }

        // 2차 문구 타이핑 + 마지막 3초 정적
        if (systemText != null)
            yield return StartCoroutine(TypeText(systemText, MESSAGE_2, charInterval, textEndDelay));

        // 심정지 소리
        if (sfxSource != null && heartStopClip != null)
            sfxSource.PlayOneShot(heartStopClip);

        // 암전 + 경고음 페이드
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / Mathf.Max(0.01f, darkenDuration);

            if (fadeImage != null)
            {
                var c = fadeImage.color;
                c.a = Mathf.Clamp01(t);
                fadeImage.color = c;
            }

            if (warningSource != null)
            {
                float wT = Mathf.Clamp01(t * (darkenDuration / Mathf.Max(0.01f, warningFadeDuration)));
                warningSource.volume = 1f - wT;
            }

            yield return null;
        }

        // 완전 암전 + 사운드 끊김
        if (warningSource != null) warningSource.Stop();
        if (sfxSource != null) sfxSource.Stop();

        yield return new WaitForSecondsRealtime(afterBlackDelay);

        // 폐기 배경으로 교체 (암전 상태에서 갈아끼움)
        if (endingImageRoot != null) endingImageRoot.SetActive(false);
        if (disposedImageRoot != null) disposedImageRoot.SetActive(true);

        yield return new WaitForSecondsRealtime(disposedHoldDelay);

        // [폐기] 도장
        if (stampText != null)
        {
            stampText.gameObject.SetActive(true);
            stampText.text = "[ 폐기 ]";
        }

        if (sfxSource != null && stampClip != null)
            sfxSource.PlayOneShot(stampClip);

        canClick = true;
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