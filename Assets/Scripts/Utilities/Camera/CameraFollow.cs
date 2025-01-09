using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothSpeed = 0.125f;
    [SerializeField] private Vector3 offset = new Vector3(0f, 2f, -10f);

    [Header("Position Constraints")]
    [SerializeField] private float minX = 0f; // Giới hạn tối thiểu trục X

    private void LateUpdate()
    {
        if (target == null)
        {
            Debug.LogWarning("Camera target is not assigned!");
            return;
        }

        // Tính toán vị trí đích
        Vector3 desiredPosition = target.position + offset;

        // Áp dụng giới hạn cho trục X
        desiredPosition.x = Mathf.Max(minX, desiredPosition.x);

        // Di chuyển mượt đến vị trí đích
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}