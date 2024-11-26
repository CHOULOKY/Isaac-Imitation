using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

public class Tear : MonoBehaviour
{
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
            rigid = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
      }

      protected virtual void OnEnable()
      {
            rigid.simulated = true;

            SetGravitySetTimeByDirection(out float curGravitySetTime);
            StartCoroutine(SetGravityAfter(curGravitySetTime));
            StartCoroutine(AfterActiveTime(tearActiveTime));
      }

      protected virtual void OnDisable()
      {
            transform.position = transform.parent.position;
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
      }

      // For animation event
      public void SetActiveFalse()
      {
            gameObject.SetActive(false);
      }
}
