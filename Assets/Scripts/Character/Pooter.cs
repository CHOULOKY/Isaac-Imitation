using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PooterStates;
using Photon.Pun.Demo.PunBasics;

public class Pooter : Monster<Pooter>
{
    private enum States { Idle, Move, Attack, Dead }
    private States? curState;

    [HideInInspector] public RaycastHit2D playerHit;
    private float curAttackCooltime = 0;
    private bool isAttackFinished = false;
    public void SetIsAttackFinished(int value) => isAttackFinished = value == 0 ? false : true; // for animation event


    private void Start()
    {
        curState = States.Idle;
        fsm = new FSM<Pooter>(new IdleState(this));
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        if (curState != null) ChangeState(States.Idle);
    }

    private void Update()
    {
        if (curState == States.Dead) {
            return;
        }

        switch (curState) {
            case States.Idle:
                if (OnDead()) {
                    ChangeState(States.Dead);
                }
                else if (isSpawned) {
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
                else if (isAttackFinished) {
                    isAttackFinished = false;
                    ChangeState(States.Move);
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

    public RaycastHit2D CanSeePlayer()
    {
        Vector2[] directions = { Vector2.right, Vector2.left, Vector2.down, Vector2.up };
        RaycastHit2D playerHit = default;

        foreach (var direction in directions) {
            playerHit = Physics2D.Raycast(rigid.position, direction, 5, LayerMask.GetMask("Player"));
            if (playerHit) break; // 플레이어를 발견하면 루프를 종료
        }

        return playerHit;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        // Gizmos.DrawRay(this.transform.position, Vector2.right * 5f);
    }
}
