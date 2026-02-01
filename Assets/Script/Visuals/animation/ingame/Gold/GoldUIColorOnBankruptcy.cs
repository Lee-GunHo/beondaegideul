using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MoneyUIColorOnBankruptcy : MonoBehaviour
{
    [Header("Refs (둘 중 하나만 넣어도 됨)")]
    [SerializeField] private TMP_Text tmpText;
    [SerializeField] private Text uiText;

    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color bankruptcyColor = Color.red;

    private void Reset()
    {
        tmpText = GetComponent<TMP_Text>();
        uiText = GetComponent<Text>();
    }

    private void OnEnable()
    {
        EndingManager.OnBankruptcyWarning += TurnRed;
    }

    private void OnDisable()
    {
        EndingManager.OnBankruptcyWarning -= TurnRed;
    }

    public void SetNormal()
    {
        if (tmpText != null) tmpText.color = normalColor;
        if (uiText != null) uiText.color = normalColor;
    }

    private void TurnRed()
    {
        if (tmpText != null) tmpText.color = bankruptcyColor;
        if (uiText != null) uiText.color = bankruptcyColor;
    }
}