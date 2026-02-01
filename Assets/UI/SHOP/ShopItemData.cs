using UnityEngine;

[CreateAssetMenu(fileName = "ShopItem", menuName = "Shop/Item")]
public class ShopItemData : ScriptableObject
{
    [Header("Data Link")]
    // CSV의 첫 번째 칸(1001 등)을 저장할 변수
    // 저장/불러오기 할 때 "몇 번 아이템 샀니?" 체크용으로 씁니다.
    public int itemId;

    public ShopItemType itemType; // 스캐너 업그레이드, 뇌물 등 기능 구분용

    [Header("Display")]
    public string itemName;     // 이름
    public Sprite itemImage;    // 이미지
    public int price;           // 가격

    // CSV의 설명(Desc) 칸을 저장할 변수
    [TextArea] // 인스펙터에서 여러 줄로 보이게 해줌
    public string description;
}