using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster<T> : MonoBehaviour where T : class
{
    protected FSM<T> fsm;

    public MonsterStat stat;

    public Transform effectParent;
    public GameObject spawnEffect;
    public GameObject[] deathEffect;
    private FlashEffect flashEffect;

    protected void Awake()
    {
        if (effectParent == null) {
            effectParent = GameObject.Find("Effect Parent").transform;
        }
        if (spawnEffect == null) {
            spawnEffect = Resources.Load<GameObject>("FX_MonsterSpawn Variant");
        }
        if (deathEffect == null || deathEffect.Length == 0) {
            deathEffect = new GameObject[2];
            deathEffect[0] = Resources.Load<GameObject>("Blood1 Variant");
            deathEffect[1] = Resources.Load<GameObject>("Blood2 Variant");
        }
        flashEffect = GetComponent<FlashEffect>();
    }

    private bool isHurt = false;
    public bool IsHurt
    {
        get { return isHurt; }
        set {
            if (isHurt == false) {
                isHurt = true;
                if (stat.health <= 0) {
                    IsDeath = true;
                    return;
                }

                // this.animator.SetTrigger("Hit");
                flashEffect.Flash(new Color(1, 0, 0, 1));
                isHurt = false;
            }
        }
    }

    private bool isDeath = false;
    public bool IsDeath
    {
        get { return isDeath; }
        set {
            if (isDeath == false) {
                isDeath = true;
                this.gameObject.layer = LayerMask.NameToLayer("Destroyed");
                Instantiate(deathEffect[UnityEngine.Random.Range(0, 2)], 
                    this.transform.position, Quaternion.identity, effectParent);
                GetComponent<Animator>().SetBool("isDeath", true);
            }
        }
    }

    public void DisableMonster()
    {
        this.gameObject.SetActive(false);
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
