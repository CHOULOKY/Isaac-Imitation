using System.Collections;
using UnityEngine;

public class MonstroAI : MonoBehaviour
{
    public float jumpCooldown = 2f;         // 점프 쿨타임 (초 단위)
    public float vomitCooldown = 3f;        // 구토 공격 쿨타임 (초 단위)
    public float diveCooldown = 5f;         // 강하 공격 쿨타임 (초 단위)
    public float jumpForce = 2f;            // 점프 힘 (조정됨)
    public float highJumpForce = 4f;        // 강하 공격 시 높은 점프 힘 (조정됨)
    public GameObject projectilePrefab;     // 구토 공격에 사용할 탄환 프리팹
    public int projectileCount = 5;         // 구토 공격 탄환 개수
    public float projectileSpeed = 1.5f;    // 탄환 속도 (조정됨)
    public float jumpRange = 1f;            // 랜덤 점프 거리 (조정됨)
    public float diveImpactRadius = 1f;     // 강하 공격 착지 시 충격파 범위 (조정됨)
    public float diveImpactForce = 5f;      // 충격파 힘 (조정됨)

    private bool isJumping = false;         // 점프 중 여부 체크
    private bool isDiving = false;          // 강하 공격 중 여부 체크
    private float nextJumpTime = 0f;        // 다음 점프 시간
    private float nextVomitTime = 0f;       // 다음 구토 공격 시간
    private float nextDiveTime = 0f;        // 다음 강하 공격 시간
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        nextJumpTime = Time.time + jumpCooldown;
        nextVomitTime = Time.time + vomitCooldown;
        nextDiveTime = Time.time + diveCooldown;
    }

    void Update()
    {
        if (Time.time >= nextJumpTime && !isDiving)
        {
            Jump();
            nextJumpTime = Time.time + jumpCooldown;
        }

        if (Time.time >= nextVomitTime && !isDiving)
        {
            VomitAttack();
            nextVomitTime = Time.time + vomitCooldown;
        }

        if (Time.time >= nextDiveTime && !isJumping)
        {
            StartCoroutine(DiveAttack());
            nextDiveTime = Time.time + diveCooldown;
        }
    }

    void Jump()
    {
        if (isJumping) return;
        isJumping = true;

        Vector2 jumpTarget = new Vector2(
            transform.position.x + Random.Range(-jumpRange, jumpRange),
            transform.position.y
        );

        Vector2 jumpDirection = (jumpTarget - (Vector2)transform.position).normalized;
        rb.AddForce(jumpDirection * jumpForce, ForceMode2D.Impulse);

        Invoke("EndJump", 0.5f);
    }

    void EndJump()
    {
        isJumping = false;
    }

    void VomitAttack()
    {
        for (int i = 0; i < projectileCount; i++)
        {
            float angle = i * (360f / projectileCount);
            Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.up;

            GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            Rigidbody2D projectileRb = projectile.GetComponent<Rigidbody2D>();
            projectileRb.velocity = direction * projectileSpeed;
        }
    }

    IEnumerator DiveAttack()
    {
        isDiving = true;

        // 높이 점프 (조정된 점프 힘 사용)
        rb.AddForce(Vector2.up * highJumpForce, ForceMode2D.Impulse);

        // 일정 시간 기다린 후 강하
        yield return new WaitForSeconds(1.2f);

        // 빠르게 내려오기
        rb.AddForce(Vector2.down * (highJumpForce * 1.5f), ForceMode2D.Impulse);

        yield return new WaitUntil(() => Mathf.Abs(rb.velocity.y) < 0.1f); // 착지할 때까지 대기

        // 충격파 발생
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, diveImpactRadius);
        foreach (Collider2D hit in hitColliders)
        {
            Rigidbody2D hitRb = hit.GetComponent<Rigidbody2D>();
            if (hitRb != null)
            {
                Vector2 impactDirection = (hit.transform.position - transform.position).normalized;
                hitRb.AddForce(impactDirection * diveImpactForce, ForceMode2D.Impulse);
            }
        }

        // 강하 공격 종료
        isDiving = false;
    }

    void OnDrawGizmosSelected()
    {
        // 강하 공격 충격파 범위 표시
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, diveImpactRadius);
    }
}


