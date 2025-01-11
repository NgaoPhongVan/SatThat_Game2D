using UnityEngine;
using TMPro;
using System.Collections;

public class PickupTextManager : MonoBehaviour
{
    public static PickupTextManager Instance { get; private set; }
    [SerializeField] private TMP_Text pickupText;
    [SerializeField] private float displayTime = 2f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (pickupText != null)
        {
            pickupText.gameObject.SetActive(false);
        }
    }

    public void ShowPickupText(string message)
    {
        StopAllCoroutines();
        StartCoroutine(ShowPickupTextRoutine(message));
    }

    private IEnumerator ShowPickupTextRoutine(string message)
    {
        if (pickupText != null)
        {
            pickupText.text = message;
            pickupText.gameObject.SetActive(true);
            yield return new WaitForSeconds(displayTime);
            pickupText.gameObject.SetActive(false);
        }
    }
}