using System.Collections;
using UnityEngine;

public class TigerBossController : MonoBehaviour
{
    public Transform waypoint1;
    public Transform waypoint2;
    public Transform player;
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public float dashSpeed = 10f;
    public float attackRange = 3f;
    public float dashCooldown = 5f;
    public float dodgeSpeed = 8f;
    public float dodgeDistance = 5f;
    public float deathDelay = 3f;

    private Animator animator;
    private Rigidbody2D rb;
    private bool isPatrolling = true;
    private bool isChasing = false;
    private bool isAttacking = false;
    private bool isDashing = false;
    private bool isDodging = false;
    private bool isDead = false;
    private Transform currentWaypoint;
    private Vector3 localScale;
    private float lastDashTime;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        currentWaypoint = waypoint1;
        localScale = transform.localScale;
    }

    void FixedUpdate()
    {
        if (isDead) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (isPatrolling)
        {
            Patrol();
        }

        if (distanceToPlayer <= attackRange)
        {
            if (!isAttacking && !isDodging && !isDashing)
            {
                StartCoroutine(AttackPlayer());
            }
        }
        else if (distanceToPlayer <= attackRange * 2)
        {
            isPatrolling = false;
            isChasing = true;
            ChasePlayer();
        }
        else
        {
            isChasing = false;
            isPatrolling = true;
        }

        if (Time.time - lastDashTime >= dashCooldown && !isDashing && !isDodging)
        {
            if (Random.value < 0.5f)
            {
                StartCoroutine(DashTowardsPlayer());
            }
            else
            {
                StartCoroutine(DodgeAwayFromPlayer());
            }
        }
    }

    void Patrol()
    {
        animator.SetBool("IsMoving", true);

        if (Vector3.Distance(transform.position, currentWaypoint.position) < 0.1f)
        {
            currentWaypoint = currentWaypoint == waypoint1 ? waypoint2 : waypoint1;
        }

        Vector3 direction = (currentWaypoint.position - transform.position).normalized;
        rb.MovePosition(transform.position + direction * patrolSpeed * Time.fixedDeltaTime);
        Flip(direction.x);
    }

    void ChasePlayer()
    {
        animator.SetBool("IsMoving", true);
        Vector3 direction = (player.position - transform.position).normalized;
        rb.MovePosition(transform.position + direction * chaseSpeed * Time.fixedDeltaTime);
        Flip(direction.x);
    }

    IEnumerator AttackPlayer()
    {
        isAttacking = true;
        animator.SetTrigger("Attack");
        yield return new WaitForSeconds(1f); // Wait for attack animation to complete
        isAttacking = false;
    }

    IEnumerator DashTowardsPlayer()
    {
        isDashing = true;
        lastDashTime = Time.time;
        animator.SetTrigger("Dash");

        Vector3 direction = (player.position - transform.position).normalized;
        float dashTime = 0.3f;

        while (dashTime > 0)
        {
            rb.MovePosition(transform.position + direction * dashSpeed * Time.fixedDeltaTime);
            dashTime -= Time.fixedDeltaTime;
            yield return null;
        }

        isDashing = false;
    }

    IEnumerator DodgeAwayFromPlayer()
    {
        isDodging = true;
        lastDashTime = Time.time;
        animator.SetTrigger("Dodge");

        Vector3 direction = (transform.position - player.position).normalized;
        float dodgeTime = dodgeDistance / dodgeSpeed;

        while (dodgeTime > 0)
        {
            rb.MovePosition(transform.position + direction * dodgeSpeed * Time.fixedDeltaTime);
            dodgeTime -= Time.fixedDeltaTime;
            yield return null;
        }

        isDodging = false;
    }

    public void OnHit()
    {
        if (isDead) return;

        animator.SetTrigger("Hit");

        if (Random.value < 0.5f)
        {
            StartCoroutine(DodgeAwayFromPlayer());
        }
    }

    public void OnDeath()
    {
        if (isDead) return;

        isDead = true;
        animator.SetTrigger("Death");
        rb.velocity = Vector2.zero;

        StartCoroutine(HandleDeath());
    }

    IEnumerator HandleDeath()
    {
        yield return new WaitForSeconds(deathDelay);
        Destroy(gameObject);
    }

    private void Flip(float directionX)
    {
        if (directionX > 0)
            transform.localScale = new Vector3(Mathf.Abs(localScale.x), localScale.y, localScale.z);
        else if (directionX < 0)
            transform.localScale = new Vector3(-Mathf.Abs(localScale.x), localScale.y, localScale.z);
    }
}
