using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsaacTear : MonoBehaviour
{
    private Rigidbody2D rigid;

    public int tearDamage = 1;

    public float gravitySetTime;
    public float gravityScale = 0.3f;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        StartCoroutine(SetGravityAfter(gravitySetTime));
    }

    private IEnumerator SetGravityAfter(float _setDuration = 0.15f)
    {
        yield return new WaitForSeconds(_setDuration);

        rigid.gravityScale = gravityScale;
    }
}
