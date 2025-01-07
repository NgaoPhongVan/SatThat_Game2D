using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform Player; // Player hoặc mục tiêu
    [SerializeField] private Transform BossTiger;
    [SerializeField] private float smoothSpeed = 0.125f;
    [SerializeField] private Vector3 offset = new Vector3(0f, 2f, -10f);

    [Header("Bounds (Map Size)")]
    [SerializeField] private float mapMinX = 0f;   // Giới hạn trái của map
    [SerializeField] private float mapMaxX = 108f; // Giới hạn phải của map
    [SerializeField] private float mapMinY = -10f; // Giới hạn dưới của map
    [SerializeField] private float mapMaxY = 1.6f;  // Giới hạn trên của map

    private float cameraHalfWidth;
    private float cameraHalfHeight;

    private Transform target;

    private void Start()
    {
        target = Player;
    }

    private void LateUpdate()
    {
        //ChangeTarget();

        if (target == null)
        {
            Debug.LogWarning("Camera target is not assigned!");
            return;
        }

        // Tính toán vị trí mong muốn
        Vector3 desiredPosition = target.position + offset;

        // Giới hạn vị trí camera
        desiredPosition.x = Mathf.Clamp(desiredPosition.x, mapMinX + cameraHalfWidth, mapMaxX - cameraHalfWidth);
        desiredPosition.y = Mathf.Clamp(desiredPosition.y, mapMinY + cameraHalfHeight, mapMaxY - cameraHalfHeight);

        // Di chuyển mượt đến vị trí mong muốn
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }

    public void ChangeTarget()
    {
        if (target == Player)
        {
            target = BossTiger;
        }
        else
        {
            target = Player;
        }
    }

    //public void UpdateCameraBounds(float newOrthographicSize)
    //{
    //    Camera cam = Camera.main;
    //    cameraHalfHeight = newOrthographicSize;
    //    cameraHalfWidth = cam.aspect * cameraHalfHeight;

    //    // Đảm bảo giới hạn camera không vượt ngoài map
    //    mapMinX = Mathf.Max(mapMinX, cameraHalfWidth);
    //    mapMaxX = Mathf.Min(mapMaxX, mapMaxX - cameraHalfWidth);
    //    mapMinY = Mathf.Max(mapMinY, cameraHalfHeight);
    //    mapMaxY = Mathf.Min(mapMaxY, mapMaxY - cameraHalfHeight);
    //}
}
