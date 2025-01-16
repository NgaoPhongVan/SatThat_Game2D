using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class BossController : MonoBehaviour
{
    public Transform waypoint1; 
    public Transform waypoint2;
    public float speed = 2f;
    public float rollSpeed = 2f; 
    public Rigidbody2D rb;
    public float attackRange = 5f; 
    public Transform player; 
    public float waitTime = 2f; 
    public GameObject enemy;
    private Transform currentWaypoint; 
    private Animator animator; 
    private bool isPatrolling = true; 
    private bool isAttacking = false; 
    private bool isWaiting = false;
    private Vector3 localScale; 
    private string[] attacks = new string[] { "attack1", "attack2", "attack3" }; 
    private string currentAttack; 
    public bool isDefending; 
    private bool isHit; 
    public float patrolRange = 10f; 
    private bool isJumping; 
    private bool isDeath; 
    private bool isSuperKill; 
    private bool isGenerated;
    private int jumpSpeed;

    void Start()
    {
        currentWaypoint = waypoint1; 
        animator = GetComponent<Animator>(); 
        localScale = transform.localScale; 
        rb = GetComponent<Rigidbody2D>(); 
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void FixedUpdate()
    {
        if (isDeath) return; 
        float distanceToPlayer = Vector3.Distance(transform.position, player.position); 
        
        if (IsPlayerWithinPatrolArea())
        {
            GenerateEnemies();
            if (distanceToPlayer <= attackRange && !isHit && !isDefending)  
            {
                if (player.position.y > transform.position.y + 1.0f) 
                {
                    animator.SetBool("IsMoving", false); 
                    if (!isJumping) 
                    {
                        StartCoroutine(JumpAndAttackPlayer()); 
                    }
                }
                else
                {
                    animator.SetBool("IsMoving", false); 
                    if (!isAttacking && !isDeath) 
                    {
                        StartCoroutine(AttackPlayer()); 
                    }
                }
            }
            else if (distanceToPlayer > 2 * attackRange) 
            {
                RollToPlayer(); 
            }
            else
            {
                var stateInfo = animator.GetCurrentAnimatorStateInfo(0); 
                if (stateInfo.IsName("roll") && stateInfo.normalizedTime < 1.0f) 
                {
                    return; 
                }
                if (isJumping) return; 
                MoveTowardsPlayer(); 
            }
        }
        else if (isAttacking)
        { 
            isAttacking = false;
            animator.ResetTrigger(currentAttack);
        }
        else
        {
            animator.SetBool("IsMoving", false); 
        }
    }

    private void GenerateEnemies()
    {
        if (isGenerated || isDeath) return;
        isGenerated = true;
        InvokeRepeating("InitEnemies", 0, 10);
    }

    private void InitEnemies()
    {
        int amout = Random.Range(0, 7);
        StartCoroutine(GenEnemies(amout));
    }

    private IEnumerator GenEnemies(int amout)
    {
        for (int i = 0; i < amout; i++)
        {
            Instantiate(enemy, waypoint1.position, Quaternion.identity);
            var e = Instantiate(enemy, waypoint2.position, Quaternion.identity);
            e.GetComponent<EnemyPatrol>().UpdateFacingDirection(false);
            yield return new WaitForSeconds(1);
        }
    }

    void RollToPlayer()
    {
        if (isDeath) return; 
        StopCoroutine(Roll()); 
        StartCoroutine(Roll()); 
    }

    IEnumerator Roll()
    {
        animator.SetBool("IsMoving", false); 
        animator.SetTrigger("roll");
        Vector3 targetPosition = player.position + (transform.position - player.position).normalized * attackRange; 
        while (Vector3.Distance(transform.position, targetPosition) > attackRange) 
        {
            Vector3 direction = (targetPosition - transform.position).normalized; 
            transform.position += direction * rollSpeed * Time.deltaTime; 
            if (direction.x > 0) 
            {
                transform.localScale = new Vector3(Mathf.Abs(localScale.x), localScale.y, localScale.z); 
            }
            else if (direction.x < 0) 
            {
                transform.localScale = new Vector3(-Mathf.Abs(localScale.x), localScale.y, localScale.z); 
            }
            yield return null; 
        }
        if (Vector3.Distance(transform.position, player.position) <= attackRange) 
        {
            StartCoroutine(AttackPlayer()); 
        }
    }

    bool IsPlayerWithinPatrolArea()
    { 
        return player.position.x >= waypoint1.position.x && player.position.x <= waypoint2.position.x;
    }
    void MoveTowardsPlayer()
    {
        if (isDeath) return; 
        if (Vector3.Distance(transform.position, player.position) > attackRange - 1) 
        {
            Vector3 direction = player.position - transform.position; 
            transform.position += direction.normalized * speed * Time.deltaTime; 
            Flip(); 
            animator.SetBool("IsMoving", true); 
        }
        else
        {
            animator.SetBool("IsMoving", false); 
        }
    }
    IEnumerator AttackPlayer()
    {
        if (isDeath) yield return null; 
        if(!player.GetComponent<PlayerMovement>().CanTakeDamage())
        {
          yield break;
        }
        isPatrolling = false; 
        isAttacking = true; 
        animator.SetBool("IsMoving", false); 
        var playerHealth = player.GetComponent<HealthSystem>();
        if(playerHealth.currentHealth <= 50)
        {
            isSuperKill = true;
        }
        while (Vector3.Distance(transform.position, player.position) <= attackRange)
        {
            if (!isDefending && !isHit && !isJumping && !isDeath) 
            {
                Flip(); 
                var stateInfo = animator.GetCurrentAnimatorStateInfo(0); 
                if (stateInfo.IsName("sup_attack") && stateInfo.normalizedTime < 1.0f) 
                {
                    yield return null; 
                    continue;
                }
                if (stateInfo.IsName(currentAttack) && stateInfo.normalizedTime < 1.0f) 
                {
                    yield return null; 
                    continue; 
                }
                if (stateInfo.IsName("hit") && stateInfo.normalizedTime < 1.0f) 
	            {
                    yield return null; 
                    continue; 
                }
                if (stateInfo.IsName("defend") && stateInfo.normalizedTime < 1.0f) 
                {
                    yield return null; 
                    continue; 
                }
                if (playerHealth.currentHealth <= 50)
                {
                    isSuperKill = true;
                    OnSupperAttack();
                    yield break;
                }
                currentAttack = RandomAttack(); 
                animator.SetTrigger(currentAttack); 
                Debug.Log("Attacking the player!"); 
                yield return new WaitForSeconds(0.5f); 

                player.GetComponent<HealthSystem>().TakeDamage(SetDamageAttack(currentAttack));
            }
            else
            {
                break; 
            }
            yield return null; 
        }
        isAttacking = false; 
    }

    private float SetDamageAttack(string currentAttack)
    {
        switch(currentAttack)
        {
            case "attack1": return 10;
            case "attack2": return 20;
            case "attack3": return 30;
            default: return 10;
        }
    }

    private IEnumerator OnEndLastHit()
    {
        yield return new WaitForSeconds(1.5f); 
        animator.ResetTrigger("supper_attack"); 
        isSuperKill = false; 
        player.GetComponent<HealthSystem>().TakeDamage(50);

    }

    private void Flip()
    {
        Vector3 direction = player.position - transform.position; 
        if (direction.x > 0) 
        {
            transform.localScale = new Vector3(Mathf.Abs(localScale.x), localScale.y, localScale.z);
            
        }
        else if (direction.x < 0) 
        {
            transform.localScale = new Vector3(-Mathf.Abs(localScale.x), localScale.y, localScale.z); 
            
        }
    }
    void Defend()
    {
        isDefending = true; 
        animator.SetTrigger("defend"); 
        Debug.Log("Defending!"); 
        StopCoroutine(EndDefend()); 
        StartCoroutine(EndDefend()); 
    }
    IEnumerator EndDefend()
    {
        yield return new WaitForSeconds(2f);
        isDefending = false; 
    }
    public void OnHit()
    {
        if (isDeath) return; 
        animator.ResetTrigger(currentAttack); 
        if (Random.value < 0.1f) 
        {
            var stateInfo = animator.GetCurrentAnimatorStateInfo(0); 
            if (stateInfo.IsName("hit") && stateInfo.normalizedTime < 1.0f) 
            {
                return; 
            }
            Defend(); 
        }
        else
        {
            var stateInfo = animator.GetCurrentAnimatorStateInfo(0); 
            if (stateInfo.IsName("defend") && stateInfo.normalizedTime < 1.0f) 
            {
                return;
            }
            isHit = true; 
            StopCoroutine(AttackPlayer()); 
            isAttacking = false;
            animator.SetBool("IsMoving", false);
            animator.SetTrigger("hit"); 
            StopCoroutine(RecoverFromHit()); 
            StartCoroutine(RecoverFromHit()); 
        }
    }

    public void OnDeath()
    {
        if (isDeath) return;
        isDeath = true; 
        animator.SetTrigger("death");
        StartCoroutine(OnEndDeath());
    }
    private IEnumerator OnEndDeath()
    {
        foreach (var enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            Destroy(enemy);
        }
        yield return new WaitForSeconds(2f); 
        SceneManager.LoadScene(0); 
    }

    IEnumerator RecoverFromHit()
    {
        yield return new WaitForSeconds(2f); 
        isHit = false; 
        animator.ResetTrigger("hit"); 
    }

    public void OnSupperAttack()
    {
        if (isDeath) return;
        isSuperKill = true; 
        animator.SetTrigger("supper_attack");
        StartCoroutine(OnEndLastHit());
    }

    private string RandomAttack()
    {
        return attacks[Random.Range(0, attacks.Length)];
    }

    IEnumerator JumpAndAttackPlayer()
    {

        isJumping = true; 
        animator.SetTrigger("jump"); 
        
        float jumpForce = Mathf.Sqrt(2 * jumpSpeed * (player.position.y - transform.position.y)); 
        rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse); 
        
       
        while (rb.velocity.y > 0) { yield return null; } 
       
        while (transform.position.y < player.position.y && rb.velocity.y > 0) { yield return null; } 

        rb.simulated = false; 
        while (player.position.y > transform.position.y - 1.0f) 
        {
            Flip(); 
                float distanceToPlayer = Vector3.Distance(transform.position, player.position); 
            if (distanceToPlayer > attackRange || player.position.y <= transform.position.y) 
            {
                break; 
            }
            animator.SetTrigger("air_attack"); 
          
            Debug.Log("Air Attacking the player!"); 
            yield return new WaitForSeconds(1f); 
        } 
        rb.simulated = true; 
        animator.SetBool("IsMoving", false); 
        animator.SetTrigger("jumpdown");


        rb.AddForce(new Vector2(0, -jumpForce), ForceMode2D.Impulse); 
        
        while (rb.velocity.y < 0) { yield return null; } 
        isJumping = false; 
       }
}