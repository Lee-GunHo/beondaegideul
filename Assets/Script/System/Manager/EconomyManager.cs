using UnityEngine;
using System;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }
    public int currentMoney = 0;
    public bool isTaxFreeToday = false; // 1004번 아이템으로 활성화
    public int rewardAmount;
    public int penaltyAmount;

    public int CurrentMoney => currentMoney;

    private void Awake() => Instance = this;
    private void OnEnable() => EntrantManager.OnDecisionResult += ProcessTransaction;
    private void OnDisable() => EntrantManager.OnDecisionResult -= ProcessTransaction;

    private void ProcessTransaction(bool isCorrect, DecisionType decision)
    {
        if (isCorrect) AddMoney(100); // 보상
        else
        {
            if (isTaxFreeToday)
            {
                Debug.Log("면제권 사용 중! 벌금이 취소됩니다.");
                isTaxFreeToday = false; // 사용 후 해제
            }
            else
            {
                SubtractMoney(50); // 벌금
            }
        }
    }

    public void AddMoney(int amount)
    {
        currentMoney += amount;
        if (GlobalDataManager.Instance != null) GlobalDataManager.Instance.UpdateMoney(amount);
    }

    public void SubtractMoney(int amount)
    {
        currentMoney = Mathf.Max(0, currentMoney - amount);
        if (GlobalDataManager.Instance != null) GlobalDataManager.Instance.UpdateMoney(-amount);
    }
}