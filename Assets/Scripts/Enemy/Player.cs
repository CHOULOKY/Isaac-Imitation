using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // 기본 이동 속도
    [SerializeField]
    private float moveSpeed = 5f;

    // Rigidbody2D와 Animator 컴포넌트
    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 movement;

    // 이동 속도를 외부에서 설정하는 프로퍼티
    public float MoveSpeed
    {
        get { return moveSpeed; }
        set { moveSpeed = Mathf.Max(0, value); } // 속도가 음수가 되지 않도록 설정
    }

    // 초기 설정
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    // 프레임마다 호출되는 메서드
    void Update()
    {
        // 입력을 감지하여 이동 방향 설정
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // 애니메이션 업데이트
        UpdateAnimation();
    }

    // 물리적 이동을 처리하는 FixedUpdate 메서드
    void FixedUpdate()
    {
        Move();
    }

    // 이동 처리 메서드
    void Move()
    {
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    // 애니메이션 처리 메서드
    void UpdateAnimation()
    {
        if (movement != Vector2.zero)
        {
            animator.SetFloat("Horizontal", movement.x);
            animator.SetFloat("Vertical", movement.y);
            animator.SetBool("isMoving", true);
        }
        else
        {
            animator.SetBool("isMoving", false);
        }
    }

    // 이동 속도를 변경하는 메서드
    public void SetMoveSpeed(float newSpeed)
    {
        MoveSpeed = newSpeed;
    }
}
