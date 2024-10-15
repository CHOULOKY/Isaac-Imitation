using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class IssacHead : MonoBehaviour
{
    private IssacBody body;

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

        body = transform.parent.GetComponent<IssacBody>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            inputVec.x = -1;
            inputVec.y = 0;
        } else if (Input.GetKeyDown(KeyCode.RightArrow)) {
            inputVec.x = 1;
            inputVec.y = 0;
        } else if (Input.GetKeyDown(KeyCode.UpArrow)) {
            inputVec.x = 0;
            inputVec.y = 1;
        } else if (Input.GetKeyDown(KeyCode.DownArrow)) {
            inputVec.x = 0;
            inputVec.y = -1;
        }

        if (inputVec.x > 0) {
            spriteRenderer.flipX = false;
        } else if (inputVec.x < 0) {
            spriteRenderer.flipX = true;
        }
        animator.SetInteger("XAxisRaw", (int)inputVec.x);
        animator.SetInteger("YAxisRaw", (int)inputVec.y);

        curAttackTime += Time.deltaTime;
        if (curAttackTime > attackSpeed) {
            if (Input.GetButton("Horizontal Arrow") || Input.GetButton("Vertical Arrow")) {
                curAttackTime = 0;

                GameObject curTear = GameManager.Instance.isaacTearFactory.GetTear(tearType, false);
                Rigidbody2D tearRigid = curTear.GetComponent<Rigidbody2D>();

                float x = default, y = default;
                tearWhatEye = tearWhatEye * -1;
                if (inputVec.x == 1) {
                    x = 0.3f;
                    y = 0.2f * tearWhatEye;
                }
                else if (inputVec.x == -1) {
                    x = -0.3f;
                    y = 0.2f * tearWhatEye;
                }
                else if (inputVec.y == 1) {
                    x = 0.2f * tearWhatEye;
                    y = 0.3f;
                }
                else if (inputVec.y == -1) {
                    x = 0.2f * tearWhatEye;
                    y = -0.3f;
                }

                curTear.SetActive(true);
                tearRigid.position = (Vector2)this.transform.position + new Vector2(x, y);
                tearRigid.velocity = Vector2.zero;
                tearRigid.AddForce(inputVec * tearSpeed, ForceMode2D.Impulse);
            }
        }
    }
}
