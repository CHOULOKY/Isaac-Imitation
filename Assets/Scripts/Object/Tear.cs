using System.Collections;
using System.Reflection;
using UnityEngine;

public class Tear : MonoBehaviour
{
    protected Rigidbody2D rigid;

    public int tearDamage = 1;
    public float knockPower;

    [HideInInspector] public int tearDirection; // Up: 0, Down: 1, Right: 2, Left: 3
    public float gravitySetTime = 0.15f;
    public float gravityScale = 0.3f;

    public float tearActiveTime = 2;

    [Header("Gravity Set Time (Up: 0, Down: 1, Right: 2, Left: 3)")]
    [SerializeField] protected float Up;
    [SerializeField] protected float Down, Right, Left;

    protected virtual void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();


        // Debug: Check if Rigidbody2D and Animator are assigned

        // Debug: Check if SpriteRenderer is assigned and has a sprite
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null || spriteRenderer.sprite == null)
        {
            Debug.LogWarning("Tear has no SpriteRenderer or Sprite assigned!");
        }
    }

    protected virtual void OnEnable()
    {
        rigid.simulated = true;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.enabled = true;
            sr.color = Color.white; // 혹시 모를 투명도 문제 해결
        }

        SetGravitySetTimeByDirection(out float curGravitySetTime);
        StartCoroutine(SetGravityAfter(curGravitySetTime));
        StartCoroutine(AfterActiveTime(tearActiveTime));

        Debug.Log($"Tear Enabled at Position: {transform.position}, Direction: {tearDirection}");
    }

    protected virtual void OnDisable()
    {
        transform.position = transform.parent.position;
        Debug.Log("Tear Disabled");
    }

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
        Debug.Log($"Gravity set to {gravityScale} after {_setDuration} seconds");
    }

    protected IEnumerator AfterActiveTime(float _tearActiveTime)
    {
        yield return new WaitForSeconds(_tearActiveTime);
        DisableTear();
    }

    protected virtual void DisableTear()
    {
        rigid.velocity = Vector3.zero;
        rigid.simulated = false;
        rigid.gravityScale = 0;

        Debug.Log("Tear Disabled and Animator Pop Triggered");
    }
}

