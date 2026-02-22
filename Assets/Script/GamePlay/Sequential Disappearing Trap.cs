using UnityEngine;
using System.Collections;

/// <summary>
/// Trap zone that makes objects disappear and reappear sequentially
/// Attach this to a GameObject with a trigger collider
/// </summary>
[RequireComponent(typeof(Collider))]
public class SequentialDisappearingTrapZone : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string playerTag = "Player";

    [Header("Objects to Disappear")]
    [SerializeField] private GameObject[] objectsToDisappear;

    [Header("Timing")]
    [SerializeField] private float disappearInterval = 0.5f;
    [SerializeField] private float reappearInterval = 0.5f;
    [SerializeField] private float delayBeforeReappear = 2f;

    [Header("Options")]
    [SerializeField] private bool useRenderer = false;
    [SerializeField] private bool triggerOnce = false;

    private bool hasTriggered = false;
    private bool isSequenceRunning = false;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            if (triggerOnce && hasTriggered)
                return;

            if (isSequenceRunning)
                return;

            hasTriggered = true;
            StartCoroutine(DisappearReappearSequence());
        }
    }

    private IEnumerator DisappearReappearSequence()
    {
        isSequenceRunning = true;

        // Disappear phase
        for (int i = 0; i < objectsToDisappear.Length; i++)
        {
            if (objectsToDisappear[i] != null)
            {
                SetObjectVisible(objectsToDisappear[i], false);
                yield return new WaitForSeconds(disappearInterval);
            }
        }

        // Wait before reappearing
        yield return new WaitForSeconds(delayBeforeReappear);

        // Reappear phase
        for (int i = 0; i < objectsToDisappear.Length; i++)
        {
            if (objectsToDisappear[i] != null)
            {
                SetObjectVisible(objectsToDisappear[i], true);
                yield return new WaitForSeconds(reappearInterval);
            }
        }

        isSequenceRunning = false;
    }

    private void SetObjectVisible(GameObject obj, bool visible)
    {
        if (useRenderer)
        {
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.enabled = visible;
            }
        }
        else
        {
            obj.SetActive(visible);
        }
    }

    /// <summary>
    /// Manually trigger the sequence
    /// </summary>
    public void TriggerSequence()
    {
        if (!isSequenceRunning)
        {
            StartCoroutine(DisappearReappearSequence());
        }
    }

    /// <summary>
    /// Reset the trigger state
    /// </summary>
    public void ResetTrigger()
    {
        hasTriggered = false;
    }
}