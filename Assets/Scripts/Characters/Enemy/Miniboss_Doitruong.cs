using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Miniboss_Doitruong : MonoBehaviour
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

    [Header("Phase Change Settings")]
    [SerializeField] private GameObject enemyPrefab; // Prefab của EnemyPatrol
    [SerializeField] private Vector2[] spawnPoints; // Điểm spawn quân hỗ trợ
    [SerializeField] private float phase2Threshold = 0.5f; // 50% HP
    [SerializeField] private float phase3Threshold = 0.25f; // 25% HP
    private bool phase2Triggered = false;
    private bool phase3Triggered = false;

    [Header("Enhanced Attack Settings")]
    [SerializeField] private float attack1Damage = 15f;
    [SerializeField] private float attackAirDamage = 20f;
    [SerializeField] private float specialAttackCooldown = 3f;
    private float lastSpecialAttackTime = 0f;

    [Header("Scene Transition")]
    [SerializeField] private string nextSceneName = "NextSceneName";
    [SerializeField] private float sceneTransitionDelay = 2f;
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
        yield return new WaitForSeconds(0.3f); // Điều chỉnh thời gian phù hợp

        isHit = false;

        // Tiếp tục hành động trước đó nếu chưa chết
        if (!isDead)
        {
            animator.SetBool("isMoving", true);
        }
    }

    private void CheckPhaseTransition(float healthPercentage)
    {
        // Kiểm tra Phase 2 (50% HP)
        if (!phase2Triggered && healthPercentage <= phase2Threshold)
        {
            Debug.Log("Entering Phase 2");
            EnterPhase2();
        }

        // Kiểm tra Phase 3 (25% HP)
        if (!phase3Triggered && healthPercentage <= phase3Threshold)
        {
            Debug.Log("Entering Phase 3");
            EnterPhase3();
        }
    }

    // Thêm hàm này để debug trong Inspector
    public bool IsInPhase3()
    {
        return phase3Triggered;
    }

    private void EnterPhase2()
    {
        phase2Triggered = true;

        // Spawn quân hỗ trợ
        foreach (Vector2 spawnPoint in spawnPoints)
        {
            Instantiate(enemyPrefab, spawnPoint, Quaternion.identity);
        }

        // Tăng sức mạnh cho boss
        attackDamage *= 1.2f; // Tăng sát thương cơ bản
        chaseSpeed *= 1.1f; // Tăng tốc độ đuổi theo
    }

    private void EnterPhase3()
    {
        phase3Triggered = true;
        attackDamage *= 1.3f; // Tăng thêm sát thương
    }

    private System.Collections.IEnumerator PerformComboAttack()
    {
        isAttacking = true;
        comboCount = 0;
        rb.velocity = Vector2.zero;
        animator.SetBool("isMoving", false);

        while (comboCount < 3)
        {
            // Debug log để kiểm tra
            Debug.Log($"Current HP Percentage: {healthSystem.GetHealthPercentage()}");
            Debug.Log($"Phase 3 Triggered: {phase3Triggered}");
            Debug.Log($"Time since last special: {Time.time - lastSpecialAttackTime}");

            // Ưu tiên AttackAir trong Phase 3
            if (phase3Triggered && Time.time >= lastSpecialAttackTime + specialAttackCooldown)
            {
                Debug.Log("Triggering Air Attack");
                PerformAirAttack();
                yield return new WaitForSeconds(attackRate * 2f);
            }
            // Sau đó mới đến Attack1 trong Phase 2
            else if (phase2Triggered && Time.time >= lastSpecialAttackTime + specialAttackCooldown)
            {
                Debug.Log("Triggering Attack1");
                PerformSpecialAttack1();
                yield return new WaitForSeconds(attackRate * 1.5f);
            }
            else
            {
                animator.SetTrigger("attack");
                DealDamage(attackDamage);
                yield return new WaitForSeconds(attackRate);
            }

            comboCount++;
            lastAttackTime = Time.time;
        }

        isAttacking = false;
        UpdateFacingDirection(movingRight);
        animator.SetBool("isMoving", true);
    }

    private void PerformSpecialAttack1()
    {
        animator.SetTrigger("attack1");
        lastSpecialAttackTime = Time.time;
        DealDamage(attack1Damage);
    }

    private void PerformAirAttack()
    {
        Debug.Log("Performing Air Attack");
        animator.SetTrigger("attackAir");
        lastSpecialAttackTime = Time.time;
        StartCoroutine(AirAttackSequence());
    }

    private System.Collections.IEnumerator AirAttackSequence()
    {
        // Vô hiệu hóa gravity tạm thời
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0;

        // Nhảy lên
        Debug.Log("Air Attack - Jump Up");
        rb.velocity = Vector2.up * 10f;
        yield return new WaitForSeconds(0.5f);

        // Tấn công
        Debug.Log("Air Attack - Deal Damage");
        DealDamage(attackAirDamage);

        // Rơi xuống
        Debug.Log("Air Attack - Fall Down");
        rb.gravityScale = originalGravity;
        rb.velocity = Vector2.down * 15f;

        yield return new WaitForSeconds(0.5f);

        // Reset velocity
        rb.velocity = Vector2.zero;
    }

    public void OnAttack1DamageFrame()
    {
        // Gọi khi animation Attack1 đến frame gây damage
        DealDamage(attack1Damage);
    }

    public void OnAttackAirStart()
    {
        Debug.Log("Animation Event: AttackAir Start");
        rb.gravityScale = 0;
        rb.velocity = Vector2.up * 10f;
    }

    public void OnAttackAirDamage()
    {
        Debug.Log("Animation Event: AttackAir Damage");
        DealDamage(attackAirDamage);
    }

    public void OnAttackAirEnd()
    {
        Debug.Log("Animation Event: AttackAir End");
        rb.gravityScale = 1;
        rb.velocity = Vector2.zero;
    }

    private void DealDamage(float damage)
    {
        Debug.Log($"Attempting to deal {damage} damage");
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(
            transform.position + transform.right * 1f,
            1.5f,
            playerLayer
        );

        foreach (Collider2D hitPlayer in hitPlayers)
        {
            Debug.Log($"Hit player: {hitPlayer.name}");
            PlayerMovement playerMovement = hitPlayer.GetComponent<PlayerMovement>();
            HealthSystem playerHealth = hitPlayer.GetComponent<HealthSystem>();

            if (playerHealth != null && playerMovement != null && playerMovement.CanTakeDamage())
            {
                playerHealth.TakeDamage(damage);
                Debug.Log($"Dealt {damage} damage to player");
            }
        }
    }

    private void CheckHealth(float healthPercentage)
    {
        CheckPhaseTransition(healthPercentage);

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

        // Load scene mới sau 2 giây
        StartCoroutine(LoadNextSceneAfterDelay());
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


    // Thay thế hàm Flip() bằng hàm mới này
    private void UpdateFacingDirection(bool shouldFaceRight)
    {
        if (facingRight != shouldFaceRight)
        {
            facingRight = shouldFaceRight;
            transform.Rotate(0f, 180f, 0f);
        }
    }

    private System.Collections.IEnumerator LoadNextSceneAfterDelay()
    {
        yield return new WaitForSeconds(sceneTransitionDelay);
        UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
    }

    // Cập nhật Update để kiểm tra sức khỏe
    private void Update()
    {
        if (isDead || isHit) return;

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

        // Debug để kiểm tra khoảng cách
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        Debug.Log($"Distance to player: {distanceToPlayer}");

        // Kiểm tra player trong tầm phát hiện bằng cả Raycast và khoảng cách
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            facingRight ? Vector2.right : Vector2.left,
            detectionRange,
            playerLayer
        );

        // Vẽ ray để debug trong Scene view
        Debug.DrawRay(
            transform.position,
            (facingRight ? Vector2.right : Vector2.left) * detectionRange,
            hit.collider != null ? Color.red : Color.green
        );

        // Debug hit information
        if (hit.collider != null)
        {
            Debug.Log($"Hit object: {hit.collider.gameObject.name}");
        }

        // Kiểm tra phát hiện player bằng cả khoảng cách và hướng nhìn
        Vector2 directionToPlayer = ((Vector2)player.position - (Vector2)transform.position).normalized;
        float angle = Vector2.Angle(facingRight ? Vector2.right : Vector2.left, directionToPlayer);

        if ((hit.collider != null && hit.collider.CompareTag("Player")) ||
            (distanceToPlayer <= detectionRange && angle < 90f))
        {
            Debug.Log("Player detected!");
            isChasing = true;
            isWaiting = false;
        }
        else if (isChasing)
        {
            // Kiểm tra khoảng cách để quay về
            float distanceToStart = Vector2.Distance(transform.position, startPosition);
            if (distanceToStart > patrolDistance * 1.5f)
            {
                Debug.Log("Returning to patrol");
                isChasing = false;
                ReturnToPatrol();
            }
        }
    }

    private void Flip()
    {
        facingRight = !facingRight;
        transform.Rotate(0f, 180f, 0f);
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
