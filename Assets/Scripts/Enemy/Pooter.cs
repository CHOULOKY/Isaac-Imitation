using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pooter : MonoBehaviour
{
    public float moveSpeed = 1.5f;
    public float shootingInterval = 2f;
    public float chaseDistance = 8f;
    public float stopChaseDistance = 10f;
    public float wanderRange = 2f; // 주변에서 비행할 범위

    public GameObject bulletPrefab;
    public float bulletSpeed = 5f;
    public Transform tailPosition; // 꼬리 위치에 대한 Transform

    private Transform player;
    private Rigidbody2D rb;
    private Animator animator;
    private float nextShootTime;
    private bool isChasing = false;
    private Vector2 wanderTarget;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        nextShootTime = Time.time + shootingInterval;

        // 처음 목표 위치 설정
        wanderTarget = transform.position;
    }

    void Update()
    {
        if (player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            if (distanceToPlayer <= chaseDistance)
            {
                isChasing = true;
                MoveTowardsPlayer();

                if (Time.time >= nextShootTime)
                {
                    animator.SetTrigger("isShooting"); // 발사 애니메이션 트리거
                    nextShootTime = Time.time + shootingInterval;
                }
            }
            else if (distanceToPlayer > stopChaseDistance)
            {
                isChasing = false;
            }

            if (!isChasing)
            {
                WanderAround(); // 주변 비행
            }

            FlipSprite();
        }
    }

    void MoveTowardsPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.MovePosition(rb.position + direction * moveSpeed * Time.deltaTime);
    }

    // 애니메이션 이벤트에 의해 호출되는 메서드
    void Shoot()
    {
        if (player == null || tailPosition == null) return;

        Vector2 direction = (player.position - transform.position).normalized;

        GameObject bullet = Instantiate(bulletPrefab, tailPosition.position, Quaternion.identity);
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        bulletRb.velocity = direction * bulletSpeed;

        Destroy(bullet, 3f);
    }

    void FlipSprite()
    {
        if (player.position.x < transform.position.x)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
    }

    void WanderAround()
    {
        // 주변 랜덤 위치로 조금씩 이동
        if (Vector2.Distance((Vector2)transform.position, wanderTarget) < 0.1f)
        {
            // 일정 범위 내에서만 랜덤 목표 위치 설정
            wanderTarget = (Vector2)transform.position + new Vector2(Random.Range(-wanderRange, wanderRange), Random.Range(-wanderRange, wanderRange));
        }

        Vector2 direction = (wanderTarget - (Vector2)transform.position).normalized;
        rb.MovePosition(rb.position + direction * moveSpeed * Time.deltaTime);
    }
}

