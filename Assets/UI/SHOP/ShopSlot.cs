using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopSlot : MonoBehaviour
{
    [Header("UI")]
    public Image icon;
    public TMP_Text nameText;
    public TMP_Text priceText;

    private ShopItemData item;
    private PurchaseConfirmPopup popup;   //  추가

    public void SetItem(ShopItemData data, PurchaseConfirmPopup purchasePopup)
    {
        item = data;
        popup = purchasePopup;

        if (icon) icon.sprite = item.itemImage;
        if (nameText) nameText.text = item.itemName;
        if (priceText) priceText.text = item.price.ToString();
    }

    // Button OnClick()에 연결할 함수
    public void OnClickSlot()
    {
        Debug.Log("슬롯 클릭됨!", this);
        Debug.Log($"itemNull={item == null}, popupNull={popup == null}", this);

        if (item == null || popup == null) return;

        popup.Show(item);  //  이제 PurchaseConfirmPopup.Show가 확실히 호출됨
    }
}
