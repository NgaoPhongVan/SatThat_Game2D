using System.Collections;
using UnityEngine;

public class TriggerZone : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float newOrthographicSize = 7.5f;
    [SerializeField] private float newYPosition = 2.75f;
    [SerializeField] private float transitionDuration = 1.0f;
    [SerializeField] private TigerBossController bossTiger;

    private bool isTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isTriggered)
        {
            isTriggered = true;
            //StartCoroutine(AdjustCamera());

            CameraFollow finalCameraFollow = mainCamera.GetComponent<CameraFollow>();
            
            if (finalCameraFollow != null)
            {
                finalCameraFollow.ChangeTarget("BossTiger");
                if (bossTiger == null)
                {
                    bossTiger = FindObjectOfType<TigerBossController>();
                }
                bossTiger.ActiveBoss();
            }
        }
    }

}
