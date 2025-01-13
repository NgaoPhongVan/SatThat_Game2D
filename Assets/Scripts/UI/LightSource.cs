using UnityEngine;

public class LightSource : MonoBehaviour
{
    [Header("Light Settings")]
    [SerializeField] private float radius = 5f;
    [SerializeField] private bool isDamaging = false;
    [SerializeField] private float damageAmount = 5f;
    [SerializeField] private float damageRate = 0.5f;

    [Header("Flicker Effect")]
    [SerializeField] private bool shouldFlicker = false;     // Bật/tắt hiệu ứng
    [SerializeField] private float flickerSpeed = 5f;        // Tốc độ nhấp nháy
    [SerializeField] private float flickerIntensity = 0.2f;  // Độ mạnh nhấp nháy

    private SpriteRenderer lightSprite;
    private float originalAlpha;
    private float lastDamageTime;
    private CircleCollider2D lightCollider;

    private void Start()
    {
        // Lấy component và lưu alpha gốc
        lightSprite = GetComponent<SpriteRenderer>();
        lightCollider = GetComponent<CircleCollider2D>();
        if (lightSprite != null)
        {
            originalAlpha = lightSprite.color.a;
        }

        // Thiết lập collider
        if (lightCollider != null)
        {
            lightCollider.radius = radius;
            lightCollider.isTrigger = true;
        }
    }

    private void Update()
    {
        if (shouldFlicker && lightSprite != null)
        {
            // Tạo hiệu ứng nhấp nháy bằng hàm Sin
            float flicker = Mathf.Sin(Time.time * flickerSpeed) * flickerIntensity;
            Color color = lightSprite.color;
            color.a = originalAlpha + flicker;
            lightSprite.color = color;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (isDamaging && other.CompareTag("Player"))
        {
            if (Time.time >= lastDamageTime + damageRate)
            {
                HealthSystem playerHealth = other.GetComponent<HealthSystem>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damageAmount);
                    lastDamageTime = Time.time;
                }
            }
        }
    }

    public bool IsInLight(Vector2 position)
    {
        return Vector2.Distance(transform.position, position) <= radius;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isDamaging ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}