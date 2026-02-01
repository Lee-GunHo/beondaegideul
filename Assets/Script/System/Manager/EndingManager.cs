using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum EndingType
{
    None,
    Bankruptcy,
    HumanApproval,
    DayClear
}
public class EndingManager : MonoBehaviour
{
    public static EndingManager Instance { get; private set; }
    [Header("Scene Names")]
    [SerializeField] private string endingBankruptcyScene = "Ending_Bankruptcy";
    [SerializeField] private string titleScene = "TitleScene"; // 엔딩씬에서 돌아갈 때 쓸 수도 있음

    [Header("Timing")]
    [SerializeField] private float bankruptcyPreDelay = 0.3f;

    public static event Action OnBankruptcyWarning;

    [Header("Refs")]
    [SerializeField] private EconomyManager economy;
    [SerializeField] private StageManager stage;
    [SerializeField] private EntrantManager entrant;

    [Header("Human Approval")]
    [SerializeField] private int requiredHumanApproval = 10;

    private bool endingTriggered;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (endingTriggered) return;
        if (EconomyManager.Instance != null && EconomyManager.Instance.CurrentMoney <= 0)
        {
            StartCoroutine(BankruptcySequence());
        }
    }

    private void CheckEnding()
    {
        // 파산 엔딩 (최우선)
        if (economy.CurrentMoney <= 0)
        {
            TriggerEnding(EndingType.Bankruptcy);
            return;
        }

        // 인간 승인 엔딩
        if (entrant.ApprovedHumanCount >= requiredHumanApproval)
        {
            TriggerEnding(EndingType.HumanApproval);
            return;
        }

        // 일수 완료 엔딩
        if (stage.CurrentDay >= stage.MaxDay)
        {
            TriggerEnding(EndingType.DayClear);
        }
    }

    private void TriggerEnding(EndingType type)
    {
        endingTriggered = true;

        Debug.Log($"엔딩 발생: {type}");

        switch (type)
        {
            case EndingType.Bankruptcy:
                SceneManager.LoadScene("Ending_Bankruptcy");
                break;

            case EndingType.HumanApproval:
                SceneManager.LoadScene("Ending_Human");
                break;

            case EndingType.DayClear:
                SceneManager.LoadScene("Ending_DayClear");
                break;
        }
    }


    private IEnumerator BankruptcySequence()
    {
        endingTriggered = true;

        // (A안) 돈 UI를 빨갛게 바꾸는 신호
        OnBankruptcyWarning?.Invoke();

        // 0.3초 보여주기
        yield return new WaitForSecondsRealtime(bankruptcyPreDelay);

        // 씬 전환
        SceneManager.LoadScene(endingBankruptcyScene);
    }
}