using UnityEngine;

public class Gaper_body : MonoBehaviour
{
    public float moveSpeed = 2f; // 이동 속도
    public Transform player; // 추적할 플레이어의 위치
    public Animator animator; // 애니메이터
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private Vector2 direction; // 이동 방향
    private Vector2 startPosition; // Gaper의 시작 위치
    public float movementRadius = 3f; // 이동할 수 있는 범위

    private bool isMovingSide = false; // 좌우 이동 상태 추적

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        startPosition = transform.position; // 초기 위치 저장
    }

    private void Update()
    {
        MoveTowardsPlayer();
        UpdateAnimation();
    }

    // 플레이어를 향해 이동 (이동 범위 제한)
    private void MoveTowardsPlayer()
    {
        Vector2 targetPosition = player.position;
        direction = (targetPosition - (Vector2)transform.position).normalized;

        Vector2 desiredPosition = (Vector2)transform.position + direction * moveSpeed * Time.deltaTime;
        Vector2 offset = desiredPosition - startPosition;

        if (offset.magnitude < movementRadius)
        {
            rb.velocity = direction * moveSpeed;
        }
        else
        {
            rb.velocity = Vector2.zero; // 범위를 넘으면 이동을 멈춤
        }
    }

    // 애니메이션 상태 업데이트
    private void UpdateAnimation()
    {
        float absX = Mathf.Abs(direction.x);
        float absY = Mathf.Abs(direction.y);

        // 애니메이션이 번갈아 나오는 문제 해결
        if (absX > absY) // 좌우로 이동할 때
        {
            if (!isMovingSide)
            {
                animator.Play("Gaper-Move-side");
                isMovingSide = true;
            }
            spriteRenderer.flipX = direction.x < 0; // 좌우 방향 전환
        }
        else // 상하로 이동할 때
        {
            if (isMovingSide)
            {
                animator.Play("Gaper-Move-idle");
                isMovingSide = false;
            }

            // 위로 이동 시 좌우 반전된 idle 애니메이션
            if (direction.y > 0)
            {
                spriteRenderer.flipX = true; // 위로 이동 시 flipX 설정
            }
            else
            {
                spriteRenderer.flipX = false; // 아래로 이동 시 기본 설정
            }
        }
    }
}

