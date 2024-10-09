using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHead : MonoBehaviour
{
    private PlayerBody body;

    private Animator animator;
    private SpriteRenderer spriteRenderer;

    public Vector2 inputVec;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        body = transform.parent.GetComponent<PlayerBody>();
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
    }
}
