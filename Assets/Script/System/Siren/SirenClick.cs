using UnityEngine;

public class SirenClick : MonoBehaviour
{
    [SerializeField] private DayStartAndSceneLoader dayController;
    [SerializeField] private float clickCooldown = 0.2f;

    private float lastClickTime = -999f;

    private void Awake()
    {
        if (dayController == null)
            dayController = FindFirstObjectByType<DayStartAndSceneLoader>();
    }

    private void OnMouseDown()
    {
        if (Time.time - lastClickTime < clickCooldown) return;
        lastClickTime = Time.time;

        if (dayController != null)
            dayController.StartDayBySiren();
    }
}