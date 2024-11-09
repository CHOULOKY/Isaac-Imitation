using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class IsaacHead : MonoBehaviour
{
    private IsaacBody body;

    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private Vector2 inputVec;

    private TearFactory.Tears tearType = TearFactory.Tears.Basic;
    [Tooltip("= tearRange")] public float tearSpeed = 6;
    private int tearWhatEye = 1;

    public float attackSpeed = 0.25f;
    private float curAttackTime = 0.25f;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        body = transform.parent.GetComponent<IsaacBody>();
    }

    private void Update()
    {
        if (body.IsHurt) return;

        GetInputVec();

        SetHeadDirection();

        AttackUsingTear();
    }

    private void GetInputVec()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            inputVec.x = -1;
            inputVec.y = 0;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow)) {
            inputVec.x = 1;
            inputVec.y = 0;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow)) {
            inputVec.x = 0;
            inputVec.y = 1;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow)) {
            inputVec.x = 0;
            inputVec.y = -1;
        }
    }

    private void SetHeadDirection()
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

    private void AttackUsingTear()
    {
        curAttackTime += Time.deltaTime;
        if (curAttackTime > attackSpeed) {
            if (Input.GetButton("Horizontal Arrow") || Input.GetButton("Vertical Arrow")) {
                curAttackTime = 0;

                GameObject curTear = GameManager.Instance.isaacTearFactory.GetTear(tearType, false);
                SetTearPositionAndGravityTime(curTear, out float x, out float y);
                SetTearVelocity(out Vector2 tearVelocity);
                ShootSettedTear(curTear, tearVelocity, x, y);
            }
        }
    }

    private void SetTearPositionAndGravityTime(GameObject curTear, out float x, out float y)
    {
        if (curTear.GetComponent<IsaacTear>() is IsaacTear tear) {
            tearWhatEye *= -1;
            // Up: 0, Down: 1, Right: 2, Left: 3
            if (inputVec.x == 1) {
                x = 0.3f;
                y = 0.2f * tearWhatEye;
                tear.tearDirection = 2;
            }
            else if (inputVec.x == -1) {
                x = -0.3f;
                y = 0.2f * tearWhatEye;
                tear.tearDirection = 3;
            }
            else if (inputVec.y == 1) {
                x = 0.2f * tearWhatEye;
                y = 0.3f;
                tear.tearDirection = 0;
            }
            else {
                // inputVec.y == -1
                x = 0.2f * tearWhatEye;
                y = -0.3f;
                tear.tearDirection = 1;
            }
        }
        else {
            x = y = default;
        }
    }

    private void SetTearVelocity(out Vector2 tearVelocity)
    {
        tearVelocity = Vector2.zero;

        if (body.GetComponent<Rigidbody2D>() is Rigidbody2D bodyRigid) {
            float velocityByBody;
            if (bodyRigid.velocity.x > 1 || bodyRigid.velocity.x < -1) {
                velocityByBody = bodyRigid.velocity.x / 1 * 0.35f;
                tearVelocity = new Vector2(tearVelocity.x + velocityByBody, tearVelocity.y);
            }
            if (bodyRigid.velocity.y > 1 || bodyRigid.velocity.y < -1) {
                velocityByBody = bodyRigid.velocity.y / 1 * 0.35f;
                tearVelocity = new Vector2(tearVelocity.x, tearVelocity.y + velocityByBody);
            }
        }
    }

    private void ShootSettedTear(GameObject curTear, Vector2 tearVelocity, float x, float y)
    {
        if (curTear.GetComponent<Rigidbody2D>() is Rigidbody2D tearRigid) {
            curTear.SetActive(true);
            tearRigid.position = (Vector2)this.transform.position + new Vector2(x, y);
            tearRigid.velocity = Vector2.zero;
            // tearRigid.AddForce(inputVec * tearSpeed + Vector2.up / 2 + tearVelocity, ForceMode2D.Impulse);
            tearRigid.AddForce(inputVec * tearSpeed + tearVelocity, ForceMode2D.Impulse);
        }
    }

    private void OnDisable()
    {
        inputVec = Vector2.zero;

        spriteRenderer.flipX = false;

        animator.SetInteger("XAxisRaw", 0);
        animator.SetInteger("YAxisRaw", 0);
    }
}