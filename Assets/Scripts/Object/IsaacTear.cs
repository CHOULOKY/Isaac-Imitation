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
    public float knockPower;

    [HideInInspector] public int tearDirection; // Up: 0, Down: 1, Right: 2, Left: 3
    [Tooltip("Up: 15%, Down: 40%, Left&Right: 65%")]
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

        SetGravitySetTimeByDirection(out float curGravitySetTime);
        StartCoroutine(SetGravityAfter(curGravitySetTime));
        StartCoroutine(AfterActiveTime(tearActiveTime));
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Wall")) {
            DisableTear();
        }
        else if (collision.CompareTag("Monster")) {
            DisableTear();

            if (TryGetMonsterFields(collision, out MonoBehaviour script, out FieldInfo statField, 
                out PropertyInfo isHurtProperty, out FieldInfo monsterTypeField)) {
                if (statField.GetValue(script) is MonsterStat monsterStat &&
                    monsterTypeField.GetValue(script) is MonsterType monsterType) {
                    monsterStat.health -= tearDamage;
                    isHurtProperty.SetValue(script, true);
                    ApplyKnockToMonster(monsterType, script.GetComponent<Rigidbody2D>());
                }
            }
            else {
                Debug.LogWarning("stat 필드 또는 IsHurt 프로퍼티를 찾을 수 없습니다.");
            }
        }
    }

    private bool TryGetMonsterFields(Collider2D collision, out MonoBehaviour script, out FieldInfo statField, 
        out PropertyInfo isHurtProperty, out FieldInfo monsterTypeField)
    {
        // 초기화
        script = null;
        statField = null;
        isHurtProperty = null;
        monsterTypeField = null;

        MonoBehaviour[] scripts = collision.GetComponents<MonoBehaviour>();
        if (scripts.Length <= 1) scripts = collision.GetComponentsInParent<MonoBehaviour>();
        foreach (MonoBehaviour s in scripts) {
            Type baseType = s.GetType()?.BaseType; // 부모: Monster
            if (baseType != null && baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(Monster<>)) {
                // stat 필드와 IsHurt 프로퍼티 찾기
                statField = baseType.GetField("stat", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                isHurtProperty = baseType.GetProperty("IsHurt", BindingFlags.Instance | BindingFlags.Public);
                // MonsterType 열거형 필드 찾기
                monsterTypeField = baseType.GetField("monsterType", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                
                if (statField != null && isHurtProperty != null && monsterTypeField != null) {
                    script = s;
                    return true;
                }
                break;
            }
        }
        return false;
    }

    private void ApplyKnockToMonster(MonsterType monsterType, Rigidbody2D monsterRigid)
    {
        if (monsterType == MonsterType.Charger) return;
        
        monsterRigid.velocity = Vector2.zero;
        monsterRigid.AddForce((monsterRigid.position - rigid.position).normalized * knockPower, ForceMode2D.Impulse);
    }
    
    private void OnDisable()
    {
        transform.position = transform.parent.position;
    }

    private void SetGravitySetTimeByDirection(out float curGravitySetTime)
    {
        // Up: 15%, Down: 40%, Left&Right: 65%
        // Up: 0, Down: 1, Right: 2, Left: 3
        curGravitySetTime = tearDirection switch
        {
            0 => 0.15f,
            1 => gravitySetTime * 0.4f,
            _ => gravitySetTime * 0.65f,
        };
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
        rigid.gravityScale = 0;

        animator.SetTrigger("Pop");
    }

    // For animation event
    public void SetActiveFalse()
    {
        gameObject.SetActive(false);
    }
}
