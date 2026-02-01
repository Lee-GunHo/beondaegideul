using UnityEngine;

// 프로젝트 창 우클릭 -> GGJ -> StageData 로 생성 가능
[CreateAssetMenu(fileName = "Stage_01", menuName = "GGJ/StageData")]
public class StageData : ScriptableObject
{
    [Header("Day Info")]
    public int stageNumber;          // 제 n일차
    public int targetEntrantCount;   // 오늘 처리해야 할 목표 인원 수

    [Header("Economy Settings")]
    public int successReward;        // 성공 시 받는 보수 (예: 100)
    public int failurePenalty;       // 실패 시 차감되는 벌금 (예: 50)

    [Header("Difficulty Settings (0.0 ~ 1.0)")]
    [Range(0f, 1f)] public float humanProb;       // 인간 출현 확률
    [Range(0f, 1f)] public float contradictProb;  // 서류 위조(모순) 확률
    [Range(0f, 1f)] public float defectProb;      // 고장난 로봇 확률

    [Header("Special Event (Visuals)")]
    public bool isBlackoutDay;          // 정전 이벤트 여부
    [TextArea] public string dailyNews; // 신문 기사 내용
}