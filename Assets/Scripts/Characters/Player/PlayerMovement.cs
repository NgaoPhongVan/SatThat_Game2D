
using System.Collections;
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
    [SerializeField] private float heavyAttackDamge = 30f;
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
    [Header("Block Settings")]
    [SerializeField] private float blockDamageReduction = 0.3f;
    private bool isBlocking = false;
    private bool shouldResumeBlock = false;
    private bool isHealing = false;

    private ManaSystem manaSystem;
    private bool outOfMana = false;
    private bool isBuff = false;
    private float currentMana;
    private bool isManaRecovering = false;
    private Coroutine buffCoroutine;


    private SpriteRenderer spriteRenderer;
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

        manaSystem = GetComponent<ManaSystem>();
        //Dk event cho mana
        if(manaSystem != null)
        {
            manaSystem.OnManaChanged.AddListener(CheckMana);
            manaSystem.OutOfMana.AddListener(OutOfMana);
        }
        // Lưu tên scene hiện tại để sử dụng cho retry
        PlayerPrefs.SetString("LastPlayedScene", SceneManager.GetActiveScene().name);

    }

    private void Update()
    {
        if (isDead) return;
        if (isHit) return;
        if (isHealing) return;

        currentMana = manaSystem.getCurrentMana();

        CheckGrounded();
        HandleBlock();

        // Chỉ xử lý movement và attack nếu không đang block
        if (!isBlocking)
        {
            HandleMovement();
            HandleAttack();
            HandleHeavyAttack();
            HandleUseBuff();
        }
        HandleJumpAnimation();
        if (!isHealing)
        {
            HandleBlock();
            HandleMovement();
            HandleJumpAnimation();
            HandleAttack();
            HandleHeavyAttack();
            HandleUseBuff();
        }
    }

    private void HandleHit()
    {
        if (!isDead && !isInvulnerable)
        {
            isHit = true;
            isInvulnerable = true;

            // Lưu trạng thái block nếu đang block
            shouldResumeBlock = isBlocking && Input.GetKey(KeyCode.Space);

            // Không set trigger hit nếu đang block
            if (!isBlocking)
            {
                animator.SetTrigger("hit");
            }
            animator.SetTrigger("hit");
            StartCoroutine(HitStunCoroutine());
            StartCoroutine(InvulnerabilityCoroutine());
        }
    }

    private System.Collections.IEnumerator HitStunCoroutine()
    {
        yield return new WaitForSeconds(hitStunTime);
        isHit = false;

        // Khôi phục block nếu vẫn đang giữ Space
        if (shouldResumeBlock && Input.GetKey(KeyCode.Space))
        {
            isBlocking = true;
            animator.SetBool("blocking", true);
        }
    }

    private System.Collections.IEnumerator InvulnerabilityCoroutine()
    {
        float elapsedTime = 0f;

        // Nếu không đang block thì mới chớp nháy
        if (!isBlocking)
        {
            while (elapsedTime < invulnerableTime)
            {
                //animator.SetTrigger("hit");
                spriteRenderer.enabled = !spriteRenderer.enabled;
                yield return new WaitForSeconds(0.1f);
                elapsedTime += 0.1f;
            }
            spriteRenderer.enabled = true;
        }
        else
        {
            // Nếu đang block thì chỉ đợi thời gian bất tử
            yield return new WaitForSeconds(invulnerableTime);
        }

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

    private void HandleDeath()
    {
        if (isDead) return;

        isDead = true;
        animator.SetTrigger("death");

        // Vô hiệu hóa các thao tác điều khiển
        rb.velocity = Vector2.zero;
        this.enabled = false;

        //// Vô hiệu hóa collider
        //if (GetComponent<Collider2D>() != null)
        //{
        //    GetComponent<Collider2D>().enabled = false;
        //}

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
    // Sửa lại phương thức CanTakeDamage
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
        else if (verticalVelocity > 0.1f)
        {
            animator.SetBool("isJumping", true);
            animator.SetBool("isFalling", false);
            animator.SetBool("isGrounded", false);
        }
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

    private void HandleHeavyAttack()
    {
        if (Input.GetMouseButtonDown(1) && Time.time >= lastAttackTime + attackRate && !isAttacking && isBuff)
        {
            HeavyAttack();
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
                if (isBuff)
                {
                    enemyHealth.TakeDamage(attackDamage+5f);
                }
                else
                {
                    enemyHealth.TakeDamage(attackDamage);
                }
            }
        }

        // Sử dụng Animation Event thay vì Coroutine
        // Animation Event sẽ gọi OnAttackComplete khi animation kết thúc
    }
    private void HeavyAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        animator.SetTrigger("heavyAttack");




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
                
                enemyHealth.TakeDamage(heavyAttackDamge);
                
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
        // Hủy đăng ký các event khi object bị hủy
        if (healthSystem != null)
        {
            healthSystem.OnHealthChanged.RemoveListener(CheckHealth);
            healthSystem.OnDeath.RemoveListener(HandleDeath);
            healthSystem.OnHit.RemoveListener(HandleHit);
        }

        if (manaSystem != null)
        {
            manaSystem.OnManaChanged.RemoveListener(CheckMana);
        }
    }


    // Xu ly su dung mana

    private void CheckMana(float manaPercentage)
    {
        if (manaPercentage <= 0 )
        {
            OutOfMana();
        }
    }

    // Thêm hàm xử lý animation event
    public void OnManaRecoveringStart()
    {
        isManaRecovering = true;
        rb.velocity = Vector2.zero;
    }

    public void OnManaRecoveringComplete()
    {
        isManaRecovering = false;
        animator.SetBool("isManaRecovering", false);
    }

    private void OutOfMana()
    {
        if (isDead) return;

        outOfMana = true;
    }

    private void HandleUseBuff()
    {
        if (!isBuff && currentMana >= 20f )
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                UseBuff();
            }
            
        }
    }

    private void UseBuff()
    {
        if (isDead || isBuff) return; // Kiểm tra nếu nhân vật đã chết hoặc đang buff

        //Input.GetKeyDown(KeyCode.E);

        isBuff = true;
        animator.SetBool("isBuff", true);
        manaSystem.UseBuff(20f);

        if (buffCoroutine != null)
        {
            StopCoroutine(buffCoroutine); // Dừng bất kỳ buff nào đang chạy trước đó
        }

        buffCoroutine = StartCoroutine(DisableBuffAfterDuration(3f)); // Chạy buff trong 3 giây
    }

    private IEnumerator DisableBuffAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration); // Chờ 3 giây

        isBuff = false;
        animator.SetBool("isBuff", false);
        buffCoroutine = null; // Reset lại Coroutine để có thể kích hoạt buff lại
    }

}