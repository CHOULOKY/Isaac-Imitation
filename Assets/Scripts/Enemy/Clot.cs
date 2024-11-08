using System.Collections;
using UnityEngine;

public class Clot : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float moveDuration = 1.5f;
    public float waitDuration = 1f;
    public float movementRadius = 5f; // 행동 반경 설정

    [Header("Shooting Settings")]
    public GameObject bulletPrefab;
    public float bulletSpeed = 5f;
    public Transform mouthPosition; // 눈물이 발사될 입 부분 위치

    private Vector2 startingPosition; // 행동 반경의 중심점
    private Rigidbody2D rb;
    private Animator animator;
    private bool isMoving = true; // 현재 이동 중인지 여부
    private bool isShooting = false; // 공격 중인지 여부
    private bool canShoot = true; // 한 번의 공격만 실행되도록 제어

    private SpriteRenderer spriteRenderer; // SpriteRenderer 추가

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>(); // SpriteRenderer 초기화

        // Clot의 초기 위치를 중심점으로 설정
        startingPosition = transform.position;

        // 초기 이동 코루틴 시작
        StartCoroutine(MoveShootCycle());
    }

    // 이동과 공격을 번갈아 수행
    private IEnumerator MoveShootCycle()
    {
        while (true)
        {
            if (isMoving)
            {
                // 이동 상태
                animator.SetBool("isMoving", true);
                yield return MoveWithinRadius();
                animator.SetBool("isMoving", false);

                isMoving = false; // 다음은 공격 상태로 전환
                canShoot = true; // 공격을 할 수 있도록 설정
            }
            else
            {
                // 공격 상태
                if (canShoot && !isShooting)
                {
                    isShooting = true; // 공격 시작
                    animator.SetTrigger("clot-shoot");

                    // 공격이 끝날 때까지 대기
                    yield return new WaitForSeconds(waitDuration);

                    isShooting = false; // 공격 종료
                    canShoot = false; // 다음 이동 전까지 추가 공격 방지
                }

                isMoving = true; // 다음은 이동 상태로 전환
            }
        }
    }

    // 반경 내에서 랜덤 이동
    private IEnumerator MoveWithinRadius()
    {
        Vector2 targetPosition;
        float timer = 0f;

        // 반경 내 랜덤한 위치 설정
        do
        {
            Vector2 randomDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
            targetPosition = startingPosition + randomDirection * Random.Range(0f, movementRadius);

        } while (Vector2.Distance(startingPosition, targetPosition) > movementRadius);

        // 목표 지점까지 이동
        while (timer < moveDuration)
        {
            Vector2 moveDirection = (targetPosition - (Vector2)transform.position).normalized;
            rb.velocity = moveDirection * moveSpeed;

            // 이동 방향에 따라 스프라이트 회전
            FlipSprite(moveDirection.x);

            timer += Time.deltaTime;

            // 목표 지점에 가까워지면 이동 멈춤
            if (Vector2.Distance(transform.position, targetPosition) < 0.2f)
                break;

            yield return null;
        }

        rb.velocity = Vector2.zero;
    }

    // 스프라이트 방향 전환
    private void FlipSprite(float moveDirectionX)
    {
        if (moveDirectionX < 0)
        {
            spriteRenderer.flipX = true; // 왼쪽으로 이동할 때 스프라이트 좌우 반전
        }
        else if (moveDirectionX > 0)
        {
            spriteRenderer.flipX = false; // 오른쪽으로 이동할 때 원래 방향으로
        }
    }

    // 애니메이션 이벤트에서 호출되는 Shoot 메서드
    public void Shoot()
    {
        if (isShooting)
        {
            ShootFromMouth();
        }
    }

    // 입에서 X자 방향으로 4방향 발사
    private void ShootFromMouth()
    {
        Vector2[] directions = new Vector2[]
        {
            new Vector2(1, 1),   // 오른쪽 위
            new Vector2(-1, 1),  // 왼쪽 위
            new Vector2(1, -1),  // 오른쪽 아래
            new Vector2(-1, -1)  // 왼쪽 아래
        };

        foreach (Vector2 dir in directions)
        {
            GameObject bullet = Instantiate(bulletPrefab, mouthPosition.position, Quaternion.identity);
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            bulletRb.velocity = dir.normalized * bulletSpeed;

            // 3초 후 탄환 자동 파괴
            Destroy(bullet, 3f);
        }
    }
}




