using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBody : MonoBehaviour
{
    public Vector2 inputvec;
    public float speed = 5f;

    private Rigidbody2D rigid;
    private Animator animator;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        Vector2 nextVex = inputvec * speed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + nextVex);

        // 걷고 있는지 상태 업데이트
        animator.SetBool("isWalking", inputvec.magnitude > 0);

        // 이동 방향에 따라 애니메이션 전환
        if (inputvec.x > 0)
        {
            animator.SetFloat("moveX", 1); // 오른쪽
        }
        else if (inputvec.x < 0)
        {
            animator.SetFloat("moveX", -1); // 왼쪽
        }
        else
        {
            animator.SetFloat("moveX", 0); // 정지
        }
    }

    void OnMove(InputValue value)
    {
        inputvec = value.Get<Vector2>();
    }
}
