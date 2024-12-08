using PooterStates;
using UnityEngine;
using Photon.Pun;
using System.Runtime.CompilerServices;
using System;

public class Pooter : Monster<Pooter>
{
      private enum States { Idle, Move, Attack, Dead }
      private States? curState;

      [HideInInspector] public RaycastHit2D playerHit;
      private float curAttackCooltime = 0;

      private bool isAttackFinished = false;
      public bool IsAttackFinished
      {
            get => isAttackFinished;
            set {
                  if (isAttackFinished != value) {
                        isAttackFinished = value;
                        photonView.RPC(nameof(RPC_SetisAttackFinished), RpcTarget.Others, value);
                  }
            }
      }
      [PunRPC]
      private void RPC_SetisAttackFinished(bool value)
      {
            isAttackFinished = value;
      }

      private bool[] isAttackTiming = { false, false };
      public bool[] IsAttackTiming
      {
            get => isAttackTiming; // 배열 전체 반환
            set {
                  if (value != null && value.Length == isAttackTiming.Length) {
                        isAttackTiming = value; // 배열 전체를 교체
                  }
            }
      }

      // For animation events
      public void SetIsAttackFinished(int value) => IsAttackFinished = value == 0 ? false : true;
      public void TriggerAttackTiming(int value) {
            if (value == 0) {
                  IsAttackTiming = new bool[] { true, isAttackTiming[1] };
            }
            else {
                  IsAttackTiming = new bool[] { isAttackTiming[0], true };
            }
      }

      
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
            // 소유권이 바뀌어도 fsm update만 실행하면 될 수 있도록 -> OnStateEnter, OnStateExit만 실행
            if (curState == States.Dead || !photonView.IsMine) {
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
                        else if (IsAttackFinished) {
                              IsAttackFinished = false;
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
            //curState = nextState;

            //switch (curState) {
            //      case States.Idle:
            //            fsm.ChangeState(new IdleState(this));
            //            break;
            //      case States.Move:
            //            fsm.ChangeState(new MoveState(this));
            //            break;
            //      case States.Attack:
            //            fsm.ChangeState(new AttackState(this));
            //            break;
            //      case States.Dead:
            //            fsm.ChangeState(new DeadState(this));
            //            break;
            //}

            photonView.RPC(nameof(RPC_ChangeState), RpcTarget.AllBuffered, nextState);
      }
      [PunRPC]
      private void RPC_ChangeState(States nextState)
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

      private RaycastHit2D CanSeePlayer()
      {
            Vector2[] directions = { Vector2.right, Vector2.left, Vector2.down, Vector2.up };
            RaycastHit2D playerHit = default;

            foreach (var direction in directions) {
                  playerHit = Physics2D.Raycast(rigid.position, direction, 5, LayerMask.GetMask("Player"));
                  if (playerHit) break; // 플레이어를 발견하면 루프를 종료
            }

            return playerHit;
      }

      protected override void OnDisable()
      {
            base.OnDisable();
            curState = null;
      }

      private void OnDrawGizmos()
      {
            Gizmos.color = Color.red;
            // Gizmos.DrawRay(this.transform.position, Vector2.right * 5f);
      }



      public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
      {
            base.OnPhotonSerializeView(stream, info);

            // 언제 소유권이 바뀌어도 문제 없도록
            if (stream.IsWriting) {
                  stream.SendNext(curAttackCooltime); // 현재 공격 쿨타임
            }
            else {
                  curAttackCooltime = (float)stream.ReceiveNext();
            }
      }
}
