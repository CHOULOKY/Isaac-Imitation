using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackFly : MonoBehaviour
{
    // 이동 속도
    public float normalSpeed = 2f;    // 평소 속도
    public float chaseSpeed = 4f;     // 추적 시 속도
    public float distanceThreshold = 5f; // 플레이어 추적을 시작할 거리

    private Transform player;
    private Rigidbody2D rb;
    private float currentSpeed;

    // 초기 설정
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
        currentSpeed = normalSpeed; // 초기 속도는 평소 속도
    }

    // 프레임마다 호출되는 메서드
    void Update()
    {
        if (player != null)
        {
            // 플레이어와의 거리 계산
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            // 플레이어가 일정 거리 내에 있으면 추적 모드로 전환
            if (distanceToPlayer <= distanceThreshold)
            {
                currentSpeed = chaseSpeed;
                ChasePlayer();
            }
            else
            {
                currentSpeed = normalSpeed;
                MoveRandomly();
            }
        }
    }

    // 플레이어를 추적하는 메서드
    void ChasePlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.MovePosition(rb.position + direction * currentSpeed * Time.deltaTime);
    }

    // 평소에 천천히 움직이는 메서드 (랜덤 움직임)
    void MoveRandomly()
    {
        Vector2 randomDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
        rb.MovePosition(rb.position + randomDirection * currentSpeed * Time.deltaTime);
    }
}
