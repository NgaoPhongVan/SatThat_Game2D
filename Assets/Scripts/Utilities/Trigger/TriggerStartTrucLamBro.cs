using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerStartTrucLamBro : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    

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
                
            }

        }
    }
}
