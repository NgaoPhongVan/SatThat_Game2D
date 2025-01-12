using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ManaItem : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("Physics Settings")]
    [SerializeField] private float initialUpForce = 5f;
    [SerializeField] private float scatterForce = 2f;

    private Rigidbody2D rb;
    private CircleCollider2D triggerCollider; // Collider ?? phát hi?n pickup
    private CircleCollider2D physicsCollider; // Collider ?? x? lý va ch?m v?t lý

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        SetupColliders();
        ApplyInitialForce();
    }

    private void SetupColliders()
    {
        // T?o trigger collider ?? phát hi?n player
        triggerCollider = gameObject.AddComponent<CircleCollider2D>();
        triggerCollider.isTrigger = true;
        triggerCollider.radius = 0.1f; // ?i?u ch?nh bán kính cho phù h?p

        // T?o physics collider ?? va ch?m v?i ground
        physicsCollider = gameObject.AddComponent<CircleCollider2D>();
        physicsCollider.isTrigger = false;
        physicsCollider.radius = 0.05f; // Nh? h?n trigger collider m?t chút

        // Thi?t l?p Rigidbody2D
        rb.gravityScale = 1f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation; // Ng?n item xoay
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
            // Hi?n th? thông báo thông qua TextManager
            PickupTextManager.Instance.ShowPickupText("Bạn đã nhặt được vật phẩm hồi mana (Hãy bấm 2 để sử dụng)");

            // Thêm vào inventory
            InventoryManager.Instance.AddManaPotion();

            // H?y v?t ph?m
            Destroy(gameObject);
        }
    }
}
