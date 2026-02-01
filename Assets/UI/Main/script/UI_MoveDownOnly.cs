using UnityEngine;

public class UI_MoveDownOnly : MonoBehaviour
{
    [Header("Move Target")]
    public RectTransform targetImage;   // 움직일 UI 이미지

    [Header("Move Amount")]
    public float moveDown = 200f;       // 아래로 이동할 거리 (양수로 입력)

    private Vector2 startPos;
    private bool moved = false;

    void Awake()
    {
        if (targetImage == null)
            targetImage = GetComponent<RectTransform>();

        startPos = targetImage.anchoredPosition;
    }

    // 버튼 OnClick 연결
    public void MoveDown()
    {
        // 이미 내려갔으면 다시 안 움직이게 (원하면 삭제 가능)
        

        targetImage.anchoredPosition =
            new Vector2(startPos.x, startPos.y - moveDown);

        moved = true;
    }
}