using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoldButtonRandomResultOnlyWhileHolding : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [Header("Optional: Effect Image (shown while holding)")]
    public Image effectImage;

    [Header("Result Images (Size = 3)")]
    public Image[] resultImages;

    [Header("Optional Condition")]
    public GameObject conditionImage;

    private int currentIndex = -1;

    void Start()
    {
        HideAll();

        // 켜지는 이미지들이 입력을 가로채지 않게(중요)
        if (effectImage != null) effectImage.raycastTarget = false;

        if (resultImages != null)
        {
            foreach (var img in resultImages)
                if (img != null) img.raycastTarget = false;
        }
    }

    void Update()
    {
        // 기준 오브젝트가 꺼지면 전부 강제 OFF
        if (!gameObject.activeInHierarchy)
            HideAll();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!gameObject.activeInHierarchy)
            return;

        if (conditionImage != null && !conditionImage.activeInHierarchy)
            return;

        // (옵션) 이펙트 ON
        if (effectImage != null)
        {
            effectImage.gameObject.SetActive(true);
            effectImage.transform.SetAsLastSibling();
        }

        // 결과 3개 중 랜덤 1개만 ON
        ShowRandomResult();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        HideAll(); // 떼면 전부 OFF
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideAll(); // 누른 채로 밖으로 나가도 OFF
    }

    void ShowRandomResult()
    {
        HideResults();

        if (resultImages == null || resultImages.Length == 0)
            return;

        currentIndex = Random.Range(0, resultImages.Length);

        var img = resultImages[currentIndex];
        if (img != null)
        {
            img.gameObject.SetActive(true);
            img.transform.SetAsLastSibling();
        }
    }

    void HideResults()
    {
        if (resultImages == null) return;

        for (int i = 0; i < resultImages.Length; i++)
        {
            if (resultImages[i] != null)
                resultImages[i].gameObject.SetActive(false);
        }

        currentIndex = -1;
    }

    void HideAll()
    {
        if (effectImage != null)
            effectImage.gameObject.SetActive(false);

        HideResults();
    }
}