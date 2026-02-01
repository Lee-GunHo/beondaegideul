using UnityEngine;

public class ImageToggleByButtons : MonoBehaviour
{
    [Header("Target Image")]
    public GameObject targetImage;   // 켜고/끌 이미지(GameObject)

    void Start()
    {
        if (targetImage != null)
            targetImage.SetActive(false); // 시작할 때 꺼두기
    }

    // 버튼 A OnClick()
    public void ShowImage()
    {
        if (targetImage == null) return;

        targetImage.SetActive(true);
    }

    // 버튼 B OnClick()
    public void HideImage()
    {
        if (targetImage == null) return;

        targetImage.SetActive(false);
    }
}