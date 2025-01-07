using System.Collections;
using UnityEngine;

public class TriggerZone : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float newOrthographicSize = 7.5f;
    [SerializeField] private float newYPosition = 2.75f;
    [SerializeField] private float transitionDuration = 1.0f;
    [SerializeField] private BossTigerMovement bossTiger;

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
                finalCameraFollow.ChangeTarget();
            }
        }
    }



    //private IEnumerator AdjustCamera()
    //{
    //    float elapsedTime = 0f;
    //    float initialSize = mainCamera.orthographicSize;
    //    Vector3 initialPosition = mainCamera.transform.position;

    //    Vector3 targetPosition = new Vector3(initialPosition.x, newYPosition, initialPosition.z);

    //    while (elapsedTime < transitionDuration)
    //    {
    //        elapsedTime += Time.deltaTime;
    //        float t = elapsedTime / transitionDuration;

    //        // Lerp kích thước camera
    //        float newSize = Mathf.Lerp(initialSize, newOrthographicSize, t);
    //        mainCamera.orthographicSize = newSize;

    //        // Lerp vị trí camera
    //        mainCamera.transform.position = Vector3.Lerp(initialPosition, targetPosition, t);

    //        // Cập nhật giới hạn camera
    //        CameraFollow cameraFollow = mainCamera.GetComponent<CameraFollow>();
    //        if (cameraFollow != null)
    //        {
    //            cameraFollow.UpdateCameraBounds();
    //        }

    //        yield return null;
    //    }

    //    // Đảm bảo giá trị cuối cùng chính xác
    //    mainCamera.orthographicSize = newOrthographicSize;
    //    mainCamera.transform.position = targetPosition;

    //    // Cập nhật giới hạn camera lần cuối
    //    CameraFollow finalCameraFollow = mainCamera.GetComponent<CameraFollow>();
    //    if (finalCameraFollow != null)
    //    {
    //        finalCameraFollow.UpdateCameraBounds();
    //    }
    //}
}
