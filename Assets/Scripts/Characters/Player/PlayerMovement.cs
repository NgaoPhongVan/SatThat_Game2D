using UnityEngine;
using UnityEngine.SceneManagement;

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
    [SerializeField] private LayerMask bossLayer;
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
    [Header("Block Settings")]
    [SerializeField] private float blockDamageReduction = 0.3f;
    private bool isBlocking = false;
    private bool shouldResumeBlock = false;
    private bool isHealing = false;


    private SpriteRenderer spriteRenderer;
    private Transform currentBoat;
    public LayerMask boatMask;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        healthSystem = GetComponent<HealthSystem>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Đăng ký các event
        if (healthSystem != null)
        {
            healthSystem.OnHealthChanged.AddListener(CheckHealth);
            healthSystem.OnDeath.AddListener(HandleDeath);
            healthSystem.OnHit.AddListener(HandleHit);
        }
        // Lưu tên scene hiện tại để sử dụng cho retry
        PlayerPrefs.SetString("LastPlayedScene", SceneManager.GetActiveScene().name);
    }

    private void Update()
    {
        if (isDead) return;
        if (isHit) return;
        if (isHealing) return;
        CheckGrounded();
        HandleBlock();

        // Chỉ xử lý movement và attack nếu không đang block
        if (!isBlocking)
        {
            HandleMovement();
            HandleAttack();
        }
        HandleJumpAnimation();
        if (!isHealing)
        {
            HandleBlock();
            HandleMovement();
            HandleJumpAnimation();
            HandleAttack();
        }
    }

    private void FixedUpdate()
    {
        if (!isAttacking)
        {
            Move();
        }
    }

    private void HandleHit()
    {
        // Chỉ xử lý hit nếu nhân vật chưa chết và không trong trạng thái bất tử
        if (!isDead && !isInvulnerable)
        {
            isHit = true;  // Đánh dấu đang trong trạng thái bị đánh
            isInvulnerable = true;  // Kích hoạt trạng thái bất tử tạm thời

            // Lưu trạng thái block để khôi phục sau
            shouldResumeBlock = isBlocking && Input.GetKey(KeyCode.Space);

            // Chỉ chạy animation hit nếu không đang block
            if (!isBlocking)
            {
                animator.SetTrigger("hit");
            }

            // Khởi chạy 2 coroutine xử lý hiệu ứng
            StartCoroutine(HitStunCoroutine());      // Xử lý thời gian choáng
            StartCoroutine(InvulnerabilityCoroutine()); // Xử lý thời gian bất tử
        }
    }

    private System.Collections.IEnumerator HitStunCoroutine()
    {
        // Đợi trong thời gian hitStunTime
        yield return new WaitForSeconds(hitStunTime);

        isHit = false;  // Hết trạng thái bị đánh

        // Nếu người chơi vẫn giữ Space và trước đó đang block
        if (shouldResumeBlock && Input.GetKey(KeyCode.Space))
        {
            isBlocking = true;  // Khôi phục trạng thái block
            animator.SetBool("blocking", true);  // Khôi phục animation block
        }
    }

    private System.Collections.IEnumerator InvulnerabilityCoroutine()
    {
        // Hiệu ứng nhấp nháy khi bất tử
        float elapsedTime = 0f;

        // Xử lý khác nhau tùy theo đang block hay không
        if (!isBlocking)
        {
            // Tạo hiệu ứng nháy bằng cách bật/tắt sprite
            while (elapsedTime < invulnerableTime)
            {
                spriteRenderer.enabled = !spriteRenderer.enabled;  // Đảo trạng thái hiển thị
                yield return new WaitForSeconds(0.1f);  // Đợi 0.1s
                elapsedTime += 0.1f;
            }
            spriteRenderer.enabled = true;  // Đảm bảo sprite hiển thị lại
        }
        else
        {
            // Nếu đang block thì chỉ đợi hết thời gian bất tử
            yield return new WaitForSeconds(invulnerableTime);
        }

        isInvulnerable = false;  // Kết thúc trạng thái bất tử
    }

    private void CheckHealth(float healthPercentage)
    {
        // Kiểm tra nếu máu <= 0 và nhân vật chưa chết
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
        this.enabled = false;

        // Ẩn game UI
        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.ShowGameUI(false);
        }

        // Đợi animation death kết thúc rồi mới chuyển scene
        StartCoroutine(ShowGameOverAfterDeath());
    }

    private System.Collections.IEnumerator ShowGameOverAfterDeath()
    {
        // Đợi animation death kết thúc
        yield return new WaitForSeconds(1f);

        // Load Game Over scene
        SceneManager.LoadScene("GameOver");
    }

    // Thêm hàm xử lý animation event
    public void OnHealingStart()
    {
        isHealing = true;
        rb.velocity = Vector2.zero;
        Debug.Log("Healing Started");
    }

    public void OnHealingComplete()
    {
        isHealing = false;
        animator.SetBool("isHealing", false);
        Debug.Log("Healing Completed");
    }

    // Nếu animation bị gián đoạn hoặc không kết thúc đúng cách
    private void OnDisable()
    {
        isHealing = false;
    }

    public bool CanTakeDamage()
    {
        return !isInvulnerable && !isDead && !isHealing;
    }

    public float GetDamageReduction()
    {
        return isBlocking ? blockDamageReduction : 1f;
    }

    private void HandleMovement()
    {
        if (!isAttacking && !isBlocking && !isHealing)
        {
            horizontalInput = Input.GetAxisRaw("Horizontal");
            animator.SetBool("isRunning", Mathf.Abs(horizontalInput) > 0);

            if (Input.GetKeyDown(KeyCode.W) && isGrounded)
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

    private void Move()
    {
        Vector2 moveVelocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);
        rb.velocity = moveVelocity;
    }

    private void Flip()
    {
        facingRight = !facingRight;
        transform.Rotate(0f, 180f, 0f);
    }

    private void HandleBlock()
    {
        // Nếu đang hit stun, không cho phép bắt đầu block mới
        if (isHit) return;

        if (Input.GetKey(KeyCode.Space))
        {
            if (!isBlocking && !isAttacking)
            {
                StartBlocking();
            }
        }
        else if (isBlocking)
        {
            EndBlocking();
        }
    }

    private void StartBlocking()
    {
        isBlocking = true;
        shouldResumeBlock = true;
        animator.SetBool("blocking", true);
        animator.SetTrigger("blockStart");
    }

    private void EndBlocking()
    {
        isBlocking = false;
        shouldResumeBlock = false;
        animator.SetBool("blocking", false);
        animator.SetTrigger("blockEnd");
    }

    private void HandleJumpAnimation()
    {
        verticalVelocity = rb.velocity.y;
        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Kiểm tra trạng thái
        if (isGrounded)
        {
            animator.SetBool("isJumping", false);
            animator.SetBool("isFalling", false);
            animator.SetBool("isGrounded", true);
        }
        else if (verticalVelocity > 0.1f) // Vận tốc dương = đang đi lên
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
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Cham be");
        if (other.CompareTag("boat"))
        {
            currentBoat = other.transform;
            transform.SetParent(currentBoat);
            isGrounded = true;

        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!gameObject.activeSelf) { return; }
        if (currentBoat != null && collision.CompareTag("boat"))
        {
            transform.SetParent(null);
            currentBoat = null;
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

        Collider2D[] hitBoss = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRange,
            bossLayer
            );
        foreach (Collider2D boss in hitBoss)
        {
            HealthSystem bossHealth = boss.GetComponent<HealthSystem>();
            if (bossHealth != null && !bossHealth.gameObject.GetComponent<BossController>().isDefending)
            {
                // Debug.Log($"Dealing damage to {enemy.name}"); // Debug line
                bossHealth.TakeDamage(attackDamage);
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

    private void Jump()
    {
        if (isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            animator.SetTrigger("jump");
            isGrounded = false;
        }
    }

    private void CheckGrounded()
    {
        // Chỉ cập nhật trạng thái isGrounded
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
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
        // Hủy đăng ký các event khi object bị hủy
        if (healthSystem != null)
        {
            healthSystem.OnHealthChanged.RemoveListener(CheckHealth);
            healthSystem.OnDeath.RemoveListener(HandleDeath);
            healthSystem.OnHit.RemoveListener(HandleHit);
        }
    }
}