using UnityEngine;
using UnityEngine.UI;

public class ScrollViewController : MonoBehaviour
{
    public ScrollRect scrollRect;
    public float scrollSpeed = 0.1f;

    public void ScrollUp()
    {
        scrollRect.verticalNormalizedPosition += scrollSpeed;
        scrollRect.verticalNormalizedPosition = Mathf.Clamp01(scrollRect.verticalNormalizedPosition);
    }

    public void ScrollDown()
    {
        scrollRect.verticalNormalizedPosition -= scrollSpeed;
        scrollRect.verticalNormalizedPosition = Mathf.Clamp01(scrollRect.verticalNormalizedPosition);
    }
}