using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private bool facingRight = true;

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private Transform groundCheck;

    [Header("Attack Settings")]
    [SerializeField] private float attackDamage = 20f;
    [SerializeField] private float attackRate = 0.5f; // Thời gian giữa các đòn đánh
    [SerializeField] private float attackRange = 1f; // Tầm đánh
    [SerializeField] private Transform attackPoint; // Điểm xuất phát đòn đánh
    [SerializeField] private LayerMask enemyLayer; // Layer của enemy

    private Rigidbody2D rb;
    private Animator animator;
    private float horizontalInput;
    private bool isGrounded;
    private float lastAttackTime;
    private bool isAttacking;
    private float verticalVelocity;
    private bool wasGrounded; // Để kiểm tra trạng thái trước đó
    private bool isDead = false;
    private HealthSystem playerHealth;
    private bool isHit = false;
    private HealthSystem healthSystem;
    [Header("Hit Settings")]
    [SerializeField] private float invulnerableTime = 1f; // Thời gian bất tử sau khi bị hit
    [SerializeField] private float hitStunTime = 0.2f; // Thời gian không điều khiển được khi bị hit
    private bool isInvulnerable = false;


    private SpriteRenderer spriteRenderer;
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        healthSystem = GetComponent<HealthSystem>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (healthSystem != null)
        {
            healthSystem.OnHit.AddListener(HandleHit);
        }
    }

    private void HandleHit()
    {
        if (!isDead && !isInvulnerable)
        {
            isHit = true;
            isInvulnerable = true;
            animator.SetTrigger("hit");
            StartCoroutine(HitStunCoroutine());
            StartCoroutine(InvulnerabilityCoroutine());
        }
    }

    private System.Collections.IEnumerator HitStunCoroutine()
    {
        // Chỉ bị choáng trong thời gian ngắn
        yield return new WaitForSeconds(hitStunTime);
        isHit = false;
    }

    private System.Collections.IEnumerator InvulnerabilityCoroutine()
    {
        // Hiệu ứng nhấp nháy khi bất tử
        float elapsedTime = 0f;
        while (elapsedTime < invulnerableTime)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(0.1f);
            elapsedTime += 0.1f;
        }

        spriteRenderer.enabled = true;
        isInvulnerable = false;
    }


    private System.Collections.IEnumerator ResetHitState()
    {
        // Đợi animation hit kết thúc
        yield return new WaitForSeconds(0.5f); // Điều chỉnh thời gian phù hợp với độ dài animation
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

        // Vô hiệu hóa các thao tác điều khiển
        rb.velocity = Vector2.zero;
        enabled = false; // Tắt script này

        //// Tùy chọn: Vô hiệu hóa collider
        //if (GetComponent<Collider2D>() != null)
        //{
        //    GetComponent<Collider2D>().enabled = false;
        //}
    }

    private void Update()
    {
        if (isDead) return;

        // Chỉ không điều khiển được trong thời gian hit stun ngắn
        if (isHit) return;

        CheckGrounded();
        HandleMovement();
        HandleJumpAnimation();
        HandleAttack();
    }

    public bool CanTakeDamage()
    {
        return !isInvulnerable && !isDead;
    }

    private void HandleMovement()
    {
        // Chỉ cho phép di chuyển khi không đang tấn công
        if (!isAttacking)
        {
            horizontalInput = Input.GetAxisRaw("Horizontal");
            animator.SetBool("isRunning", Mathf.Abs(horizontalInput) > 0);

            if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            {
                Jump();
            }

            if (horizontalInput > 0 && !facingRight)
            {
                Flip();
            }
            else if (horizontalInput < 0 && facingRight)
            {
                Flip();
            }
        }
    }

    private void HandleJumpAnimation()
    {
        verticalVelocity = rb.velocity.y;

        // Chạm đất
        if (isGrounded)
        {
            animator.SetBool("isJumping", false);
            animator.SetBool("isFalling", false);
            animator.SetBool("isGrounded", true);
        } 
        // Đang nhảy lên
        else if (verticalVelocity > 0.1f)
        {
            animator.SetBool("isJumping", true);
            animator.SetBool("isFalling", false);
            animator.SetBool("isGrounded", false);
        }
        // Đang rơi xuống
        else if (verticalVelocity < -0.1f)
        {
            animator.SetBool("isJumping", false);
            animator.SetBool("isFalling", true);
            animator.SetBool("isGrounded", false);
        }
    }

    private void HandleAttack()
    {
        if (Input.GetMouseButtonDown(0) && Time.time >= lastAttackTime + attackRate && !isAttacking)
        {
            Attack();
        }
    }

    private void Attack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        animator.SetTrigger("attack");

        // Đảm bảo không di chuyển khi đang tấn công
        rb.velocity = Vector2.zero;

        // Phát hiện và gây sát thương cho enemy
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRange,
            enemyLayer
        );

        // Debug.Log($"Detected {hitEnemies.Length} enemies in range"); // Debug line

        foreach (Collider2D enemy in hitEnemies)
        {
            HealthSystem enemyHealth = enemy.GetComponent<HealthSystem>();
            if (enemyHealth != null)
            {
                // Debug.Log($"Dealing damage to {enemy.name}"); // Debug line
                enemyHealth.TakeDamage(attackDamage);
            }
        }

        // Sử dụng Animation Event thay vì Coroutine
        // Animation Event sẽ gọi OnAttackComplete khi animation kết thúc
    }

    // Thêm phương thức này để gọi từ Animation Event
    public void OnAttackComplete()
    {
        isAttacking = false;
        animator.SetTrigger("attackComplete");
    }

    private System.Collections.IEnumerator ResetAttack()
    {
        // Đợi animation attack kết thúc (điều chỉnh thời gian phù hợp với độ dài animation)
        yield return new WaitForSeconds(0.5f);
        isAttacking = false;
    }

    private void FixedUpdate()
    {
        if (!isAttacking)
        {
            Move();
        }
    }


    private void Move()
    {
        Vector2 moveVelocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);
        rb.velocity = moveVelocity;
    }

    private void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
    }

    private void CheckGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void Flip()
    {
        facingRight = !facingRight;
        transform.Rotate(0f, 180f, 0f);
    }


    // Vẽ Gizmos để debug tầm đánh
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    } 

    private void OnDestroy()
    {
        // Hủy đăng ký sự kiện khi object bị hủy
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged.RemoveListener(CheckHealth);
            playerHealth.OnDeath.RemoveListener(HandleDeath);
        }
        if (healthSystem != null)
        {
            healthSystem.OnHit.RemoveListener(HandleHit);
        }
    }
}