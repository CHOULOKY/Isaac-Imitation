using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster<T> : MonoBehaviour where T : class
{
    protected FSM<T> fsm;

    public MonsterStat stat;

    public GameObject spawnEffect;

    protected void Awake()
    {
        spawnEffect = spawnEffect != null ? spawnEffect : Resources.Load<GameObject>("FX_MonsterSpawn Variant");
    }
}

[System.Serializable]
public class MonsterStat
{
    public float health;
    public float maxHealth = 5;

    public float moveForce = 10;
    public float maxVelocity = 3; // moveForce/5 + 1

    public int attackDamage;
    public float attackSpeed;
}
