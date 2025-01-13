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
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask npcLayer; // Thêm layer cho NPC

    private Transform currentTarget;
    private bool isTargetingNPC = false;

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

        // Tìm player
        player = GameObject.FindGameObjectWithTag("Player").transform;
        currentTarget = player; // Ban đầu target là player
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
        // Xác định layer mask dựa trên target hiện tại
        LayerMask targetLayer = isTargetingNPC ? npcLayer : playerLayer;

        Collider2D[] hitTargets = Physics2D.OverlapCircleAll(
            transform.position + transform.right * 1f,
            1.5f,
            targetLayer
        );

        foreach (Collider2D hitTarget in hitTargets)
        {
            HealthSystem targetHealth = hitTarget.GetComponent<HealthSystem>();

            if (targetHealth != null)
            {
                if (isTargetingNPC || (hitTarget.CompareTag("Player") && hitTarget.GetComponent<PlayerMovement>().CanTakeDamage()))
                {
                    targetHealth.TakeDamage(damage);
                    Debug.Log($"Dealt {damage} damage to {hitTarget.name}");
                }
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
        // Nếu không có target hợp lệ, quay về tuần tra
        if (currentTarget == null)
        {
            ReturnToPatrol();
            return;
        }

        float distanceToStart = Vector2.Distance(transform.position, startPosition);
        if (distanceToStart > patrolDistance)
        {
            ReturnToPatrol();
            return;
        }

        float distanceToTarget = Vector2.Distance(transform.position, currentTarget.position);
        Vector2 directionToTarget = ((Vector2)currentTarget.position - (Vector2)transform.position).normalized;

        if (distanceToTarget <= 1.5f)
        {
            rb.velocity = Vector2.zero;
            UpdateFacingDirection(directionToTarget.x > 0);

            if (Time.time >= lastAttackTime + attackRate)
            {
                StartCoroutine(PerformComboAttack());
            }
        }
        else
        {
            rb.velocity = directionToTarget * chaseSpeed;
            UpdateFacingDirection(directionToTarget.x > 0);
            animator.SetBool("isMoving", true);
        }
    }

    private void ReturnToPatrol()
    {
        Debug.Log("Returning to patrol");
        isChasing = false;
        isTargetingNPC = false;
        Vector2 returnDirection = (startPosition - (Vector2)transform.position).normalized;
        rb.velocity = returnDirection * patrolSpeed;
        UpdateFacingDirection(returnDirection.x > 0);

        if (Vector2.Distance(transform.position, startPosition) < 0.1f)
        {
            transform.position = startPosition;
            rb.velocity = Vector2.zero;
            UpdateFacingDirection(movingRight);
        }
    }

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
        // Cập nhật quest trước khi tắt UI
        var questManager = QuestSystem.QuestManager.Instance;
        if (questManager != null)
        {
            questManager.CompleteQuest("Clear_EnemyPatrol");
            questManager.StartQuest("Hide_Enemy");
        }
        else
        {
            Debug.LogError("QuestManager is null!");
        }
        UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
    }

    // Cập nhật Update để kiểm tra sức khỏe
    private void Update()
    {
        if (isDead || isHit) return;

        // Kiểm tra nếu đang nhắm vào NPC và NPC không còn nữa
        if (isTargetingNPC && currentTarget == null)
        {
            Debug.Log("NPC target lost, resetting to player target");
            isTargetingNPC = false;
            currentTarget = player;
            isChasing = false; // Reset trạng thái chase
            ReturnToPatrol(); // Quay về tuần tra
        }

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
        // Kiểm tra NPC trong tầm trước
        Collider2D[] npcs = Physics2D.OverlapCircleAll(transform.position, detectionRange, npcLayer);
        if (npcs.Length > 0)
        {
            foreach (Collider2D npc in npcs)
            {
                // Kiểm tra có phải là NPC không
                if (npc.CompareTag("NPC"))
                {
                    currentTarget = npc.transform;
                    isTargetingNPC = true;
                    isChasing = true;
                    isWaiting = false;
                    Debug.Log("NPC detected: " + npc.name);
                    return;
                }
            }
        }

        // Nếu không có NPC, kiểm tra player
        if (!isTargetingNPC && player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            RaycastHit2D hit = Physics2D.Raycast(
                transform.position,
                facingRight ? Vector2.right : Vector2.left,
                detectionRange,
                playerLayer
            );

            Vector2 directionToPlayer = ((Vector2)player.position - (Vector2)transform.position).normalized;
            float angle = Vector2.Angle(facingRight ? Vector2.right : Vector2.left, directionToPlayer);

            if ((hit.collider != null && hit.collider.CompareTag("Player")) ||
                (distanceToPlayer <= detectionRange && angle < 90f))
            {
                currentTarget = player;
                isChasing = true;
                isWaiting = false;
                Debug.Log("Player detected!");
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

        // Vẽ vùng phát hiện NPC
        if (isTargetingNPC)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
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
