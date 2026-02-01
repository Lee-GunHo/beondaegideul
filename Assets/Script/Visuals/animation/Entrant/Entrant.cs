using UnityEngine;

public class Entrant : MonoBehaviour
{
    public Transform targetSlot;

    [SerializeField] private float moveSpeed = 2.0f;
    [SerializeField] private float arriveThreshold = 0.02f;

    private void Update()
    {
        if (targetSlot == null) return;

        Vector3 pos = transform.position;
        Vector3 target = targetSlot.position;
        target.z = pos.z;

        float dist = Vector2.Distance(pos, target);

        if (dist > arriveThreshold)
            transform.position = Vector3.MoveTowards(pos, target, moveSpeed * Time.deltaTime);
        else
            transform.position = target;
    }
}