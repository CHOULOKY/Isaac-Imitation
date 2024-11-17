using System.Collections;
using UnityEngine;

public class RandomBossEnemy : MonoBehaviour
{
    public float normalSpeed = 2f; // 기본 이동 속도
    public float fastSpeed = 5f; // 빠르게 움직일 때의 속도
    public float moveRangeX = 5f; // X축 이동 범위
    public float moveRangeY = 5f; // Y축 이동 범위

    private Rigidbody2D rigid;
    private Vector2 initialPosition; // 보스의 처음 위치
    private Vector2 randomDirection; // 랜덤하게 움직일 방향
    private float currentSpeed; // 현재 속도
    private bool isResting = false; // 보스가 멈춘 상태인지 여부

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        initialPosition = rigid.position; // 보스의 처음 위치 설정
        currentSpeed = normalSpeed; // 초기 속도를 기본 속도로 설정
        StartCoroutine(ChangeRandomDirection()); // 주기적으로 이동 방향 변경
        StartCoroutine(RandomPatternCycle()); // 랜덤한 패턴 주기적으로 실행
    }

    void FixedUpdate()
    {
        if (isResting) return; // 정지 상태라면 움직이지 않음
        MoveRandomly(); // 일반적인 랜덤 이동
    }

    // 패턴 주기적으로 실행 (속도 변화 또는 잠시 멈추기)
    IEnumerator RandomPatternCycle()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(3f, 6f)); // 3초~6초 간격으로 패턴 변화
            int pattern = Random.Range(0, 2); // 2가지 패턴 중 하나 선택

            if (pattern == 0)
            {
                StartCoroutine(SpeedChangePattern());
            }
            else if (pattern == 1)
            {
                StartCoroutine(RestAndMove());
            }
        }
    }

    // 일정 시간 동안 빠르게 이동했다가 다시 정상 속도로 돌아오는 패턴
    IEnumerator SpeedChangePattern()
    {
        currentSpeed = fastSpeed; // 빠르게 이동
        yield return new WaitForSeconds(2f); // 2초 동안 빠르게 이동
        currentSpeed = normalSpeed; // 다시 정상 속도로 돌아옴
    }

    // 잠시 멈췄다가 다시 움직이는 패턴
    IEnumerator RestAndMove()
    {
        isResting = true; // 보스 멈춤
        yield return new WaitForSeconds(1.5f); // 1.5초 동안 정지
        isResting = false; // 다시 이동
    }

    // 일정 시간마다 랜덤한 방향으로 이동
    IEnumerator ChangeRandomDirection()
    {
        while (true)
        {
            if (!isResting) // 정지 상태가 아니면 방향을 바꾼다
            {
                randomDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
            }
            yield return new WaitForSeconds(2f); // 2초마다 방향 변경
        }
    }

    // 랜덤하게 정해진 방향으로 이동
    void MoveRandomly()
    {
        Vector2 nextVec = randomDirection * currentSpeed * Time.fixedDeltaTime;
        Vector2 newPosition = rigid.position + nextVec;

        // 이동 범위를 벗어나지 않도록 제한
        if (newPosition.x >= initialPosition.x + moveRangeX || newPosition.x <= initialPosition.x - moveRangeX)
        {
            nextVec.x = 0; // 범위를 벗어나면 X축 방향 멈춤
        }

        if (newPosition.y >= initialPosition.y + moveRangeY || newPosition.y <= initialPosition.y - moveRangeY)
        {
            nextVec.y = 0; // 범위를 벗어나면 Y축 방향 멈춤
        }

        rigid.MovePosition(rigid.position + nextVec);
    }

    void LateUpdate()
    {
        // 시각적 효과: 보스가 움직이는 방향에 따라 스프라이트를 뒤집음
        if (randomDirection.x != 0)
        {
            GetComponent<SpriteRenderer>().flipX = randomDirection.x < 0;
        }
    }
}
