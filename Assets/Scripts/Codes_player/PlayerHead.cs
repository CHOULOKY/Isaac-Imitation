using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHead_0 : MonoBehaviour
{
    public Vector2 inputvec;
    public float speed; // speed 변수를 추가합니다.

    Rigidbody2D rigid;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        inputvec.x = Input.GetAxis("Horizontal"); // 대문자로 수정
        inputvec.y = Input.GetAxis("Vertical");   // 대문자로 수정
    }

    void FixedUpdate()
    {
        Vector2 nextVex = inputvec.normalized * speed * Time.fixedDeltaTime; // 오타 수정 및 일관성 유지
        rigid.MovePosition(rigid.position + nextVex);
    }
}
