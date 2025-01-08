using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class BossController : MonoBehaviour
{
    public Transform waypoint1;
    public Transform waypoint2;
    public float speed = 2f;
    public float jumpSpeed = 15f;
    public float rollSpeed = 2f;
    public Rigidbody2D rb;
    public float attackRange = 5f;
    public Transform player;
    public float waitTime = 2f;
    private Transform currentWaypoint;
    private Animator animator;
    private bool isPatrolling = true;
    private bool isAttacking = false;
    private bool isWaiting = false;
    private Vector3 localScale;
    private string[] attacks = new string[] { "attack1", "attack2", "attack3" };
    private string currentAttack;
    private bool isDefending;
    private bool isHit;
    public float patrolRange = 10f;
    private bool isJumping;
    private bool isDeath;
    private bool isSuperKill;

    void Start()
    {
        currentWaypoint = waypoint1;
        animator = GetComponent<Animator>(); // Get the Animator component
        localScale = transform.localScale; // Store the original local scale
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (isDeath) return;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        // Check if the player is within the defined area (waypoint1 to waypoint2)
        if (IsPlayerWithinPatrolArea())
        {
            if (distanceToPlayer <= attackRange && !isHit && !isDefending)
            {
                if (player.position.y > transform.position.y + 1.0f)
                {
                    animator.SetBool("IsMoving", false);
                    if (!isJumping)
                    {
                        //StartCoroutine(JumpAndAttackPlayer());
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
        { // Stop attacking if the player leaves the defined area

            isAttacking = false;
            animator.ResetTrigger(currentAttack);
        }
        else
        {
            animator.SetBool("IsMoving", false);
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
    { // Check if the player is within the horizontal bounds of waypoint1 and waypoint2
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
        isPatrolling = false;
        isAttacking = true;
        animator.SetBool("IsMoving", false);
        while (Vector3.Distance(transform.position, player.position) <= attackRange)
        {
            if (!isDefending && !isHit && !isJumping && !isDeath)
            {
                Flip();
                var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                if (isSuperKill)
                {
                    animator.SetTrigger("super_attack");
                    StartCoroutine(OnEndLastHit());
                }
                else
                {
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
                    currentAttack = RandomAttack();
                    animator.SetTrigger(currentAttack);
                    Debug.Log("Attacking the player!");
                }
               
                yield return new WaitForSeconds(1f);
            }
            else
            {
                break;
            }
            yield return null;
        }
        isAttacking = false;
    }

    private IEnumerator OnEndLastHit()
    {
        yield return new WaitForSeconds(1.5f);
        animator.ResetTrigger("supper_attack");
        isSuperKill = false;
    }

    private void Flip()
    {
        Vector3 direction = player.position - transform.position;
        if (direction.x > 0)
        {
            transform.localScale = new Vector3(Mathf.Abs(localScale.x), localScale.y, localScale.z);
            // Face right
        }
        else if (direction.x < 0)
        {
            transform.localScale = new Vector3(-Mathf.Abs(localScale.x), localScale.y, localScale.z);
            // Face left
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
        isDeath = true;
        animator.SetTrigger("death");
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
    }

    private string RandomAttack()
    {
        return attacks[Random.Range(0, attacks.Length)];
    }

    //IEnumerator JumpAndAttackPlayer()
    //{
       
    //    isJumping = true;
    //    animator.SetTrigger("jump");
    //    // Play jump animation 
    //    // Calculate the force needed to reach the player's height
    //    float jumpForce = Mathf.Sqrt(2 * jumpSpeed * (player.position.y - transform.position.y));
    //    rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
    //    // Add vertical force to jump 
    //    // Wait until boss reaches the height of the
    //    while (rb.linearVelocity.y > 0) { yield return null; }
    //    // While player is in the air, stay and attack
    //    // Wait until boss reaches the height of the player
    //    while (transform.position.y < player.position.y && rb.linearVelocity.y > 0) { yield return null; }

    //    rb.simulated = false;
    //    while (player.position.y > transform.position.y - 1.0f)
    //    {
    //        Flip();
    //        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
    //        if (distanceToPlayer > attackRange || player.position.y <= transform.position.y)
    //        {
    //            break;
    //        }
    //        animator.SetTrigger("air_attack"); 
    //        // Play air attack animation
    //        Debug.Log("Air Attacking the player!"); 
    //        yield return new WaitForSeconds(1f); 
    //    } // Jump down when player starts to fall
    //    rb.simulated = true;
    //    animator.SetBool("IsMoving", false);
    //    animator.SetTrigger("jumpdown"); 
        

    //    rb.AddForce(new Vector2(0, -jumpForce), ForceMode2D.Impulse); 
    //    // Add force to jump down 
    //    // Wait until boss lands
    //    while (rb.linearVelocity.y < 0) { yield return null; } 
    //    isJumping = false;
    //    //isJumping = true; 
    //    //animator.SetTrigger("jump"); 
    //    //// Play jump animation
    //    //Vector3 targetPosition = new Vector3(player.position.x, player.position.y, transform.position.z); 
    //    //// Jump towards the player
    //    //while (Vector3.Distance(transform.position, targetPosition) > attackRange) { 
    //    //    Vector3 direction = (targetPosition - transform.position).normalized; 
    //    //    rb.MovePosition(transform.position + direction * jumpSpeed * Time.fixedDeltaTime);
    //    //    if (direction.x > 0) { 
    //    //        transform.localScale = new Vector3(Mathf.Abs(localScale.x), localScale.y, localScale.z);
    //    //    } 
    //    //    else if (direction.x < 0) { 
    //    //        transform.localScale = new Vector3(-Mathf.Abs(localScale.x), localScale.y, localScale.z); 
    //    //    } yield return null; 
    //    //} // While player is in the air, attack
    //    //while (player.position.y >= transform.position.y) { 
    //    //    animator.SetTrigger("air_attack"); 
    //    //    Debug.Log("Air Attacking the player!"); 
    //    //    yield return new WaitForSeconds(1f); 
    //    //} 
    //    //animator.SetTrigger("jumpdown"); 
        
    //    //isJumping = false;
    //}
}