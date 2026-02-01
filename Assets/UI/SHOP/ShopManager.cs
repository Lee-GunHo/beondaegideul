using UnityEngine;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    public List<ShopItemData> loadedItems = new List<ShopItemData>();

    private void Awake()
    {
        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
            // 상점이 여러 씬에서 유지되어야 한다면 DontDestroyOnLoad 추가
            // DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 상점 슬롯에서 구매 버튼을 눌렀을 때 호출
    public void TryPurchase(ShopItemData item)
    {
        if (EconomyManager.Instance.currentMoney >= item.price)
        {
            EconomyManager.Instance.SubtractMoney(item.price);
            ItemManager.Instance.AddItem(item.itemId); // ItemManager 인벤토리로 전달
            Debug.Log($"{item.itemName} 구매 성공!");
        }
        else Debug.Log("돈이 부족합니다.");
    }

    // ... 기존 CSV LoadShopDataFromCSV 로직 유지 ...
}