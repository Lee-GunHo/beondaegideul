using UnityEngine;
using UnityEngine.UI;

public class ScrollView : MonoBehaviour
{
    public ScrollRect scrollRect;
    public float speed = 0.2f;

    void Update()
    {
        float wheel = Input.GetAxis("Mouse ScrollWheel");
        scrollRect.verticalNormalizedPosition += wheel * speed;
        scrollRect.verticalNormalizedPosition = Mathf.Clamp01(scrollRect.verticalNormalizedPosition);
    }
}