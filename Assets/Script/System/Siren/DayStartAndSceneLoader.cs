using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class DayStartAndSceneLoader : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private LineAnimation line;
    [SerializeField] private TMP_Text dayText;

    [Header("Day")]
    [SerializeField] private int dayNumber = 1;
    [SerializeField] private string messageFormat = "{0}일차 시작하겠습니다";

    [Header("UI Timing")]
    [SerializeField] private float messageSeconds = 1.2f;

    [Header("Scene")]
   //"검문소 씬 이름 (Build Settings에 추가되어 있어야 함)
    [SerializeField] private string inspectionSceneName = "InspectionScene";

    private bool busy;

    private void Awake()
    {
        if (line == null) line = FindFirstObjectByType<LineAnimation>();
        if (dayText != null) dayText.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (line != null) line.OnEntrantEnteredDesk += HandleEntrantEnteredDesk;
    }

    private void OnDisable()
    {
        if (line != null) line.OnEntrantEnteredDesk -= HandleEntrantEnteredDesk;
    }

    // 사이렌에서 호출할 함수
    public void StartDayBySiren()
    {
        if (busy) return;
        busy = true;
        StartCoroutine(StartDayRoutine());
    }

    private IEnumerator StartDayRoutine()
    {
        // 1) 문구 표시
        if (dayText != null)
        {
            dayText.text = string.Format(messageFormat, dayNumber);
            dayText.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(messageSeconds);

        if (dayText != null) dayText.gameObject.SetActive(false);

        // 2) 한 명 호출
        if (line != null) line.CallNext();
        // 3) 씬 전환은 HandleEntrantEnteredDesk에서 처리
    }

    private void HandleEntrantEnteredDesk(Entrant entrant)
    {
        // entrant가 deskPos에 "들어온 순간"
        // 씬 전환
        SceneManager.LoadScene(inspectionSceneName);
    }
}
