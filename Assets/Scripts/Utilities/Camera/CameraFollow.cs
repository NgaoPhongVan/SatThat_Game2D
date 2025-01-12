using System;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    public Transform target;
    [SerializeField] private float smoothSpeed = 0.125f;
    [SerializeField] private Vector3 offset = new Vector3(0, 2, -10);

    //Thanh thay doi
    [SerializeField] private Transform Player; // Player hoặc mục tiêu
    [SerializeField] private Transform BossTiger;
    [SerializeField] private Transform PointSetUpCamera;

    [Header("Position Constraints")]
    [SerializeField] private float minX = 0f;
    [SerializeField] private float maxX = 155f;

    private void Start()
    {
        target = Player;
    }


    private void LateUpdate()
    {
        if (target == null) return;

        // Tính toán vị trí mới cho camera
        Vector3 desiredPosition = target.position + offset;

        // Giới hạn vị trí X
        desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);

        // Làm mượt chuyển động camera
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }

    //public void ChangeTarget(String a)
    //{
    //    switch (a)
    //    {
    //        case "Player":
    //            target = Player;
    //            break;
    //        case "BossTiger":
    //            target = BossTiger;
    //            break;
    //        case "Point":
    //            target = PointSetUpCamera;
    //            break;
    //    }

    //}

    public void ChangeSizeCamera(int newSize = 5)
    {
        Camera camera = Camera.main;
        camera.orthographicSize = newSize;
    }
    // Phương thức để thay đổi target
    public void ChangeTarget(Transform newTarget)
    {
        target = newTarget;
    }
}

