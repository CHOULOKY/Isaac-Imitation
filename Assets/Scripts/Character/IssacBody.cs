using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IssacBody : MonoBehaviour
{
    private IssacHead head;

    private Rigidbody2D rigid;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private Vector2 inputVec;
    public float moveForce = 20; // +5
    public float maxVelocity = 5; // moveForce/5 + 1

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        head = GetComponentInChildren<IssacHead>();
    }

    private void Update()
    {
        GetInputVec();

        SetBodyDirection();
    }

    private void FixedUpdate()
    {
        MoveBody();
    }

    private void GetInputVec()
    {
        inputVec.x = Input.GetAxisRaw("Horizontal WASD");
        inputVec.y = Input.GetAxisRaw("Vertical WASD");
    }

    private void SetBodyDirection()
    {
        if (inputVec.x > 0) {
            spriteRenderer.flipX = false;
        }
        else if (inputVec.x < 0) {
            spriteRenderer.flipX = true;
        }
        animator.SetInteger("XAxisRaw", (int)inputVec.x);
        animator.SetInteger("YAxisRaw", (int)inputVec.y);
    }

    private void MoveBody()
    {
        rigid.AddForce(inputVec.normalized * moveForce, ForceMode2D.Force);
        if (rigid.velocity.magnitude > maxVelocity) {
            rigid.velocity = rigid.velocity.normalized * maxVelocity;
        }
    }
}
