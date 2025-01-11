using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Arrow : MonoBehaviour
{
    private float damage;
    private float speed;
    private bool initialized = false;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(float damage, float speed, bool facingRight)
    {
        this.damage = damage;
        this.speed = speed;
        initialized = true;

        // Xoay mũi tên theo hướng bắn
        transform.rotation = Quaternion.Euler(0, facingRight ? 0 : 180, 0);

        // Áp dụng vận tốc
        rb.velocity = (facingRight ? Vector2.right : Vector2.left) * speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!initialized) return;

        if (other.CompareTag("Player"))
        {
            PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
            HealthSystem playerHealth = other.GetComponent<HealthSystem>();

            if (playerHealth != null && playerMovement != null && playerMovement.CanTakeDamage())
            {
                playerHealth.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
        //else if (other.CompareTag("Ground"))
        //{
        //    // Tùy chọn: có thể để mũi tên găm vào tường một lúc rồi mới biến mất
        //    StartCoroutine(DestroyAfterDelay());
        //}
    }

    private System.Collections.IEnumerator DestroyAfterDelay()
    {
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }

    // Tự hủy sau một thời gian nếu không trúng gì
    private void Start()
    {
        Destroy(gameObject, 5f);
    }
}