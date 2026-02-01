using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class PurchaseConfirmPopup : MonoBehaviour
{
    [Header("UI")]
    public GameObject root;     // 팝업 최상위 패널(없으면 자기 자신 사용)
    public Image icon;
    public TMP_Text nameText;
    public TMP_Text priceText;
    public TMP_Text messageText;

    [Header("Buttons")]
    public Button yesButton;
    public Button noButton;

    private ShopItemData currentItem;
    private Action<ShopItemData> onConfirm;

    void Awake()
    {
        //  1) 먼저 root 안전하게 세팅
        if (root == null) root = gameObject;

        //  2) 버튼 연결
        yesButton.onClick.RemoveAllListeners();
        yesButton.onClick.AddListener(OnYes);

        noButton.onClick.RemoveAllListeners();
        noButton.onClick.AddListener(OnNo);

        // 3) 시작할 땐 숨김
        Hide();
    }

    public void Show(ShopItemData item, Action<ShopItemData> confirmCallback = null)
    {
        if (item == null) return;

        currentItem = item;

        onConfirm = confirmCallback;

        // UI 갱신
        if (icon != null) icon.sprite = item.itemImage;
        if (nameText != null) nameText.text = item.itemName;
        if (priceText != null) priceText.text = item.price.ToString();

        // 메시지 갱신
        if (EconomyManager.Instance != null && EconomyManager.Instance.currentMoney < item.price)
        {
            if (messageText != null) messageText.text = "<color=red>잔액이 부족합니다.</color>";
            yesButton.interactable = false; // 구매 버튼 비활성화
        }
        else
        {
            if (messageText != null) messageText.text = "구매하시겠습니까?";
            yesButton.interactable = true;
        }

        root.SetActive(true);
        root.transform.SetAsLastSibling(); //  항상 맨 위로
    }

    public void Hide()
    {
        root.SetActive(false);
    }

    private void OnYes()
    {
        if (currentItem == null) return;

        // 매니저 없으면 중단
        if (EconomyManager.Instance == null || GlobalDataManager.Instance == null)
        {
            Debug.LogError("매니저(Economy 또는 GlobalData)가 씬에 없습니다!");
            Hide();
            return;
        }

        // 돈이 충분한지 확인.
        if (EconomyManager.Instance.currentMoney >= currentItem.price)
        {
            // 돈 차감
            EconomyManager.Instance.SubtractMoney(currentItem.price);

            // 아이템 저장 (GlobalData에 Enum 타입 전달)
            GlobalDataManager.Instance.AddItem(currentItem.itemType);

            Debug.Log($"[구매 성공] {currentItem.itemName} 구매 완료. 잔액: {EconomyManager.Instance.currentMoney}");

            // 팝업 닫기
            Hide();
        }
        else
        {
            Debug.Log("[구매 실패] 잔액 부족");
            // v UI 흔들기 효과등 효과 넣는 부분 코드 v
            
            // ^                                   ^
        }
    }

    private void OnNo()
    {
        Hide();
    }
}