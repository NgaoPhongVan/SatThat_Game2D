using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TigerBossController : MonoBehaviour
{
  
    [SerializeField] private Camera mainCamera;
    
    public Transform activePoint;
    public float speed = 2f;
    public Rigidbody2D rb;
    public Transform player;
    

    //Animation
    private Animator animator;
    private bool isAttacking = false;
    private bool isDeath = false;

    //Health Bar
    public GameObject healthBarCanvas; // Canvas hiển thị thanh máu
    private HealthSystem healthSystem;


    //Active
    private bool hasReachedActivePoint = false;
    private bool isActived = false;
    private bool isPhase2 = false;

    private void Start()
    {
        animator = GetComponent<Animator>();



        // Kiểm tra xem player hoặc activePoint có bị thiếu hay không
        if (player == null || activePoint == null || healthBarCanvas == null)
        {
            Debug.LogError("Player or ActivePoint or HealthBarCanvas is not assigned in the Inspector!");
        }

        if (healthBarCanvas != null)
        {
            healthBarCanvas.SetActive(false);
        }

        healthSystem = GetComponent<HealthSystem>();
        if (healthSystem != null)
        {
            healthSystem.OnHealthChanged.AddListener(CheckHealth);
            healthSystem.OnDeath.AddListener(OnDeath);
        }
        if (healthSystem == null)
        {
            Debug.LogError("HealthSystem component is missing on player!");
        }

        animator.SetBool("IsMoving", false);
    }



    private void FixedUpdate()
    {
        if (isDeath) return;


        Flip();
        if (isActived)
        {
            MoveToActivePoint();


            MoveTowardsPlayer();

        }
        
    }

    private void CheckHealth(float healthPercentage)
    {
        if (healthPercentage <= 0 && !isDeath)
        {
            OnDeath();
        }
    }

    private void Flip()
    {
        Vector3 scale = transform.localScale;

        // Đảo hướng theo vị trí của người chơi
        if (player.position.x > transform.position.x && scale.x < 0 ||
            player.position.x < transform.position.x && scale.x > 0)
        {
            scale.x *= -1; // Đổi hướng trục X
            transform.localScale = scale;
        }
    }


    public void OnDeath()
    {
        if (isDeath) return;
        isDeath = true;
        CameraChap2Controller finalCameraFollow = mainCamera.GetComponent<CameraChap2Controller>();
        if (finalCameraFollow != null)
        {
            finalCameraFollow.ChangeTarget("Player");
            finalCameraFollow.ChangeSizeCamera();
        }
        animator.SetTrigger("IsDead");
        StartCoroutine(EndChapter());
        Debug.Log("Tiger Boss is dead.");
    }

    private void EndChapterAndChangeDuty()
    {
        if (QuestSystem.QuestManager.Instance.CanStartQuest("Destroy_BossTiger"))
        {

            QuestSystem.QuestManager.Instance.CompleteQuest("Destroy_BossTiger");
            QuestSystem.QuestManager.Instance.StartQuest("BreakIn_Barracks");
            SceneManager.LoadScene("Scene2_DoanhTrai");
        }
        else
        {
            PickupTextManager.Instance.ShowPickupText("B?n c?n hoàn thành nhi?m v? rèn ki?m tr??c!");
            Debug.Log("B?n c?n hoàn thành nhi?m v? rèn ki?m tr??c!");
        }
    }

    IEnumerator EndChapter() { yield return new WaitForSeconds(1f); EndChapterAndChangeDuty(); }

    [SerializeField] private float attackDamage = 50f; // Sát thương của Boss
    [SerializeField] private Transform attackPoint; // Điểm tấn công
    [SerializeField] private float attackRange = 3.5f; // Phạm vi tấn công

    IEnumerator AttackPlayer()
    {

        if (isDeath || isAttacking) yield break;

        isAttacking = true; // Đặt trạng thái tấn công
        animator.SetBool("IsMoving", false);

        while (Vector3.Distance(transform.position, player.position) <= attackRange)
        {
            if (isDeath) break;

            // Thực hiện tấn công
            Flip();
            animator.SetTrigger("IsAttacking");


            Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, LayerMask.GetMask("Player"));
            foreach (Collider2D player in hitPlayers)
            {
                player.GetComponent<HealthSystem>()?.TakeDamage(attackDamage);
            }

            // Chờ trước khi tấn công lần tiếp theo
            yield return new WaitForSeconds(1f);

            // Kiểm tra lại khoảng cách để quyết định có tiếp tục tấn công không
            if (Vector3.Distance(transform.position, player.position) > attackRange)
            {
                break;
            }
        }

        // Reset trạng thái khi kết thúc tấn công
        isAttacking = false;
        animator.ResetTrigger("IsAttacking");

    }



    private void MoveTowardsPlayer()
    {
        if (isDeath) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > attackRange)
        {
            if (!isAttacking) // Chỉ di chuyển nếu không tấn công
            {
                Vector3 direction = player.position - transform.position;
                transform.position += direction.normalized * speed * Time.deltaTime;
                animator.ResetTrigger("IsAttacking");
                animator.SetBool("IsMoving", true);
            }
        }
        else
        {
            animator.SetBool("IsMoving", false);
            if (!isAttacking)
            {
                StartCoroutine(AttackPlayer()); // Chỉ bắt đầu tấn công nếu chưa trong trạng thái tấn công
            }
        }
    }

    public void ActiveBoss()
    {
        isActived = true;
    }
    void MoveToActivePoint()
    {
        if (!hasReachedActivePoint)
        {
            float distanceToActivePoint = Vector3.Distance(transform.position, activePoint.position);
            if (distanceToActivePoint > 1f)
            {
                Vector3 direction = activePoint.position - transform.position;
                transform.position += direction.normalized * speed * Time.deltaTime;

                animator.SetBool("IsMoving", true);
                

                return; // Ngừng thực hiện phần còn lại để hoàn thành di chuyển đến activePoint trước
            }
            else
            {
                // Khi đã đến activePoint
                transform.position = activePoint.position;
                CameraChap2Controller finalCameraFollow = mainCamera.GetComponent<CameraChap2Controller>();
                if (finalCameraFollow != null)
                {
                    finalCameraFollow.ChangeTarget("Point");
                    finalCameraFollow.ChangeSizeCamera(8);
                }

                animator.SetBool("IsMoving", false);
               

                hasReachedActivePoint = true; // Cập nhật trạng thái

                if (healthBarCanvas != null)
                {
                    
                    healthBarCanvas.SetActive(true); // Bật thanh máu
                }   

                return; // Dừng lại tại đây sau khi đến activePoint
            }
        }
    }


    private void OnDrawGizmosSelected()
    {
        // Kiểm tra xem activePoint và player có được gán hay khôn

        if (player != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }

    private void OnDestroy()
    {
        if (healthSystem != null)
        {
            //healthSystem.OnHealthChanged.RemoveListener(UpdateHealthBar);
            healthSystem.OnHealthChanged.RemoveListener(CheckHealth);
            healthSystem.OnDeath.RemoveListener(OnDeath);
        }
    }
}
