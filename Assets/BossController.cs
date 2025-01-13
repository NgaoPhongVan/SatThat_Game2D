using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class BossController : MonoBehaviour
{
    public Transform waypoint1; // Điểm di chuyển 1
    public Transform waypoint2; // Điểm di chuyển 2
    public float speed = 2f; // Tốc độ di chuyển
    public float jumpSpeed = 15f; // Tốc độ nhảy
    public float rollSpeed = 2f; // Tốc độ lăn
    public Rigidbody2D rb; // Thành phần Rigidbody2D
    public float attackRange = 5f; // Phạm vi tấn công
    public Transform player; // Transform của người chơi
    public float waitTime = 2f; // Thời gian chờ
    public GameObject enemy;
    private Transform currentWaypoint; // Điểm di chuyển hiện tại
    private Animator animator; // Thành phần Animator
    private bool isPatrolling = true; // Trạng thái tuần tra
    private bool isAttacking = false; // Trạng thái tấn công
    private bool isWaiting = false; // Trạng thái chờ
    private Vector3 localScale; // Tỉ lệ cục bộ ban đầu
    private string[] attacks = new string[] { "attack1", "attack2", "attack3" }; // Mảng các kiểu tấn công
    private string currentAttack; // Kiểu tấn công hiện tại
    public bool isDefending; // Trạng thái phòng thủ
    private bool isHit; // Trạng thái bị đánh trúng
    public float patrolRange = 10f; // Phạm vi tuần tra
    private bool isJumping; // Trạng thái đang nhảy
    private bool isDeath; // Trạng thái đã chết
    private bool isSuperKill; // Trạng thái tấn công đặc biệt
    private bool isGenerated;

    void Start()
    {
        currentWaypoint = waypoint1; // Đặt điểm di chuyển ban đầu là waypoint1
        animator = GetComponent<Animator>(); // Lấy thành phần Animator
        localScale = transform.localScale; // Lưu trữ tỉ lệ cục bộ ban đầu
        rb = GetComponent<Rigidbody2D>(); // Lấy thành phần Rigidbody2D
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void FixedUpdate()
    {
        if (isDeath) return; // Nếu đã chết thì không thực hiện gì
        float distanceToPlayer = Vector3.Distance(transform.position, player.position); // Tính khoảng cách đến người chơi
        // Kiểm tra xem người chơi có nằm trong khu vực tuần tra (từ waypoint1 đến waypoint2)
        if (IsPlayerWithinPatrolArea())
        {
            GenerateEnemies();
            if (distanceToPlayer <= attackRange && !isHit && !isDefending) // Nếu người chơi trong phạm vi tấn công và không bị đánh trúng hoặc đang phòng thủ
            {
                if (player.position.y > transform.position.y + 1.0f) // Nếu người chơi ở trên boss
                {
                    animator.SetBool("IsMoving", false); // Dừng di chuyển
                    if (!isJumping) // Nếu không đang nhảy
                    {
                        StartCoroutine(JumpAndAttackPlayer()); // Bắt đầu coroutine nhảy và tấn công
                    }
                }
                else
                {
                    animator.SetBool("IsMoving", false); // Dừng di chuyển
                    if (!isAttacking && !isDeath) // Nếu không đang tấn công và chưa chết
                    {
                        StartCoroutine(AttackPlayer()); // Bắt đầu coroutine tấn công
                    }
                }
            }
            else if (distanceToPlayer > 2 * attackRange) // Nếu người chơi quá xa
            {
                RollToPlayer(); // Lăn về phía người chơi
            }
            else
            {
                var stateInfo = animator.GetCurrentAnimatorStateInfo(0); // Lấy thông tin trạng thái của animator
                if (stateInfo.IsName("roll") && stateInfo.normalizedTime < 1.0f) // Nếu đang lăn và chưa kết thúc animation
                {
                    return; // Không làm gì
                }
                if (isJumping) return; // Nếu đang nhảy thì không làm gì
                MoveTowardsPlayer(); // Di chuyển về phía người chơi
            }
        }
        else if (isAttacking)
        { // Dừng tấn công nếu người chơi rời khỏi khu vực
            isAttacking = false;
            animator.ResetTrigger(currentAttack);
        }
        else
        {
            animator.SetBool("IsMoving", false); // Dừng di chuyển
        }
    }

    private void GenerateEnemies()
    {
        if (isGenerated) return;
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
        if (isDeath) return; // Nếu đã chết thì không thực hiện gì
        StopCoroutine(Roll()); // Dừng coroutine lăn hiện tại
        StartCoroutine(Roll()); // Bắt đầu coroutine lăn
    }

    IEnumerator Roll()
    {
        animator.SetBool("IsMoving", false); // Dừng di chuyển
        animator.SetTrigger("roll"); // Kích hoạt animation lăn
        Vector3 targetPosition = player.position + (transform.position - player.position).normalized * attackRange; // Tính vị trí mục tiêu để lăn đến
        while (Vector3.Distance(transform.position, targetPosition) > attackRange) // Lặp cho đến khi đến gần mục tiêu
        {
            Vector3 direction = (targetPosition - transform.position).normalized; // Tính hướng di chuyển
            transform.position += direction * rollSpeed * Time.deltaTime; // Di chuyển theo hướng
            if (direction.x > 0) // Nếu di chuyển sang phải
            {
                transform.localScale = new Vector3(Mathf.Abs(localScale.x), localScale.y, localScale.z); // Lật mặt sang phải
            }
            else if (direction.x < 0) // Nếu di chuyển sang trái
            {
                transform.localScale = new Vector3(-Mathf.Abs(localScale.x), localScale.y, localScale.z); // Lật mặt sang trái
            }
            yield return null; // Chờ frame tiếp theo
        }
        if (Vector3.Distance(transform.position, player.position) <= attackRange) // Nếu đến gần người chơi
        {
            StartCoroutine(AttackPlayer()); // Bắt đầu tấn công
        }
    }

    bool IsPlayerWithinPatrolArea()
    { // Kiểm tra xem người chơi có nằm trong phạm vi ngang của waypoint1 và waypoint2
        return player.position.x >= waypoint1.position.x && player.position.x <= waypoint2.position.x;
    }
    void MoveTowardsPlayer()
    {
        if (isDeath) return; // Nếu đã chết thì không thực hiện gì
        if (Vector3.Distance(transform.position, player.position) > attackRange - 1) // Nếu người chơi ở ngoài phạm vi tấn công
        {
            Vector3 direction = player.position - transform.position; // Tính hướng di chuyển
            transform.position += direction.normalized * speed * Time.deltaTime; // Di chuyển về phía người chơi
            Flip(); // Lật mặt theo hướng di chuyển
            animator.SetBool("IsMoving", true); // Bật animation di chuyển
        }
        else
        {
            animator.SetBool("IsMoving", false); // Tắt animation di chuyển
        }
    }
    IEnumerator AttackPlayer()
    {
        if (isDeath) yield return null; // Nếu đã chết thì không thực hiện gì
        if(!player.GetComponent<PlayerMovement>().CanTakeDamage())
        {
          yield break;
        }
        isPatrolling = false; // Dừng tuần tra
        isAttacking = true; // Bắt đầu tấn công
        animator.SetBool("IsMoving", false); // Dừng di chuyển
        var playerHealth = player.GetComponent<HealthSystem>();
        if(playerHealth.currentHealth <= 50)
        {
            isSuperKill = true;
        }
        while (Vector3.Distance(transform.position, player.position) <= attackRange) // Lặp khi người chơi trong phạm vi tấn công
        {
            if (!isDefending && !isHit && !isJumping && !isDeath) // Nếu không đang phòng thủ, bị đánh, nhảy hoặc chết
            {
                Flip(); // Lật mặt về phía người chơi
                var stateInfo = animator.GetCurrentAnimatorStateInfo(0); // Lấy thông tin trạng thái animator
                if (stateInfo.IsName("sup_attack") && stateInfo.normalizedTime < 1.0f) // Nếu đang thực hiện animation tấn công đặc biệt
                {
                    yield return null; // Chờ frame tiếp theo
                    continue; // Tiếp tục vòng lặp
                }
                if (stateInfo.IsName(currentAttack) && stateInfo.normalizedTime < 1.0f) // Nếu đang thực hiện animation tấn công hiện tại
                {
                    yield return null; // Chờ frame tiếp theo
                    continue; // Tiếp tục vòng lặp
                }
                if (stateInfo.IsName("hit") && stateInfo.normalizedTime < 1.0f) // Nếu đang bị nhân vật chính tấn công
	            {
                    yield return null; // Chờ frame tiếp theo
                    continue; // Tiếp tục vòng lặp
                }
                if (stateInfo.IsName("defend") && stateInfo.normalizedTime < 1.0f) // Nếu đang trong trạng thái phòng thủ
                {
                    yield return null; // Chờ frame tiếp theo
                    continue; // Tiếp tục vòng lặp
                }
                if (playerHealth.currentHealth <= 50)
                {
                    isSuperKill = true;
                    OnSupperAttack();
                    yield break;
                }
                currentAttack = RandomAttack(); // Chọn ngẫu nhiên một kiểu tấn công
                animator.SetTrigger(currentAttack); // Kích hoạt animation tấn công
                Debug.Log("Attacking the player!"); // In thông báo tấn công
                yield return new WaitForSeconds(0.5f); // Chờ 1 giây giữa các lần tấn công

                player.GetComponent<HealthSystem>().TakeDamage(SetDamageAttack(currentAttack));
            }
            else
            {
                break; // Thoát khỏi vòng lặp nếu người chơi ra khỏi phạm vi tấn công
            }
            yield return null; // Chờ frame tiếp theo
        }
        isAttacking = false; // Kết thúc tấn công
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
        yield return new WaitForSeconds(1.5f); // Chờ 1.5 giây
        animator.ResetTrigger("supper_attack"); // Reset trigger tấn công đặc biệt
        isSuperKill = false; // Đặt trạng thái tấn công đặc biệt về false
        player.GetComponent<HealthSystem>().TakeDamage(50);

    }

    private void Flip()
    {
        Vector3 direction = player.position - transform.position; // Tính hướng giữa boss và người chơi
        if (direction.x > 0) // Nếu người chơi ở bên phải boss
        {
            transform.localScale = new Vector3(Mathf.Abs(localScale.x), localScale.y, localScale.z); // Lật mặt boss sang phải
            // Face right
        }
        else if (direction.x < 0) // Nếu người chơi ở bên trái boss
        {
            transform.localScale = new Vector3(-Mathf.Abs(localScale.x), localScale.y, localScale.z); // Lật mặt boss sang trái
            // Face left
        }
    }
    void Defend()
    {
        isDefending = true; // Bắt đầu phòng thủ
        animator.SetTrigger("defend"); // Kích hoạt animation phòng thủ
        Debug.Log("Defending!"); // In thông báo phòng thủ
        StopCoroutine(EndDefend()); // Dừng coroutine EndDefend hiện tại (để tránh xung đột)
        StartCoroutine(EndDefend()); // Bắt đầu coroutine EndDefend
    }
    IEnumerator EndDefend()
    {
        yield return new WaitForSeconds(2f); // Chờ 2 giây
        isDefending = false; // Kết thúc phòng thủ
    }
    public void OnHit()
    {
        if (isDeath) return; // Nếu đã chết thì không thực hiện gì
        animator.ResetTrigger(currentAttack); // Reset trigger tấn công hiện tại
        if (Random.value < 0.1f) // 10% cơ hội phòng thủ khi bị tấn công
        {
            var stateInfo = animator.GetCurrentAnimatorStateInfo(0); // Lấy thông tin trạng thái animator
            if (stateInfo.IsName("hit") && stateInfo.normalizedTime < 1.0f) // Nếu đang trong animation bị đánh
            {
                return; // Không làm gì
            }
            Defend(); // Phòng thủ
        }
        else
        {
            var stateInfo = animator.GetCurrentAnimatorStateInfo(0); // Lấy thông tin trạng thái animator
            if (stateInfo.IsName("defend") && stateInfo.normalizedTime < 1.0f) // Nếu đang trong animation phòng thủ
            {
                return; // Không làm gì
            }
            isHit = true; // Đặt trạng thái bị đánh
            StopCoroutine(AttackPlayer()); // Dừng coroutine tấn công
            isAttacking = false; // Dừng tấn công
            animator.SetBool("IsMoving", false); // Dừng di chuyển
            animator.SetTrigger("hit"); // Kích hoạt animation bị đánh
            StopCoroutine(RecoverFromHit()); // Dừng coroutine RecoverFromHit hiện tại (để tránh xung đột)
            StartCoroutine(RecoverFromHit()); // Bắt đầu coroutine RecoverFromHit
        }
    }

    public void OnDeath()
    {
        if (isDeath) return;
        isDeath = true; // Đặt trạng thái đã chết
        animator.SetTrigger("death"); // Kích hoạt animation chết
        StartCoroutine(OnEndDeath());
    }
    private IEnumerator OnEndDeath()
    {
        yield return new WaitForSeconds(2f); // Chờ 2 giây
        Destroy(gameObject); // Hủy đối tượng
    }

    IEnumerator RecoverFromHit()
    {
        yield return new WaitForSeconds(2f); // Chờ 2 giây
        isHit = false; // Hồi phục sau khi bị đánh
        animator.ResetTrigger("hit"); // Reset trigger bị đánh
    }

    public void OnSupperAttack()
    {
        if (isDeath) return; // Nếu đã chết thì không thực hiện gì
        isSuperKill = true; // Bật trạng thái tấn công đặc biệt
        animator.SetTrigger("supper_attack"); // Kích hoạt animation tấn công đặc biệt
        StartCoroutine(OnEndLastHit());
    }

    private string RandomAttack()
    {
        return attacks[Random.Range(0, attacks.Length)]; // Trả về một kiểu tấn công ngẫu nhiên từ mảng attacks
    }

    IEnumerator JumpAndAttackPlayer()
    {

        isJumping = true; // Đặt trạng thái đang nhảy
        animator.SetTrigger("jump"); // Kích hoạt animation nhảy
        // Play jump animation 
        // Calculate the force needed to reach the player's height
        float jumpForce = Mathf.Sqrt(2 * jumpSpeed * (player.position.y - transform.position.y)); // Tính toán lực nhảy cần thiết
        rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse); // Thêm lực nhảy theo phương thẳng đứng
        // Add vertical force to jump 
        // Wait until boss reaches the height of the
        while (rb.velocity.y > 0) { yield return null; } // Chờ đến khi boss đạt đến đỉnh của cú nhảy
        // While player is in the air, stay and attack
        // Wait until boss reaches the height of the player
        while (transform.position.y < player.position.y && rb.velocity.y > 0) { yield return null; } // Chờ đến khi boss đạt độ cao của người chơi

        rb.simulated = false; // Tắt mô phỏng vật lý để boss "treo" trên không
        while (player.position.y > transform.position.y - 1.0f) // Lặp khi người chơi vẫn ở trên boss
        {
            Flip(); // Lật mặt về phía người chơi
            float distanceToPlayer = Vector3.Distance(transform.position, player.position); // Tính khoảng cách đến người chơi
            if (distanceToPlayer > attackRange || player.position.y <= transform.position.y) // Nếu người chơi ra khỏi phạm vi tấn công hoặc xuống dưới boss
            {
                break; // Thoát khỏi vòng lặp
            }
            animator.SetTrigger("air_attack"); // Kích hoạt animation tấn công trên không
            // Play air attack animation
            Debug.Log("Air Attacking the player!"); // In thông báo tấn công trên không
            yield return new WaitForSeconds(1f); // Chờ 1 giây
        } // Jump down when player starts to fall
        rb.simulated = true; // Bật lại mô phỏng vật lý
        animator.SetBool("IsMoving", false); // Dừng di chuyển
        animator.SetTrigger("jumpdown"); // Kích hoạt animation nhảy xuống


        rb.AddForce(new Vector2(0, -jumpForce), ForceMode2D.Impulse); // Thêm lực để nhảy xuống
        // Add force to jump down 
        // Wait until boss lands
        while (rb.velocity.y < 0) { yield return null; } // Chờ đến khi boss chạm đất
        isJumping = false; // Kết thúc trạng thái nhảy
       }
}