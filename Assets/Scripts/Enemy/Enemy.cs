using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed;
    public float moveRangeX; // 적이 이동할 수 있는 X 축의 범위
    public float moveRangeY; // 적이 이동할 수 있는 Y 축의 범위

    private Rigidbody2D rigid;
    private Vector2 initialPosition; // 적이 처음 위치한 지점 (이동 범위 계산에 사용)
    private Vector2 randomDirection; // 랜덤하게 움직일 방향
    private float changeDirectionTime = 2.0f; // 랜덤 이동 방향을 변경할 주기

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        initialPosition = rigid.position; // 적의 처음 위치 설정
        StartCoroutine(ChangeRandomDirection()); // 주기적으로 이동 방향 변경
    }

    void FixedUpdate()
    {
        MoveRandomly(); // 랜덤한 방향으로 이동
    }

    IEnumerator ChangeRandomDirection()
    {
        while (true)
        {
            // 2초마다 랜덤한 방향을 설정
            randomDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
            yield return new WaitForSeconds(changeDirectionTime);
        }
    }

    void MoveRandomly()
    {
        Vector2 nextVec = randomDirection * speed * Time.fixedDeltaTime;
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
        if (randomDirection.x != 0)
        {
            // 좌우 방향에 따라 스프라이트를 뒤집음
            GetComponent<SpriteRenderer>().flipX = randomDirection.x < 0;
        }
    }
}