
using UnityEngine;

public class BossTigerMovement : MonoBehaviour
{
    public float patrolSpeed = 2f;          // Tốc độ di chuyển khi tuần tra
    public float chaseSpeed = 6f;          // Tốc độ di chuyển khi đuổi theo
    public float patrolRange = 5f;         // Khoảng cách tuần tra
    public float detectionRange = 10f;     // Phạm vi phát hiện người chơi
    [SerializeField] private Transform player;               // Vị trí của người chơi
    [SerializeField] private Animator animator;
    public float attackRange = 3f;         // Phạm vi để Boss tấn công

    private Vector3 startPosition;         // Vị trí bắt đầu tuần tra
    private bool movingRight = true;       // Hướng tuần tra
    private bool isChasing = false;        // Trạng thái đuổi theo
    private bool isActivated = false;      // Trạng thái của Boss (kích hoạt hay chưa)
    private bool reachedTarget = false;    // Trạng thái đã đến vị trí chỉ định hay chưa

    public Transform activationPoint;      // Điểm kích hoạt
    public Transform targetPosition;       // Vị trí đích
    public LayerMask groundLayer;          // Chỉ định GroundLayer
    public float groundCheckDistance = 1f; // Khoảng cách kiểm tra mặt đất

    void Start()
    {
        startPosition = transform.position; // Lưu vị trí ban đầu
    }

    void Update()
    {
        if (!isActivated)
        {
            // Kích hoạt Boss khi người chơi đến gần activationPoint
            float distanceToActivationPoint = Vector3.Distance(player.position, activationPoint.position);
            if (distanceToActivationPoint <= detectionRange)
            {
                isActivated = true;
                Debug.Log("Boss Tiger đã được kích hoạt!");
            }
        }
        else if (!reachedTarget)
        {
            // Boss đang di chuyển đến vị trí chỉ định
            MoveToTarget();
            return; // Chỉ xử lý việc di chuyển đến targetPosition
        }

        if (reachedTarget) // Sau khi đến targetPosition, tiếp tục xử lý các hành động khác
        {
            if (player != null)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, player.position);

                if (distanceToPlayer <= detectionRange && distanceToPlayer > attackRange)
                {
                    ChasePlayer(); // Đuổi theo người chơi
                }
                else if (distanceToPlayer <= attackRange)
                {
                    AttackPlayer(); // Tấn công người chơi
                }
                else
                {
                    Patrol(); // Tuần tra
                }
            }
            else
            {
                Patrol(); // Tuần tra mặc định
            }
        }
    }

    void MoveToTarget()
    {
        animator.SetBool("isRunning", true); // Kích hoạt hoạt ảnh chạy
        Vector3 direction = (targetPosition.position - transform.position).normalized;
        Move(direction, patrolSpeed);

        // Kiểm tra nếu Boss đã đến gần vị trí đích
        if (Vector3.Distance(transform.position, targetPosition.position) <= 0.1f)
        {
            reachedTarget = true; // Đánh dấu đã đến vị trí đích
            startPosition = transform.position;
            CameraFollow cameraFollow = FindObjectOfType<CameraFollow>();
            cameraFollow.ChangeTarget();

            Debug.Log("Boss Tiger đã đến vị trí chỉ định!");
        }
    }

    void Patrol()
    {
        isChasing = false;
        animator.SetBool("isRunning", true); // Kích hoạt hoạt ảnh chạy

        float patrolEnd = startPosition.x + patrolRange;
        float patrolStart = startPosition.x - patrolRange;

        Vector3 direction = movingRight ? Vector3.right : Vector3.left;
        Move(direction, patrolSpeed);

        if (transform.position.x >= patrolEnd)
            movingRight = false;
        else if (transform.position.x <= patrolStart)
            movingRight = true;
    }

    void ChasePlayer()
    {
        animator.SetBool("isRunning", true);
        isChasing = true;
        Vector3 direction = (player.position - transform.position).normalized;
        Move(direction, chaseSpeed);
    }

    void Move(Vector3 direction, float speed)
    {
        transform.position += direction * speed * Time.deltaTime;

        // Kiểm tra mặt đất và căn chỉnh vị trí
        RaycastHit2D hit = Physics2D.Raycast(transform.position + Vector3.up, Vector2.down, groundCheckDistance, groundLayer);
        if (hit.collider != null)
        {
            transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
        }
        else
        {
            Debug.LogWarning("Boss Tiger không tìm thấy mặt đất!");
        }
    }


    void AttackPlayer()
    {
        animator.SetTrigger("Attack");
        // Logic tấn công, có thể là chơi animation hoặc gây sát thương
        Debug.Log("Boss Tiger đang tấn công người chơi!");
    }

    private void OnDrawGizmos()
    {
        // Hiển thị phạm vi tuần tra và phát hiện người chơi
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(startPosition, patrolRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position + Vector3.up, transform.position + Vector3.up + Vector3.down * groundCheckDistance);
    }
}
