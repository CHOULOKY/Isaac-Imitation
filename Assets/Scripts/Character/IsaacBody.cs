using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IsaacBody : MonoBehaviour
{
    private IsaacHead head;

    private Rigidbody2D rigid;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private FlashEffect flashEffect;

    private Vector2 inputVec;
    public float moveForce = 20; // +5
    public float maxVelocity = 5; // moveForce/5 + 1

    public int health;
    public int maxHealth;

    private void Awake()
    {
        head = GetComponentInChildren<IsaacHead>();

        rigid = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        flashEffect = GetComponent<FlashEffect>();
    }

    private void OnEnable()
    {
        health = maxHealth;
    }

    private void Update()
    {
        GetInputVec();

        SetBodyDirection();

        // test code
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            IsHurt = true;
        }
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

    private bool isHurt = false;
    public bool IsHurt
    {
        get { return isHurt; }
        set {
            if (isHurt != value) {
                isHurt = value;
                if (health <= 0) {
                    IsDeath = true;
                    return;
                }

                this.animator.SetTrigger("Hit");
                flashEffect.Flash(new Color(1, 1, 0, 1));
            }
        }
    }

    // For animation event
    public void ResetIsHurtAfterAnimation()
    {
        isHurt = false;
    }

    // For animation event
    public void SetHeadSpriteAlpha(float _alpha)
    {
        head.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, _alpha);
    }

    private bool isDeath = false;
    public bool IsDeath
    {
        get { return isDeath; }
        set {
            if (isDeath == false) {
                isDeath = true;
                GameManager.Instance.GameOver();
            }
        }
    }

    private void OnDisable()
    {
        inputVec = Vector2.zero;
        rigid.velocity = Vector2.zero;

        spriteRenderer.flipX = false;

        animator.SetInteger("XAxisRaw", 0);
        animator.SetInteger("YAxisRaw", 0);
    }
}
