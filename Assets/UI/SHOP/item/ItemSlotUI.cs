using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ItemSlotUI : MonoBehaviour
{
    [Header("인게임 UI 슬롯들 (순서대로 넣어주세요)")]
    public List<Button> slotButtons;
    public List<Image> iconImages;

    private void Start()
    {
        UpdateItemSlots();
    }

    public void UpdateItemSlots()
    {
        // 모든 슬롯 비활성화
        for (int i = 0; i < slotButtons.Count; i++)
        {
            slotButtons[i].gameObject.SetActive(false);
        }

        // 인벤토리 리스트에 있는 만큼 순서대로 활성화
        for (int i = 0; i < ItemManager.Instance.inventoryList.Count; i++)
        {
            if (i >= slotButtons.Count) break;

            int currentIndex = i;
            int itemID = ItemManager.Instance.inventoryList[i];

            slotButtons[currentIndex].gameObject.SetActive(true);

            // ShopManager가 로드한 데이터에서 아이콘 가져오기
            // (주의: ShopManager.Instance.loadedItems가 채워져 있어야 함)
            var itemData = ShopManager.Instance.loadedItems.Find(x => x.itemId == itemID);
            if (itemData != null)
            {
                iconImages[currentIndex].sprite = itemData.itemImage;
            }

            // 버튼 클릭 시 사용 함수 연결
            slotButtons[currentIndex].onClick.RemoveAllListeners();
            slotButtons[currentIndex].onClick.AddListener(() => ItemManager.Instance.UseItem(itemID, currentIndex));
        }
    }
}