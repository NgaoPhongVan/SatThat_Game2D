using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxTrigger : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private Transform TrucLam;
    [SerializeField] private TrucLamController TrucLamController;
    [SerializeField] private Camera mainCamera;
    // Start is called before the first frame update
    private bool isTriggered = false;

    private void Start()
    {
        TrucLamController = TrucLam.GetComponent<TrucLamController>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isTriggered)
        {
            isTriggered = true;

            CameraChap2Controller finalCameraFollow = mainCamera.GetComponent<CameraChap2Controller>();

            if (finalCameraFollow != null)
            {
                finalCameraFollow.ChangeTarget("TrucLam");
                TrucLamController.ActiveDialogue();
            }



        }
    }


}
