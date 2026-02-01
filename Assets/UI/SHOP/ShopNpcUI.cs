using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class ShopNpcUi : MonoBehaviour, IPointerClickHandler
{
    [Header("UI")]
    public TextMeshProUGUI dialogueText;

    [TextArea]
    public List<string> dialogues = new List<string>();

    public void OnPointerClick(PointerEventData eventData)
    {
        if (dialogues == null || dialogues.Count == 0 || dialogueText == null) return;

        int rand = Random.Range(0, dialogues.Count);
        dialogueText.gameObject.SetActive(true);
        dialogueText.text = dialogues[rand];
    }
    public void SpeakRandom()
    {
        Debug.Log("NPC Clicked!");

        if (dialogues == null || dialogues.Count == 0 || dialogueText == null)
        {
            Debug.Log($"dialoguesCount={(dialogues == null ? -1 : dialogues.Count)}, dialogueTextNull={(dialogueText == null)}");
            return;
        }

        int rand = Random.Range(0, dialogues.Count);
        dialogueText.gameObject.SetActive(true);
        dialogueText.text = dialogues[rand];
    }
}