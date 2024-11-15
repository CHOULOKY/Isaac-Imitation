using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed;
    public float health;
    public float maxHealth;
    public RuntimeAnimatorController[] animCon;
    public Rigidbody2D target;

    bool isLive;

    Rigidbody2D rigid;
    Animator anim; // Animator를 저장할 변수
    SpriteRenderer spriter;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>(); // Animator 컴포넌트를 가져옵니다.
        spriter = GetComponent<SpriteRenderer>();
    }

    void FixedUpdate()
    {
        if (!isLive)
            return;

        Vector2 dirVec = target.position - rigid.position;
        Vector2 nextVec = dirVec.normalized * speed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + nextVec);
        rigid.velocity = Vector2.zero; // 'regid'를 'rigid'로 수정
    }

    void LateUpdate()
    {
        if (!isLive)
            return;

        spriter.flipX = target.position.x < rigid.position.x;
    }

    void OnEnable() // 'InEnable'을 'OnEnable'로 수정
    {
        target = GameManager.instance.player.GetComponent<Rigidbody2D>(); // 'rigid'를 'Rigidbody2D'로 수정
        isLive = true;
        health = maxHealth;
    }

    public void Init(SpawnData data)
    {
        anim.runtimeAnimatorController = animCon[data.spriteType]; // 'animCon'에서 'anim'으로 수정
        speed = data.speed;
        maxHealth = data.maxHealth;
        health = data.health;
    }

    void OnTriggerEnter2D(Collider2D collision) // 'OnTrigger2D'를 'OnTriggerEnter2D'로 수정
    {
        if (collision.CompareTag("Bullet"))
            return;

        health -= collision.GetComponent<Bullet>().damage;

        if (health > 0)
        {
            //..Live Hit Action
        }
        else
        {
            //..Die
            Dead();
        }
    }

    void Dead()
    {
        gameObject.SetActive(false);
    }
}
