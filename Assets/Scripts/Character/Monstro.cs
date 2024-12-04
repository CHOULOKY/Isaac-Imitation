using UnityEngine;
using MonstroStates;
using Photon.Realtime;
using System.Threading;
using System.Collections;
using Photon.Pun;

public class Monstro : Monster<Monstro>
{
      private enum States { Idle, SmallJump, BigJump, TearSpray, Dead }
      private States? curState;

      [HideInInspector] public IsaacBody player;
      public Vector2 playerSearchBox;

      #region Attack Pattern State Property
      private bool isSmallJump = false;
      public bool IsSmallJump
      {
            get => isSmallJump;
            set {
                  if (value != isSmallJump) {
                        isSmallJump = value;
                        photonView.RPC(nameof(RPC_SetisSmallJump), RpcTarget.OthersBuffered, value);
                  }
            }
      }
      [PunRPC]
      private void RPC_SetisSmallJump(bool value)
      {
            isSmallJump = value;
      }

      private bool isBigJump = false;
      public bool IsBigJump
      {
            get => isBigJump;
            set {
                  if (value != isBigJump) {
                        isBigJump = value;
                        photonView.RPC(nameof(RPC_SetisBigJump), RpcTarget.OthersBuffered, value);
                  }
            }
      }
      [PunRPC]
      private void RPC_SetisBigJump(bool value)
      {
            isBigJump = value;
      }

      private bool isTearSpray = false;
      public bool IsTearSpray
      {
            get => isTearSpray;
            set {
                  if (value != isTearSpray) {
                        isTearSpray = value;
                        photonView.RPC(nameof(RPC_SetisTearSpray), RpcTarget.OthersBuffered, value);
                  }
            }
      }
      [PunRPC]
      private void RPC_SetisTearSpray(bool value)
      {
            isTearSpray = value;
      }
      #endregion

      #region Control Animation Events and Properties
      // For animation events
      private bool isJumpUp = false; // for BigJump state
      public bool IsJumpUp
      {
            get => isJumpUp;
            set {
                  if (value != isJumpUp) {
                        isJumpUp = value;
                        photonView.RPC(nameof(RPC_SetisJumpUp), RpcTarget.OthersBuffered, value);
                  }
            }
      }
      [PunRPC]
      private void RPC_SetisJumpUp(bool value)
      {
            isJumpUp = value;
      }

      private bool isOnLand = false; // for all Jump state
      public bool IsOnLand
      {
            get => isOnLand;
            set {
                  if (value != isOnLand) {
                        isOnLand = value;
                        photonView.RPC(nameof(RPC_SetisOnLand), RpcTarget.OthersBuffered, value);
                  }
            }
      }
      [PunRPC]
      private void RPC_SetisOnLand(bool value)
      {
            isOnLand = value;
      }

      private bool isTearTiming; // for tear attack
      public bool IsTearTiming
      {
            get => isTearTiming;
            set {
                  if (value != isTearTiming) {
                        isTearTiming = value;
                        photonView.RPC(nameof(RPC_SetisTearTiming), RpcTarget.OthersBuffered, value);
                  }
            }
      }
      [PunRPC]
      private void RPC_SetisTearTiming(bool value)
      {
            isTearTiming = value;
      }

      private bool isDeadFinish = false; // for Dead state
      public bool IsDeadFinish
      {
            get => isDeadFinish;
            set {
                  if (value != isDeadFinish) {
                        isDeadFinish = value;
                        photonView.RPC(nameof(RPC_SetisDeadFinish), RpcTarget.OthersBuffered, value);
                  }
            }
      }
      [PunRPC]
      private void RPC_SetisDeadFinish(bool value)
      {
            isDeadFinish = value;
      }


      public void TriggerJumpUp(int value) => isJumpUp = value != 0;
      public void TriggerOnLand(int value) => isOnLand = value != 0;

      public void TriggerTearTiming(int value) => isTearTiming = value != 0;

      public void TriggerDeadFinish(int value) => isDeadFinish = value != 0;
      #endregion


      private void Start()
      {
            curState = States.Idle;
            fsm = new FSM<Monstro>(new IdleState(this));
      }

      protected override void OnEnable()
      {
            base.OnEnable();

            if (curState != null) ChangeState(States.Idle);
      }

