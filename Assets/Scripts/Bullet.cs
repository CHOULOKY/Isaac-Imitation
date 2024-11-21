using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damage;
    public int per;

    private Rigidbody2D rigid; // Rigid 필드를 선언합니다.

    void Awake()
    {
        // Rigidbody2D 컴포넌트를 가져옵니다.
        rigid = GetComponent<Rigidbody2D>();
    }

    public void Init(float damage, int per, Vector3 dir)
    {
        this.damage = damage;
        this.per = per;

        if (per > -1)
        {
            // 초기 속도를 설정합니다.
            rigid.velocity = dir;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // "Enemy" 태그가 아니거나 per 값이 -1이면 아무 작업도 하지 않습니다.
        if (!collision.CompareTag("Enemy") || per == -1)
            return;

        per--;

        if (per == -1)
        {
            // 속도를 0으로 설정하고, 객체를 비활성화합니다.
            rigid.velocity = Vector2.zero;
            gameObject.SetActive(false);
        }
    }
}
