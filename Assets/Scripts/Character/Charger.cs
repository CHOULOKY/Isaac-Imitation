using ChargerStates;
using Photon.Pun;
using UnityEngine;

public class Charger : Monster<Charger>, IPunObservable
{
      private enum States { Idle, Move, Attack, Dead }
      private States? curState;

      [HideInInspector] public RaycastHit2D playerHit;

      private bool isAttack = false;
      public bool IsAttack
      {
            get { return isAttack; }
            set {
                  if (isAttack != value) {
                        isAttack = value;
                        photonView.RPC(nameof(RPC_SetisAttack), RpcTarget.Others, value);
                  }
            }
      }
      [PunRPC]
      private void RPC_SetisAttack(bool value)
      {
            isAttack = value;
      }

      private float curAttackCooltime = 0;


      private void Start()
      {
            curState = States.Idle;
            fsm = new FSM<Charger>(new IdleState(this));
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
                        else if (OnSenseForward(0.95f, "Wall", "Obstacle") || isAttack) {
                              isAttack = false;
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


      public RaycastHit2D OnSenseForward(float _distance = 0.95f, params string[] _layers)
      {
            return Physics2D.Raycast(rigid.position, inputVec, _distance, LayerMask.GetMask(_layers));
      }

      private RaycastHit2D CanSeePlayer()
      {
            Vector2[] directions = { Vector2.right, Vector2.left, Vector2.down, Vector2.up };
            RaycastHit2D playerHit = default;

            foreach (var direction in directions) {
                  playerHit = Physics2D.Raycast(rigid.position, direction, 4, LayerMask.GetMask("Player"));
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
            // Gizmos.DrawRay(this.transform.position, inputVec * 3);
            // Gizmos.DrawRay(this.transform.position, Vector2.right * 4f);

            Gizmos.color = Color.yellow;
            // Gizmos.DrawRay(this.transform.position, inputVec * 0.85f);
            // Gizmos.DrawRay(this.transform.position, Vector2.right * 0.4f);
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
