using Photon.Pun;
using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

public class Tear : MonoBehaviour
{
      protected PhotonView photonView;

      protected Rigidbody2D rigid;
      protected Animator animator;

      public int tearDamage = 1;
      public float knockPower;

      [HideInInspector] public int tearDirection; // Up: 0, Down: 1, Right: 2, Left: 3
      public float gravitySetTime = 0.75f;
      public float gravityScale = 0.75f;
      
      public float tearActiveTime = 1;

      protected virtual void Awake()
      {
            photonView = GetComponent<PhotonView>();

            rigid = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
      }

      protected virtual void OnEnable()
      {
            // 소유권이 있으면 실행
            if (photonView.IsMine) {
                  rigid.simulated = true;

                  photonView.RequestOwnership();
                  SetGravitySetTimeByDirection(out float curGravitySetTime);
                  StartCoroutine(SetGravityAfter(curGravitySetTime));
                  StartCoroutine(AfterActiveTime(tearActiveTime));
            }
      }

      protected virtual void OnDisable()
      {
            // 소유권이 있으면 실행
            if (photonView.IsMine) {
                  transform.position = transform.parent.position;
            }
      }

      [Header("Gravity Set Time (Up: 0, Down: 1, Right: 2, Left: 3)")]
      [SerializeField] protected float Up = 0.5f;
      [SerializeField] protected float Down = 1f, Right = 0.75f, Left = 0.75f;
      protected virtual void SetGravitySetTimeByDirection(out float curGravitySetTime)
      {
            curGravitySetTime = tearDirection switch
            {
                  0 => Up,
                  1 => gravitySetTime * Down,
                  2 => gravitySetTime * Right,
                  _ => gravitySetTime * Left,
            };
      }

      protected IEnumerator SetGravityAfter(float _setDuration = 0.15f)
      {
            yield return new WaitForSeconds(_setDuration);

            rigid.gravityScale = gravityScale;
      }

      protected IEnumerator AfterActiveTime(float _tearActiveTime)
      {
            yield return new WaitForSeconds(_tearActiveTime);

            DisableTear();
      }

      protected virtual void DisableTear()
      {
            StopCoroutine(nameof(SetGravityAfter));
            StopCoroutine(nameof(AfterActiveTime));

            rigid.velocity = Vector3.zero;
            rigid.simulated = false;
            rigid.gravityScale = 0;

            animator.SetTrigger("Pop");

            photonView.RPC(nameof(RPC_DisableTear), RpcTarget.OthersBuffered, "Pop");
      }
      [PunRPC]
      protected virtual void RPC_DisableTear(string name)
      {
            rigid.velocity = Vector3.zero;
            rigid.gravityScale = 0;

            animator.SetTrigger(name);
      }

      // For animation event
      public void SetActiveFalse()
      {
            gameObject.SetActive(false);
      }
}
