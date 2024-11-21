using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MyNamespace
{
    public class PlayerHead : MonoBehaviour
    {
        public Vector2 inputVec;
        public float speed = 5f;
        public InputActionAsset inputActions;

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
            if (inputActions != null)
            {
                inputVec = inputActions.FindAction("Move").ReadValue<Vector2>();
            }
            else
            {
                inputVec.x = Input.GetAxis("Horizontal");
                inputVec.y = Input.GetAxis("Vertical");
            }
        }

        void FixedUpdate()
        {
            Vector2 nextVec = inputVec.normalized * speed * Time.fixedDeltaTime;
            rigid.MovePosition(rigid.position + nextVec);
        }

        void LateUpdate()
        {
            anim.SetFloat("speed", inputVec.magnitude);

            if (inputVec.y > 0) // 위로 이동
            {
                anim.SetBool("isBack", true);
                anim.SetBool("isLeft", false);
                anim.SetBool("isRight", false);
            }
            else if (inputVec.x < 0) // 왼쪽으로 이동
            {
                anim.SetBool("isBack", false);
                anim.SetBool("isLeft", true);
                anim.SetBool("isRight", false);
            }
            else if (inputVec.x > 0) // 오른쪽으로 이동
            {
                anim.SetBool("isBack", false);
                anim.SetBool("isLeft", false);
                anim.SetBool("isRight", true);
            }
            else // 정지 상태
            {
                anim.SetBool("isBack", false);
                anim.SetBool("isLeft", false);
                anim.SetBool("isRight", false);
            }
        }
    }
}
