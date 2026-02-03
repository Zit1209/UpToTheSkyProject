using UnityEngine;
using System.Collections;
using TMPro;

/// <summary>
/// Trigger zone that automatically displays and plays dialogue when player enters
/// Attach this to a GameObject with a trigger collider
/// </summary>
[RequireComponent(typeof(Collider))]
public class DialogueTriggerZone : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Canvas dialogueCanvas;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("Dialogue Content")]
    [TextArea(3, 10)]
    [SerializeField] private string[] dialogueLines;

    [Header("Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float typewriterSpeed = 0.05f;
    [SerializeField] private float lineDuration = 3f;
    [SerializeField] private bool playOnce = true;

    private bool hasTriggered = false;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;

        if (dialogueCanvas != null)
        {
            dialogueCanvas.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            if (playOnce && hasTriggered)
                return;

            hasTriggered = true;
            StartCoroutine(PlayDialogue());
        }
    }

    private IEnumerator PlayDialogue()
    {
        if (dialogueCanvas != null)
        {
            dialogueCanvas.gameObject.SetActive(true);
        }

        foreach (string line in dialogueLines)
        {
            yield return StartCoroutine(TypewriterEffect(line));
            yield return new WaitForSeconds(lineDuration);
        }

        if (dialogueCanvas != null)
        {
            dialogueCanvas.gameObject.SetActive(false);
        }
    }

    private IEnumerator TypewriterEffect(string fullText)
    {
        if (dialogueText == null)
            yield break;

        dialogueText.text = "";

        foreach (char letter in fullText)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typewriterSpeed);
        }
    }
}