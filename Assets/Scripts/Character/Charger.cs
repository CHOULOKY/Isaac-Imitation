using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChargerStates;
using System.Threading;

public class Charger : Monster<Charger>
{
    private enum States { Idle, Move, Attack, Dead }
    private States curState;

    private Rigidbody2D rigid;

    private bool isSpawned = false;

    [HideInInspector] public Vector2 inputVec;

    public RaycastHit2D playerHit;
    public bool isAttacked = false;
    private float curAttackCooltime = 0;


    private new void Awake()
    {
        base.Awake();
        rigid = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        curState = States.Idle;
        fsm = new FSM<Charger>(new IdleState(this));
    }

    private void OnEnable()
    {
        stat.health = stat.maxHealth;

        StartCoroutine(ParticleSystemCoroutine(spawnEffect.GetComponent<ParticleSystem>()));
    }

    private void Update()
    {
        if (curState == States.Dead) {
            return;
        }
        
        switch (curState) {
            case States.Idle:
                if (isSpawned) {
                    ChangeState(States.Move);
                }
                break;
            case States.Move:
                curAttackCooltime = curAttackCooltime >= stat.attackSpeed ?
                    stat.attackSpeed : curAttackCooltime + Time.deltaTime;
                
                if (OnDead()) {
                    ChangeState(States.Dead);
                }
                else if (playerHit = CanSeePlayer()) {
                    if (curAttackCooltime >= stat.attackSpeed) {
                        curAttackCooltime = 0;
                        ChangeState(States.Attack);
                    }
                }
                break;
            case States.Attack:
                if (OnDead()) {
                    ChangeState(States.Dead);
                }
                else if (OnSenseForward(0.85f, "Wall", "Obstacle") || isAttacked) {
                    ChangeState(States.Move);
                    isAttacked = false;
                }
                break;
            case States.Dead:
                // 
                break;
        }

        fsm.UpdateState();
    }

    private void ChangeState(States nextState)
    {
        curState = nextState;

        switch (curState) {
            case States.Idle:
                fsm.ChangeState(new IdleState(this));
                break;
            case States.Move:
                fsm.ChangeState(new MoveState(this));
                break;
            case States.Attack:
                fsm.ChangeState(new AttackState(this));
                break;
            case States.Dead:
                fsm.ChangeState(new DeadState(this));
                break;
        }
    }

    private IEnumerator ParticleSystemCoroutine(ParticleSystem effect)
    {
        yield return Instantiate(effect, 
            rigid.position + Vector2.down * 0.25f, effect.transform.rotation, this.transform);
        yield return new WaitUntil(() => !effect.isPlaying);
        yield return new WaitForSeconds(1f);

        isSpawned = true;
    }

    public RaycastHit2D OnSenseForward(float _distance = 0.85f, params string[] _layers)
    {
        return Physics2D.Raycast(rigid.position, inputVec, _distance, LayerMask.GetMask(_layers));
    }

    public RaycastHit2D CanSeePlayer()
    {
        Vector2[] directions = { Vector2.right, Vector2.left, Vector2.down, Vector2.up };
        RaycastHit2D playerHit = default;

        foreach (var direction in directions) {
            playerHit = Physics2D.Raycast(rigid.position, direction, 4, LayerMask.GetMask("Player"));
            if (playerHit) break; // 플레이어를 발견하면 루프를 종료
        }

        return playerHit;
    }

    private bool OnDead()
    {
        return stat.health <= 0;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        // Gizmos.DrawRay(this.transform.position, inputVec * 3);
        // Gizmos.DrawRay(this.transform.position, Vector2.right * 4f);

        Gizmos.color = Color.yellow;
        // Gizmos.DrawRay(this.transform.position, inputVec * 0.85f);
        // Gizmos.DrawRay(this.transform.position, Vector2.right * 0.4f);
    }
}
