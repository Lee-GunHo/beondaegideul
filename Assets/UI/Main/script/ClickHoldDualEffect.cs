using UnityEngine;
using UnityEngine.EventSystems;

public class ClickHoldDualEffect : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [Header("Condition Image")]
    public GameObject conditionImage;   // 이 이미지가 켜져 있어야 작동

    [Header("Effects (2개)")]
    public GameObject effectA;
    public GameObject effectB;

    private bool isClicking = false;

    void Start()
    {
        if (effectA) effectA.SetActive(false);
        if (effectB) effectB.SetActive(false);
    }

    // 마우스 클릭 시작
    public void OnPointerDown(PointerEventData eventData)
    {
        if (conditionImage == null || !conditionImage.activeInHierarchy)
            return;

        isClicking = true;

        if (effectA) effectA.SetActive(true);
        if (effectB) effectB.SetActive(true);
    }

    // 마우스 클릭 종료
    public void OnPointerUp(PointerEventData eventData)
    {
        StopEffects();
    }

    // 클릭한 채로 버튼 밖으로 나갔을 때도 끄기
    public void OnPointerExit(PointerEventData eventData)
    {
        StopEffects();
    }

    private void StopEffects()
    {
        if (!isClicking) return;

        isClicking = false;

        if (effectA) effectA.SetActive(false);
        if (effectB) effectB.SetActive(false);
    }
}