      private void Update()
      {
            // 소유권이 바뀌어도 fsm update만 실행하면 될 수 있도록 -> OnStateEnter, OnStateExit만 실행
            if (!photonView.IsMine) {
                  return;
            }

            switch (curState) {
                  case States.Idle:
                        if (OnDead()) {
                              ChangeState(States.Dead);
                        }
                        else if (isSpawned) {
                              isSmallJump = true;
                              ChangeState(States.SmallJump);
                        }
                        break;
                  case States.SmallJump:
                        if (OnDead()) {
                              ChangeState(States.Dead);
                        }
                        else if (!isSmallJump) {
                              ChangeState(GetNextState((States)curState));
                        }
                        break;
                  case States.BigJump:
                        if (OnDead()) {
                              ChangeState(States.Dead);
                        }
                        else if (!isBigJump) {
                              ChangeState(GetNextState((States)curState));
                        }
                        break;
                  case States.TearSpray:
                        if (OnDead()) {
                              ChangeState(States.Dead);
                        }
                        else if (!isTearSpray) {
                              ChangeState(GetNextState((States)curState));
                        }
                        break;
                  case States.Dead:
                        // 
                        break;
            }

            // Debug.Log(curState.ToString());
            fsm.UpdateState();
      }

      private void ChangeState(States nextState)
      {
            //curState = nextState;

            //switch (curState) {
            //      case States.Idle:
            //            fsm.ChangeState(new IdleState(this));
            //            break;
            //      case States.SmallJump:
            //            fsm.ChangeState(new SmallJumpState(this));
            //            break;
            //      case States.BigJump:
            //            fsm.ChangeState(new BigJumpState(this));
            //            break;
            //      case States.TearSpray:
            //            fsm.ChangeState(new TearSprayState(this));
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
                  case States.SmallJump:
                        fsm.ChangeState(new SmallJumpState(this));
                        break;
                  case States.BigJump:
                        fsm.ChangeState(new BigJumpState(this));
                        break;
                  case States.TearSpray:
                        fsm.ChangeState(new TearSprayState(this));
                        break;
                  case States.Dead:
                        fsm.ChangeState(new DeadState(this));
                        break;
            }
      }

      private States GetNextState(States curState)
      {
            States nextState = curState;
            switch (curState) {
                  case States.SmallJump:
                        // isSmallJump = false;
                        isTearSpray = true;
                        nextState = States.TearSpray;
                        break;
                  case States.BigJump:
                        // isBigJump = false;
                        isSmallJump = true;
                        nextState = States.SmallJump;
                        break;
                  case States.TearSpray:
                        // isTearSpray = false;
                        nextState = UnityEngine.Random.Range(0, 5) == 0 ? States.SmallJump : States.BigJump;
                        if (nextState == States.SmallJump) isSmallJump = true;
                        else isBigJump = true;
                        break;
            }

            return nextState;
      }

      protected override IEnumerator ParticleSystemCoroutine(ParticleSystem _effect)
      {
            ParticleSystem effect = Instantiate(_effect,
                rigid.position + Vector2.down * 0.35f, Quaternion.identity, this.transform);
            effect.transform.localScale = _effect.transform.localScale * 1.5f;
            yield return new WaitUntil(() => !effect.isPlaying);

            isSpawned = true;
      }

      protected override void OnDisable()
      {
            base.OnDisable();
            curState = null;
      }

      public override void SpawnBloodEffects()
      {
            // spawn blood puddle & blood splash
            Vector2 bloodOffest = new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(0f, -1f));
            GameObject deathBlood = Instantiate(deathBloods[UnityEngine.Random.Range(0, 3)],
                  (Vector2)this.transform.position + bloodOffest, Quaternion.identity, bloodParent);
            deathBlood.transform.localScale = Vector3.one * 0.75f;
      }

      #region Not used
      protected override void SetAfterDeath()
      {
            // Do nothing within this function
      }
      #endregion

      //private Transform shadow;
      private void OnDrawGizmos()
      {
            Gizmos.color = Color.green;
            //shadow = transform.GetChild(0);
            //Bounds bounds = shadow.GetComponent<Collider2D>().bounds;
            //Gizmos.DrawWireCube(bounds.center, bounds.size);
      }
}
