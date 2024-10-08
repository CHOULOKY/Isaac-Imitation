using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHead : MonoBehaviour
{
    private PlayerBody body;

    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private Vector2 inputVec;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        body = transform.parent.GetComponent<PlayerBody>();
    }

    private void Update()
    {
        inputVec.x = Input.GetAxisRaw("Horizontal Arrow");
        inputVec.y = Input.GetAxisRaw("Vertical Arrow");
    }
}
