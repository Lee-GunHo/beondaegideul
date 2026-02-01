using UnityEngine;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }
    public int maxStageCount = 10;
    public int currentStageIndex = 0;
    public int currentTargetCount = 5;
    public int processedCount = 0;
    public bool isDayEnded = false;
    public int MaxDay => maxStageCount;
    public int CurrentDay => currentStageIndex + 1;

    private void Awake() { Instance = this; }

    public IdentificationData GenerateEntrantData()
    {
        bool isHuman = Random.value < 0.15f;
        string prefix = isHuman ? "SN" : new string[] { "KB", "ZT", "RX", "VN" }[Random.Range(0, 4)];
        string year = Random.Range(15, 26).ToString("D2");
        string suffix = isHuman ? "ZZ" : "FL";
        string id = $"{prefix}-{year}11-{suffix}";

        int headIdx = Random.Range(0, 4);
        int bodyIdx = Random.Range(0, 4);

        // --- [추가] 캡챠 타이머 경우의 수 설정 ---
        float assignedCaptchaTime;
        float rand = Random.value;

        if (rand < 0.4f) assignedCaptchaTime = 1.0f;              // 40% 확률: 1초
        else if (rand < 0.8f) assignedCaptchaTime = Random.Range(2.0f, 3.1f); // 40% 확률: 2~3초
        else assignedCaptchaTime = 5.0f;              // 20% 확률: 5초

        IdentificationData d = new IdentificationData
        {
            Entrant_ID = id,
            Actual_ID = id,
            Actual_HeadIndex = headIdx,
            Document_HeadIndex = headIdx,
            Actual_BodyIndex = bodyIdx,
            Document_BodyIndex = bodyIdx,
            Temperature = isHuman ? 36.5f : 32.0f,
            Actual_Sound = isHuman ? MachineSoundType.Heartbeat : MachineSoundType.Normal,
            Actual_Captcha = CaptchaResult.Pass,

            // --- [추가] 생성된 시간 데이터를 기록 ---
            CaptchaTime = assignedCaptchaTime
        };

        if (isHuman) d.IsHumanMask = true;
        return d;
    }

    public int GetHumanCountToday() => Mathf.RoundToInt((currentTargetCount - processedCount) * 0.2f);
    public void AddProcessedCount() { processedCount++; if (processedCount >= currentTargetCount) isDayEnded = true; }
}