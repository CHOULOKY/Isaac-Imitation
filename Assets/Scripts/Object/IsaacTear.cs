using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class IsaacTear : MonoBehaviour
{
    private Rigidbody2D rigid;
    private Animator animator;

    public int tearDamage = 1;

    public float gravitySetTime = 0.15f;
    public float gravityScale = 0.3f;

    public float tearActiveTime = 2;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        rigid.simulated = true;
        
        StartCoroutine(SetGravityAfter(gravitySetTime));
        StartCoroutine(AfterActiveTime(tearActiveTime));
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Wall")) {
            DisableTear();
        }
        else if (collision.CompareTag("Monster")) {
            DisableTear();

            MonoBehaviour[] collisionScript = collision.gameObject.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour script in collisionScript) {
                Type baseType = script.GetType()?.BaseType;
                if (baseType != null && baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(Monster<>)) {
                    // stat 변수에 접근 (protected 또는 private라면 BindingFlags 사용)
                    FieldInfo statField = baseType.GetField("stat", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                    if (statField != null) {
                        MonsterStat monsterStat = statField.GetValue(script) as MonsterStat;
                        monsterStat.health -= tearDamage;
                    }
                    else {
                        Debug.LogWarning("stat 필드를 찾을 수 없습니다.");
                    }
                    break;
                }
            }
        }
    }

    private void OnDisable()
    {
        transform.position = transform.parent.position;
    }

    private IEnumerator SetGravityAfter(float _setDuration = 0.15f)
    {
        yield return new WaitForSeconds(_setDuration);

        rigid.gravityScale = gravityScale;
    }

    private IEnumerator AfterActiveTime(float _tearActiveTime)
    {
        yield return new WaitForSeconds(_tearActiveTime);

        DisableTear();
    }

    private void DisableTear()
    {
        rigid.velocity = Vector3.zero;
        rigid.simulated = false;

        animator.SetTrigger("Pop");
    }

    // For animation event
    public void SetActiveFalse()
    {
        gameObject.SetActive(false);
    }
}
