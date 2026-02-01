using UnityEngine;
using System;
using System.Collections;

public class EntrantManager : MonoBehaviour
{
    public static EntrantManager Instance { get; private set; }

    [Header("Difficulty (Set by StageManager)")]
    public float humanProb = 0.1f;
    public float contradictProb = 0.1f;
    public float defectProb = 0.1f;

    [Header("Current Status")]
    public EntrantState currentState = EntrantState.None;
    public IdentificationData currentData;

    [Header("Visuals")]
    public SpriteRenderer headRenderer;
    public SpriteRenderer bodyRenderer;
    public Sprite[] headSprites;
    public Sprite[] bodySprites;
    public GameObject stainPrefab;
    public Transform bodyTransform;

    public static event Action<EntrantState> OnStateChanged;
    public static event Action<IdentificationData> OnNewEntrantLoaded;
    public static event Action<bool, DecisionType> OnDecisionResult;
    public static event Action OnGameOver;

    private int approvedHumanCount = 0;
    public int ApprovedHumanCount => approvedHumanCount;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // 게임 시작 시 첫 번째 로봇을 소환합니다.
        RequestSpawn();
    }

    // [중요] 리모컨 매니저가 데이터를 가져갈 수 있게 해주는 통로
    public IdentificationData GetCurrentData()
    {
        return currentData;
    }

    public void RequestSpawn() => StartCoroutine(SpawnSequence());

    private IEnumerator SpawnSequence()
    {
        // 1. 데이터 결정 (튜토리얼 우선 체크)
        if (TutorialManager.Instance != null && !TutorialManager.Instance.IsTutorialFinished())
        {
            currentData = TutorialManager.Instance.GetNextTutorialData();
        }
        else if (StageManager.Instance != null)
        {
            currentData = StageManager.Instance.GenerateEntrantData();
        }

        // 2. 상태 변경 및 비주얼 적용
        ChangeState(EntrantState.Approaching);

        if (currentData != null)
        {
            // [머리 50종 / 몸통 5종 대응] 배열 범위를 벗어나지 않도록 나머지 연산(%) 처리
            if (headSprites.Length > 0 && headRenderer != null)
            {
                int headIdx = currentData.Actual_HeadIndex % headSprites.Length;
                headRenderer.sprite = headSprites[headIdx];
            }

            if (bodySprites.Length > 0 && bodyRenderer != null)
            {
                int bodyIdx = currentData.Actual_BodyIndex % bodySprites.Length;
                bodyRenderer.sprite = bodySprites[bodyIdx];
            }

            if (currentData.HasStain && stainPrefab != null)
                Instantiate(stainPrefab, bodyTransform);
        }

        yield return new WaitForSeconds(1.5f);

        OnNewEntrantLoaded?.Invoke(currentData);
        ChangeState(EntrantState.Waiting);
    }

    public void ChangeState(EntrantState newState)
    {
        currentState = newState;
        OnStateChanged?.Invoke(newState);
    }

    // --- [핵심 수정] 승인/거절/폐기 3분할 판정 로직 ---
    public void ConfirmDecision(DecisionType playerDecision)
    {
        if (currentData == null) return;

        ChangeState(EntrantState.Decided);

        // [체크 항목 정의]
        bool isHuman = currentData.IsHumanMask || currentData.Actual_Sound == MachineSoundType.Heartbeat;
        bool isBroken = currentData.IsDefective;
        bool isForged = currentData.HasContradiction || (currentData.Actual_HeadIndex != currentData.Document_HeadIndex);
        bool hasStain = currentData.HasStain;

        // 하나라도 문제가 있는지 여부
        bool hasAnyProblem = isHuman || isBroken || isForged || hasStain;

        bool isCorrect = false;

        switch (playerDecision)
        {
            case DecisionType.Pass:
                // 인간을 통과시키면 즉시 게임오버
                if (isHuman) { TriggerGameOver(); return; }
                // 문제가 전혀 없는 정상 로봇이어야 정답
                isCorrect = !hasAnyProblem;
                break;

            case DecisionType.Reject:
                // 로봇인데 위조, 얼룩 등 "수정 가능한" 결함이 있는 경우 정답
                // (기획에 따라 인간을 거절했을 때 오답으로 칠지 결정: 여기서는 로봇일 때만 정답 처리)
                isCorrect = hasAnyProblem && !isHuman;
                break;

            case DecisionType.Discard:
                // 인간이거나, 로봇이라도 심각한 파손(isBroken) 상태면 폐기가 정답
                isCorrect = isHuman || isBroken;
                break;
        }

        // 결과 이벤트 발생 (UI 표시용)
        OnDecisionResult?.Invoke(isCorrect, playerDecision);

        if (StageManager.Instance != null)
            StageManager.Instance.AddProcessedCount();

        StartCoroutine(ExitAndSpawnNext());
    }

    public void TriggerGameOver()
    {
        Debug.Log("<color=red>[GAME OVER]</color> 인간을 통과시켰습니다.");
        OnGameOver?.Invoke();
        ChangeState(EntrantState.None);
    }

    private IEnumerator ExitAndSpawnNext()
    {
        ChangeState(EntrantState.Exiting);
        yield return new WaitForSeconds(1.5f);

        if (StageManager.Instance != null && !StageManager.Instance.isDayEnded)
            RequestSpawn();
    }

    public void ClearStains()
    {
        if (currentData == null || bodyTransform == null) return;

        currentData.HasStain = false;
        foreach (Transform child in bodyTransform)
        {
            if (child.GetComponent<Stain>()) Destroy(child.gameObject);
        }
    }

    public void OnClickPass()
    {
        ConfirmDecision(DecisionType.Pass);
    }

    public void OnClickReject()
    {
        ConfirmDecision(DecisionType.Reject);
    }

    public void OnClickDiscard()
    {
        ConfirmDecision(DecisionType.Discard);
    }
}