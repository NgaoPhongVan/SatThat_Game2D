using System;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    
    [SerializeField] private float smoothSpeed = 0.125f;
    [SerializeField] private Vector3 offset = new Vector3(0f, 2f, -10f);

    //Thanh thay doi
    [SerializeField] private Transform Player; // Player hoặc mục tiêu
    [SerializeField] private Transform BossTiger;
    [SerializeField] private Transform PointSetUpCamera;
    private Transform target;

    [Header("Position Constraints")]
    [SerializeField] private float minX = 0f; // Giới hạn tối thiểu trục X
    [SerializeField] private float maxX = 108f; // Giới hạn tối đa trục X

    private void Start()
    {
        target = Player;
    }

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
        desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);

        // Di chuyển mượt đến vị trí đích
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }

    public void ChangeTarget(String a)
    {
        switch (a)
        {
            case "Player":
                target = Player;
                break;
            case "BossTiger":
                target = BossTiger;
                break;
            case "Point":
                target = PointSetUpCamera;
                break;
        }

    }

    public void ChangeSizeCamera(int newSize = 5)
    {
        Camera camera = Camera.main;
        camera.orthographicSize = newSize;
    }
}