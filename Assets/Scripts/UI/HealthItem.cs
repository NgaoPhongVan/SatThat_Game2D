using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class HealthItem : MonoBehaviour
{
    [Header("Physics Settings")]
    [SerializeField] private float initialUpForce = 5f;
    [SerializeField] private float scatterForce = 2f;

    private Rigidbody2D rb;
    private CircleCollider2D triggerCollider; // Collider để phát hiện pickup
    private CircleCollider2D physicsCollider; // Collider để xử lý va chạm vật lý

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        SetupColliders();
        ApplyInitialForce();
    }

    private void SetupColliders()
    {
        // Tạo trigger collider để phát hiện player
        triggerCollider = gameObject.AddComponent<CircleCollider2D>();
        triggerCollider.isTrigger = true;
        triggerCollider.radius = 0.1f; // Điều chỉnh bán kính cho phù hợp

        // Tạo physics collider để va chạm với ground
        physicsCollider = gameObject.AddComponent<CircleCollider2D>();
        physicsCollider.isTrigger = false;
        physicsCollider.radius = 0.05f; // Nhỏ hơn trigger collider một chút

        // Thiết lập Rigidbody2D
        rb.gravityScale = 1f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation; // Ngăn item xoay
    }

    private void ApplyInitialForce()
    {
        float randomDirection = Random.Range(-1f, 1f);
        Vector2 force = new Vector2(randomDirection * scatterForce, initialUpForce);
        rb.AddForce(force, ForceMode2D.Impulse);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Hiển thị thông báo thông qua TextManager
            PickupTextManager.Instance.ShowPickupText("Bạn đã nhặt được vật phẩm hồi máu (Nhấn 1 để sử dụng)");

            // Thêm vào inventory
            InventoryManager.Instance.AddHealthPotion();

            // Hủy vật phẩm
            Destroy(gameObject);
        }
    }
}