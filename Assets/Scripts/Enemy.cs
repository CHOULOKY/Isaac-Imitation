using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed;
    public Rigidbody2D target;

    private bool isLive = true;
    private Rigidbody2D rigid;
    private SpriteRenderer spriter;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>(); // 오타 수정
        spriter = GetComponent<SpriteRenderer>();
    }

    void FixedUpdate()
    {
        if (!isLive)
            return;

        Vector2 dirVec = target.position - rigid.position;
        Vector2 nextVec = dirVec.normalized * speed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + nextVec);
        // rigid.velocity = Vector2.zero; // 필요 없음
    }

    void LateUpdate()
    {
        if (!isLive)
            return;
        spriter.flipX = target.position.x < rigid.position.x;
    }
}
