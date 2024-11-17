using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gaper_Head : MonoBehaviour
{
    public Transform player; // 플레이어 위치를 추적하기 위해 할당
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        FlipTowardsPlayer();
    }

    // 플레이어 위치에 따라 Gaper_head의 좌우 반전 처리
    private void FlipTowardsPlayer()
    {
        if (player != null)
        {
            // 플레이어가 Gaper_head보다 오른쪽에 있을 경우
            if (player.position.x > transform.position.x)
            {
                spriteRenderer.flipX = true; // 오른쪽을 바라보게 좌우 반전
            }
            else
            {
                spriteRenderer.flipX = false; // 왼쪽을 바라보게 원래 상태 유지
            }
        }
    }
}
