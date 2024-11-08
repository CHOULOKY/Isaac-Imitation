using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerHead : MonoBehaviour
{
    public Vector2 inputVec;
    public float speed = 5f; // 기본 속도 설정
    public InputActionAsset inputActions; // InputActionAsset 추가

    private Rigidbody2D rigid;
    private SpriteRenderer spriter;
    private Animator anim;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriter = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        // InputActionAsset 사용하여 입력값 읽기
        if (inputActions != null)
        {
            inputVec = inputActions.FindAction("Move").ReadValue<Vector2>();
        }
        else
        {
            // 기존 Input 방식
            inputVec.x = Input.GetAxis("Horizontal");
            inputVec.y = Input.GetAxis("Vertical");
        }
    }

    void FixedUpdate()
    {
        // 입력 벡터 정규화 및 이동 처리
        Vector2 nextVec = inputVec.normalized * speed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + nextVec);
    }

    void LateUpdate()
    {
        anim.SetFloat("speed", inputVec.magnitude);

        if (inputVec.x != 0)
        {
            spriter.flipX = inputVec.x < 0;
        }
    }
}
