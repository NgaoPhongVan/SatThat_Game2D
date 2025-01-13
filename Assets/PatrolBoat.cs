using System.Collections;
using UnityEngine;

public class PatrolBoat : MonoBehaviour
{
    [Header("Điểm tuần tra")]
    public Transform patrolPoint1;
    public Transform patrolPoint2;

    [Header("Cài đặt di chuyển")]
    public float speed = 5f;
    public float stoppingDistance = 0.1f;

    [Header("Cài đặt dừng")]
    public float waitTime = 1f;

    private Transform currentTarget;
    private float currentWaitTime = 0f;
    private bool isWaiting = false;

    private void Start()
    {
        // Kiểm tra điểm tuần tra
        if (patrolPoint1 == null || patrolPoint2 == null)
        {
            Debug.LogError("Vui lòng gán cả hai điểm tuần tra trong Inspector!");
            enabled = false; // Tắt script nếu thiếu điểm tuần tra
            return;
        }

        currentTarget = patrolPoint1;
    }

    private void Update()
    {
        if (currentTarget == null) return; // Kiểm tra thêm để tránh lỗi NullReference

        if (isWaiting)
        {
            currentWaitTime += Time.deltaTime;
            if (currentWaitTime >= waitTime)
            {
                isWaiting = false;
                SwitchTarget();
            }
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, currentTarget.position, speed * Time.deltaTime);

            if (Vector3.Distance(transform.position, currentTarget.position) < stoppingDistance)
            {
                isWaiting = true;
                currentWaitTime = 0f;
            }
        }
    }

    private void SwitchTarget()
    {
        currentTarget = (currentTarget == patrolPoint1) ? patrolPoint2 : patrolPoint1;
    }

    public IEnumerator SetBoatColliderable(bool isEnable)
    {
        if (!gameObject.activeSelf) { yield break; }
        yield return new WaitForSeconds(isEnable ? 0.25f : 0);
        var boats = GameObject.FindGameObjectsWithTag("boat");
        foreach (var boat in boats)
        {
            if (boat != gameObject)
            {
                boat.GetComponent<BoxCollider2D>().enabled = boat.GetComponent<EdgeCollider2D>().enabled = isEnable;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            StartCoroutine(SetBoatColliderable(false));
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            StartCoroutine(SetBoatColliderable(false));
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            StartCoroutine(SetBoatColliderable(true));
        }
    }
}
