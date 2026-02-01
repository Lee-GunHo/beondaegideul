using UnityEngine;

public class EntrantVisual : MonoBehaviour
{
    public SpriteRenderer headRenderer;
    public SpriteRenderer bodyRenderer;

    public Sprite[] headLibrary; // 에디터에서 머리 이미지들을 넣어둠
    public Sprite[] bodyLibrary; // 에디터에서 몸통 이미지들을 넣어둠

    public void ApplyVisual(IdentificationData data)
    {
        // 인덱스에 맞는 이미지 적용
        if (data.Actual_HeadIndex < headLibrary.Length)
            headRenderer.sprite = headLibrary[data.Actual_HeadIndex];

        if (data.Actual_BodyIndex < bodyLibrary.Length)
            bodyRenderer.sprite = bodyLibrary[data.Actual_BodyIndex];
    }
}