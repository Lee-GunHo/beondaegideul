using UnityEngine;

public class RobotSpriteRandom : MonoBehaviour
{
    [Header("Renderers")]
    [SerializeField] private SpriteRenderer bodyRenderer;
    [SerializeField] private SpriteRenderer headRenderer;
    [SerializeField] private SpriteRenderer dustRenderer;

    [Header("Sprite Pools")]
    [SerializeField] private Sprite[] bodySprites;
    [SerializeField] private Sprite[] headSprites;

    private void Awake()
    {
        // 인스펙터 연결 안 했으면 자동으로 찾아줌(구조가 고정일 때 편함)
        if (bodyRenderer == null)
            bodyRenderer = transform.Find("body")?.GetComponent<SpriteRenderer>();

        if (headRenderer == null)
            headRenderer = transform.Find("head")?.GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        Randomize();
    }

    public void Randomize()
    {
        if (bodyRenderer != null && bodySprites != null && bodySprites.Length > 0)
            bodyRenderer.sprite = bodySprites[Random.Range(0, bodySprites.Length)];

        if (headRenderer != null && headSprites != null && headSprites.Length > 0)
            headRenderer.sprite = headSprites[Random.Range(0, headSprites.Length)];
    }
    public void SetDust(bool active)
    {
        if (dustRenderer != null)
            dustRenderer.enabled = active;
    }
}