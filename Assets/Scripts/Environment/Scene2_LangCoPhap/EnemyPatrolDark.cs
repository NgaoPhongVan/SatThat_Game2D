using UnityEngine;

public class EnemyPatrolDark : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float patrolSpeed = 3f;
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackRate = 0.5f; // Tốc độ tấn công trong combo

    [Header("Patrol Settings")]
    [SerializeField] private float patrolDistance = 10f; // Khoảng cách tuần tra
    [SerializeField] private float waitTime = 5f; // Thời gian đứng yên tại điểm cuối

    [Header("Detection")]
    [SerializeField] private float detectionRange = 5f; // Tầm phát hiện
    [SerializeField] private LayerMask playerLayer; // Layer của player

    [Header("Drop Settings")]
    [SerializeField] private GameObject healthItemPrefab;
    [SerializeField] private float dropChance = 0.5f;

    private Vector2 startPosition;
    private Vector2 patrolEndPosition;
    private bool movingRight = true;
    private bool isChasing = false;
    private bool isAttacking = false;
    private bool isWaiting = false;
    private float waitTimer;
    private int comboCount = 0;
    private float lastAttackTime;

    private Transform player;
    private HealthSystem healthSystem;
    private Rigidbody2D rb;
    private bool facingRight = true;
    private Animator animator;
    private bool isDead = false;
    private bool isHit = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        healthSystem = GetComponent<HealthSystem>();
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        startPosition = transform.position;
        patrolEndPosition = startPosition + Vector2.right * patrolDistance;

        // Đăng ký sự kiện cho health system
        healthSystem.OnHealthChanged.AddListener(CheckHealth);
        healthSystem.OnDeath.AddListener(HandleDeath);

        if (healthSystem != null)
        {
            healthSystem.OnHit.AddListener(HandleHit);
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
        // Tạm dừng hành động hiện tại
        rb.velocity = Vector2.zero;
        animator.SetBool("isMoving", false);

        // Đợi animation hit kết thúc
        yield return new WaitForSeconds(0.3f);

        isHit = false;

        // Tiếp tục hành động trước đó nếu chưa chết
        if (!isDead)
        {
            animator.SetBool("isMoving", true);
        }
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
        rb.velocity = Vector2.zero;
        GetComponent<Collider2D>().enabled = false;
        enabled = false;

        // Thử thả vật phẩm
        TryDropHealthItem();

        // Xóa enemy
        Destroy(gameObject, 1f);
    }

    private void TryDropHealthItem()
    {
        if (healthItemPrefab != null && Random.value < dropChance)
        {
            Vector3 dropPosition = transform.position;
            Instantiate(healthItemPrefab, dropPosition, Quaternion.identity);
        }
    }

    private void Patrol()
    {
        animator.SetBool("isMoving", true);

        if (isWaiting)
        {
            animator.SetBool("isMoving", false);
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTime)
            {
                isWaiting = false;
                movingRight = !movingRight;
                UpdateFacingDirection(movingRight);
            }
            return;
        }

        Vector2 targetPosition = movingRight ? patrolEndPosition : startPosition;
        Vector2 moveDirection = (targetPosition - (Vector2)transform.position).normalized;
        rb.velocity = moveDirection * patrolSpeed;

        float distanceToTarget = Vector2.Distance(transform.position, targetPosition);
        if (distanceToTarget < 0.1f)
        {
            rb.velocity = Vector2.zero;
            isWaiting = true;
            waitTimer = 0;
            animator.SetBool("isMoving", false);
        }
    }

    private void ChasePlayer()
    {
        float distanceToStart = Vector2.Distance(transform.position, startPosition);
        if (distanceToStart > patrolDistance)
        {
            isChasing = false;
            ReturnToPatrol();
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        Vector2 directionToPlayer = ((Vector2)player.position - (Vector2)transform.position).normalized;

        if (distanceToPlayer <= 1.5f)
        {
            rb.velocity = Vector2.zero;
            UpdateFacingDirection(directionToPlayer.x > 0);

            if (Time.time >= lastAttackTime + attackRate)
            {
                StartCoroutine(PerformComboAttack());
            }
        }
        else
        {
            rb.velocity = directionToPlayer * chaseSpeed;
            UpdateFacingDirection(directionToPlayer.x > 0);
            animator.SetBool("isMoving", true);
        }
    }

    private void ReturnToPatrol()
    {
        Vector2 returnDirection = (startPosition - (Vector2)transform.position).normalized;
        rb.velocity = returnDirection * patrolSpeed;
        UpdateFacingDirection(returnDirection.x > 0);

        if (Vector2.Distance(transform.position, startPosition) < 0.1f)
        {
            transform.position = startPosition;
            rb.velocity = Vector2.zero;
            isChasing = false;
            UpdateFacingDirection(movingRight);
        }
    }

    private System.Collections.IEnumerator PerformComboAttack()
    {
        isAttacking = true;
        comboCount = 0;
        rb.velocity = Vector2.zero;
        animator.SetBool("isMoving", false);

        while (comboCount < 3)
        {
            animator.SetTrigger("attack");

            Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(
                transform.position + transform.right * 1f,
                1f,
                playerLayer
            );

            foreach (Collider2D hitPlayer in hitPlayers)
            {
                PlayerMovement playerMovement = hitPlayer.GetComponent<PlayerMovement>();
                HealthSystem playerHealth = hitPlayer.GetComponent<HealthSystem>();

                // Chỉ gây sát thương nếu player không trong trạng thái bất tử
                if (playerHealth != null && playerMovement != null && playerMovement.CanTakeDamage())
                {
                    playerHealth.TakeDamage(attackDamage);
                }
            }

            comboCount++;
            lastAttackTime = Time.time;
            yield return new WaitForSeconds(attackRate);
        }

        isAttacking = false;

        if (isChasing)
        {
            Vector2 directionToPlayer = ((Vector2)player.position - (Vector2)transform.position).normalized;
            UpdateFacingDirection(directionToPlayer.x > 0);
        }
        else
        {
            UpdateFacingDirection(movingRight);
        }

        animator.SetBool("isMoving", true);
    }

    private void UpdateFacingDirection(bool shouldFaceRight)
    {
        if (facingRight != shouldFaceRight)
        {
            facingRight = shouldFaceRight;
            transform.Rotate(0f, 180f, 0f);
        }
    }

    private void Update()
    {
        if (healthSystem.GetHealthPercentage() <= 0 || isHit) return;
        if (healthSystem.GetHealthPercentage() <= 0) return;

        // Kiểm tra phát hiện player
        CheckPlayerDetection();

        if (!isAttacking)
        {
            if (isChasing)
            {
                ChasePlayer();
            }
            else
            {
                Patrol();
            }
        }
    }

    private void CheckPlayerDetection()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Kiểm tra player trong tầm phát hiện
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            facingRight ? Vector2.right : Vector2.left,
            detectionRange,
            playerLayer
        );

        Vector2 directionToPlayer = ((Vector2)player.position - (Vector2)transform.position).normalized;
        float angle = Vector2.Angle(facingRight ? Vector2.right : Vector2.left, directionToPlayer);

        bool inDetectionRange = (hit.collider != null && hit.collider.CompareTag("Player")) ||
                              (distanceToPlayer <= detectionRange && angle < 90f);

        // Chỉ phát hiện nếu player trong tầm VÀ được chiếu sáng
        if (inDetectionRange && LightManager.Instance.IsPositionIlluminated(player.position))
        {
            isChasing = true;
            isWaiting = false;
        }
        else if (isChasing)
        {
            // Mất dấu player nếu họ ra khỏi vùng sáng
            if (!LightManager.Instance.IsPositionIlluminated(player.position))
            {
                isChasing = false;
                ReturnToPatrol();
            }
            // Hoặc nếu họ đi quá xa
            else
            {
                float distanceToStart = Vector2.Distance(transform.position, startPosition);
                if (distanceToStart > patrolDistance * 1.5f)
                {
                    isChasing = false;
                    ReturnToPatrol();
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        // Vẽ tầm phát hiện
        Gizmos.color = Color.yellow;
        Vector2 direction = facingRight ? Vector2.right : Vector2.left;
        Gizmos.DrawRay(transform.position, direction * detectionRange);

        // Vẽ đường tuần tra
        if (Application.isPlaying)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(startPosition, patrolEndPosition);
        }
    }

    private void OnDestroy()
    {
        if (healthSystem != null)
        {
            healthSystem.OnHealthChanged.RemoveListener(CheckHealth);
            healthSystem.OnDeath.RemoveListener(HandleDeath);
            healthSystem.OnHit.RemoveListener(HandleHit);
        }
    }
}