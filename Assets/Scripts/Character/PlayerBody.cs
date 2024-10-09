using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBody : MonoBehaviour
{
    private PlayerHead head;

    private Rigidbody2D rigid;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private Vector2 inputVec;
    public float moveForce;
    public float maxVelocity;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        head = GetComponentInChildren<PlayerHead>();
    }

    private void Update()
    {
        inputVec.x = Input.GetAxisRaw("Horizontal WASD");
        inputVec.y = Input.GetAxisRaw("Vertical WASD");

        if (inputVec.x > 0) {
            spriteRenderer.flipX = false;
        } else if (inputVec.x < 0) {
            spriteRenderer.flipX = true;
        }
        animator.SetInteger("XAxisRaw", (int)inputVec.x);
        animator.SetInteger("YAxisRaw", (int)inputVec.y);
    }

    private void FixedUpdate()
    {
        rigid.AddForce(inputVec.normalized * moveForce, ForceMode2D.Force);
        if (rigid.velocity.magnitude > maxVelocity) {
            rigid.velocity = rigid.velocity.normalized * maxVelocity;
        }
    }
}
