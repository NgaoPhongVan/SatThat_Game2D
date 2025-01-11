using UnityEngine;

public class EnemyArcher : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float arrowDamage = 15f;
    [SerializeField] private float shootingCooldown = 5f;
    [SerializeField] private float arrowSpeed = 10f;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Arrow")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform shootPoint;

    private Transform player;
    private HealthSystem healthSystem;
    private Animator animator;
    private bool facingRight = true;
    private float lastShootTime;
    private bool isDead = false;
    private bool isHit = false;

    private void Start()
    {
        healthSystem = GetComponent<HealthSystem>();
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        if (healthSystem != null)
        {
            healthSystem.OnHit.AddListener(HandleHit);
            healthSystem.OnHealthChanged.AddListener(CheckHealth);
            healthSystem.OnDeath.AddListener(HandleDeath);
        }
    }

    private void Update()
    {
        if (isDead || isHit) return;

        CheckPlayerDetection();
    }

    private void CheckPlayerDetection()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            // Kiểm tra xem player đang ở bên trái hay phải của archer
            bool playerIsRight = player.position.x > transform.position.x;

            // Luôn quay về phía player khi phát hiện
            if (facingRight != playerIsRight)
            {
                UpdateFacingDirection(playerIsRight);
            }

            // Kiểm tra cooldown và bắn
            if (Time.time >= lastShootTime + shootingCooldown)
            {
                Shoot();
            }
        }
    }

    private void Shoot()
    {
        // Kích hoạt animation bắn
        animator.SetTrigger("shoot");
        lastShootTime = Time.time;
    }

    // Được gọi thông qua Animation Event khi animation bắn đến frame thích hợp
    public void SpawnArrow()
    {
        if (arrowPrefab != null && shootPoint != null)
        {
            GameObject arrow = Instantiate(arrowPrefab, shootPoint.position, shootPoint.rotation);
            Arrow arrowScript = arrow.GetComponent<Arrow>();
            if (arrowScript != null)
            {
                arrowScript.Initialize(arrowDamage, arrowSpeed, facingRight);
            }
        }
    }

    private void HandleHit()
    {
        if (!isDead)
        {
            isHit = true;
            animator.SetTrigger("hit");
            StartCoroutine(ResetHitState());
        }
    }

    private System.Collections.IEnumerator ResetHitState()
    {
        yield return new WaitForSeconds(0.3f);
        isHit = false;
    }

    private void CheckHealth(float healthPercentage)
    {
        if (healthPercentage <= 0 && !isDead)
        {
            HandleDeath();
        }
    }

    private void HandleDeath()
    {
        if (isDead) return;

        isDead = true;
        animator.SetTrigger("death");

        // Vô hiệu hóa các component
        enabled = false;
        if (GetComponent<Collider2D>() != null)
        {
            GetComponent<Collider2D>().enabled = false;
        }

        Destroy(gameObject, 1f);
    }

    private void UpdateFacingDirection(bool shouldFaceRight)
    {
        if (facingRight != shouldFaceRight)
        {
            facingRight = shouldFaceRight;
            transform.Rotate(0f, 180f, 0f);
            Debug.Log($"Archer turned to face {(shouldFaceRight ? "right" : "left")}");
        }
    }

    // Thêm debug để kiểm tra
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Vẽ hướng nhìn
        Gizmos.color = Color.red;
        Vector2 direction = facingRight ? Vector2.right : Vector2.left;
        Gizmos.DrawRay(transform.position, direction * 2f);
    }
}