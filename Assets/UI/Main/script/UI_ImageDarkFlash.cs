using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UI_ImageDarkFlash : MonoBehaviour
{
    [Header("Target Image")]
    public Image targetImage;          // 색 바꿀 이미지

    [Header("Dark Settings")]
    [Range(0f, 1f)]
    public float darkAmount = 0.5f;   // 어두워지는 정도 (0.5 = 절반 밝기)
    public float duration = 2f;       // 어두워져 있는 시간 (초)

    private Color originalColor;
    private bool isPlaying = false;

    void Awake()
    {
        if (targetImage == null)
            targetImage = GetComponent<Image>();

        originalColor = targetImage.color;
    }

    // 버튼 OnClick 연결
    public void PlayDarkFlash()
    {
       

        StartCoroutine(DarkFlash());
    }

    IEnumerator DarkFlash()
    {
        isPlaying = true;

        // 어둡게
        Color darkColor = originalColor * darkAmount;
        darkColor.a = originalColor.a; // 알파는 유지
        targetImage.color = darkColor;

        yield return new WaitForSeconds(duration);

        // 원래 색으로
        targetImage.color = originalColor;

        isPlaying = false;
    }
}