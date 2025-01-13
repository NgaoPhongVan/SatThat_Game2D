using System.Collections;
using UnityEngine;

public class TriggerZone : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private TigerBossController bossTiger;

    private bool isTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isTriggered)
        {
            isTriggered = true;
            //StartCoroutine(AdjustCamera());

            CameraChap2Controller finalCameraFollow = mainCamera.GetComponent<CameraChap2Controller>();

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
