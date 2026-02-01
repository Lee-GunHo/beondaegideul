using UnityEngine;
using System.Collections.Generic;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance { get; private set; }
    public List<int> inventoryList = new List<int>();

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    public void AddItem(int id) => inventoryList.Add(id);

    // [핵심] 아이템 ID별 기능 실행
    public void UseItem(int itemID, int slotIndex)
    {
        bool isUsed = false;

        switch (itemID)
        {
            case 1001: // 세척용 스프레이
                isUsed = UseWashSpray();
                break;
            case 1002: // 정품 부품 감별칩
                isUsed = UseRealCheckChip();
                break;
            case 1004: // 세금 면제권
                isUsed = UseTaxFreeTicket();
                break;
            case 1005: // 과부화된 예지몽
                isUsed = UsePredictorDream();
                break;
            case 1003: // 귀여운 고양이 로봇
                Debug.Log("굉장히 귀여운 고양이입니다");
                break;
            case 1006: // 누군가의 기계심장
                Debug.Log("수집용 아이템입니다. 장식장에 보관됩니다.");
                return; // 수집품은 소모되지 않음
        }

        if (isUsed)
        {
            inventoryList.RemoveAt(slotIndex);
            FindObjectOfType<ItemSlotUI>()?.UpdateItemSlots();
        }
    }

    // --- 세부 기능 함수들 ---

    // 1001: 스프레이 - 로봇의 얼룩 제거
    private bool UseWashSpray()
    {
        if (EntrantManager.Instance.currentData == null) return false;
        if (!EntrantManager.Instance.currentData.HasStain) return false;

        EntrantManager.Instance.ClearStains(); // EntrantManager에 구현된 지우기 호출
        return true;
    }

    // 1002: 감별칩 - 인간/로봇 판독
    private bool UseRealCheckChip()
    {
        var data = EntrantManager.Instance.currentData;
        if (data == null) return false;

        // 판독 로직: 인간 가면을 썼거나 심장 소리가 들리면 인간
        bool isHuman = data.IsHumanMask || data.Actual_Sound == MachineSoundType.Heartbeat;
        string msg = isHuman ? "<color=red>[경고] 생체 반응 감지!</color>" : "<color=cyan>[안전] 기계 장치 확인.</color>";

        Debug.Log(msg); // 실제 구현시 UI 텍스트로 연결
        return true;
    }

    // 1004: 세금 면제권 - 오늘 벌금 0원
    private bool UseTaxFreeTicket()
    {
        if (EconomyManager.Instance == null) return false;

        EconomyManager.Instance.isTaxFreeToday = true;
        Debug.Log("면제권 발동: 오늘 발생하는 모든 벌금이 취소됩니다.");
        return true;
    }

    // 1005: 과부화된 예지몽 - 오늘 인간 수 확인
    private bool UsePredictorDream()
    {
        if (StageManager.Instance == null) return false;

        // StageManager에서 오늘 생성할 데이터 중 IsHumanMask가 true인 것 카운트
        int humans = StageManager.Instance.GetHumanCountToday();
        Debug.Log($"예지몽: 오늘 당신을 찾아올 인간은 총 {humans}명입니다.");
        return true;
    }
}