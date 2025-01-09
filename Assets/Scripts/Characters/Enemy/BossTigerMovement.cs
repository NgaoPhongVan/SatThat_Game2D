using System;
using System.Collections;
using UnityEngine;

public class TigerBossController : MonoBehaviour
{

    [SerializeField] private Camera mainCamera;
    public Transform activePoint;
    public float speed = 2f;
    public Rigidbody2D rb;
    public float attackRange = 3f;
    public Transform player;
    public float waitTime = 2f;
    private Animator animator;
    private float patrolRange = 5f;
    private bool isPatrolling = true;
    private bool isAttacking = false;
    private bool isActived = false;
    private int hp = 100;
    private bool isDeath = false;
    private Vector3 localScale;
    public LayerMask groundLayer;
    public Transform groundCheck;
    private bool isGrounded;

    private void Start()
    {
        animator = GetComponent<Animator>();
        localScale = transform.localScale;
        rb = GetComponent<Rigidbody2D>();

        // Kiểm tra xem player hoặc activePoint có bị thiếu hay không
        if (player == null || activePoint == null)
        {
            Debug.LogError("Player or ActivePoint is not assigned in the Inspector!");
        }

        animator.SetBool("IsMoving", false);
    }

    private void FixedUpdate()
    {
        if (isDeath) return;

        GroundChecked();

        //float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        //// Ví dụ logic bổ sung nếu cần thêm hành vi di chuyển/tấn công
        //if (distanceToPlayer <= attackRange && !isAttacking)
        //{
        //    StartCoroutine(AttackPlayer());
        //}
        if (isActived)
        {
            StartCoroutine(PerformActionsWithDelay());
            //MoveToActivePoint();
            //MoveTowardsPlayer();
        }
        
    }
    IEnumerator PerformActionsWithDelay()
    {
        // Di chuyển đến điểm kích hoạt (activePoint)
        MoveToActivePoint();

        // Chờ 2 giây giữa các hành động
        yield return new WaitForSeconds(waitTime);  // Thời gian trì hoãn (2 giây)

        // Di chuyển đến người chơi (player)
        MoveTowardsPlayer();
    }


    void GroundChecked()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    private void Flip()
    {
        // Tính toán khoảng cách từ đối tượng đến người chơi
        Vector3 direction = player.position - transform.position;

        // Kiểm tra hướng hiện tại và chỉ thay đổi nếu cần
        if ((direction.x > 0 && transform.localScale.x < 0) ||
            (direction.x < 0 && transform.localScale.x > 0))
        {
            // Đảo hướng trục X bằng cách lấy giá trị âm của localScale.x
            transform.localScale = new Vector3(-transform.localScale.x, localScale.y, localScale.z);
        }
    }


    public void OnDeath()
    {
        isDeath = true;
        animator.SetTrigger("death");
        Debug.Log("Tiger Boss is dead.");
    }

    IEnumerator AttackPlayer()
    {
        if (isDeath) yield break;

        isPatrolling = false;
        isAttacking = true;
        animator.SetBool("IsMoving", false);

        while (Vector3.Distance(transform.position, player.position) <= attackRange)
        {
            if (!isDeath)
            {
                Flip();
                Debug.Log("Tiger is attacking player.");
                animator.SetTrigger("IsAttacking");

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


    private void MoveTowardsPlayer()
    {
        if (isDeath) return;
        float distance = Vector3.Distance(transform.position, player.position);
        Debug.Log(distance);
        if (distance > attackRange)
        {
            Vector3 direction = player.position - transform.position;
            transform.position += direction.normalized * speed * Time.deltaTime;
            //Flip();
            Debug.Log("Dang truy duoi");
            animator.SetBool("IsMoving", true);
        }
        else
        {
            animator.SetBool("IsMoving", false);
        }
    }

    public void ActiveBoss()
    {
        isActived = true;
    }

    public void MoveToActivePoint()
    {
        if (isDeath) return; // Ngăn di chuyển nếu đã chết

        float distance = Vector3.Distance(transform.position, activePoint.position);

        // Di chuyển đến activePoint
        if (distance > 1f)
        {
            // Tính toán vector hướng chính xác
            // Di chuyển đối tượng
            Vector3 direction = activePoint.position - transform.position;
            transform.position += direction.normalized * speed * Time.deltaTime;

            //Debug.Log("Dang di chuyen");
            // Đảm bảo hướng mặt đúng
            //Flip();

            animator.SetBool("IsMoving", true);
        }
        else
        {
            // Khi đã đến activePoint
            transform.position = activePoint.position;
            CameraFollow finalCameraFollow = mainCamera.GetComponent<CameraFollow>();
            if (finalCameraFollow != null) {
                finalCameraFollow.ChangeTarget("Point");
                finalCameraFollow.ChangeSizeCamera(8);
                
            }
            animator.SetBool("IsMoving", false);
            //Debug.Log("Đã đến điểm kích hoạt.");
        }
    }
}
