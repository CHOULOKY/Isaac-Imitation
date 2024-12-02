using GaperStates;
using Photon.Pun;
using UnityEngine;

public class Gaper : Monster<Gaper>
{
      private enum States { Idle, Move, Dead }
      private States? curState;

      public Vector2 playerSearchBox;
      public float collisionCircle = 0.35f;

      [Tooltip("For following a player (Using \"Queue\")")]
      public int followDelay = 12;


      private void Start()
      {
            curState = States.Idle;
            fsm = new FSM<Gaper>(new IdleState(this));
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
                        if (OnDead()) {
                              ChangeState(States.Dead);
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
                  case States.Dead:
                        fsm.ChangeState(new DeadState(this));
                        break;
            }
      }

      protected override void OnDisable()
      {
            base.OnDisable();
            curState = null;
      }

      private void OnDrawGizmos()
      {
            Gizmos.color = Color.red;
            // Gizmos.DrawWireCube(transform.position, playerSearchBox);

            Gizmos.color = Color.yellow;
            // Gizmos.DrawWireSphere(transform.position, collisionCircle);
      }
}
