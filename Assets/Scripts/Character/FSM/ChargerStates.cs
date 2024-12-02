using Photon.Pun;
using System.Threading.Tasks;
using UnityEngine;

namespace ChargerStates
{
      public abstract class ChargerState : BaseState<Charger>
      {
            protected PhotonView photonView;
            protected FSMRPCController fSMRPCController;

            protected Rigidbody2D rigid;
            protected Animator animator;
            protected SpriteRenderer spriteRenderer;

            protected Transform[] shadow = new Transform[2]; // 0: XShadow, 1: YShadow

            public ChargerState(Charger _monster) : base(_monster) { }

            public override void OnStateEnter()
            {
                  photonView = monster.GetComponent<PhotonView>();
                  fSMRPCController = monster.GetComponent<FSMRPCController>();

                  rigid = monster.GetComponent<Rigidbody2D>();
                  animator = monster.GetComponent<Animator>();
                  spriteRenderer = monster.GetComponent<SpriteRenderer>();

                  shadow[0] = monster.transform.GetChild(0);
                  shadow[1] = monster.transform.GetChild(1);
            }

            public override void OnStateUpdate()
            {
                  if (!rigid || !animator || !spriteRenderer) {
                        rigid = monster.GetComponent<Rigidbody2D>();
                        animator = monster.GetComponent<Animator>();
                        spriteRenderer = monster.GetComponent<SpriteRenderer>();
                  }
            }
      }

      public class IdleState : ChargerState
      {
            public IdleState(Charger _monster) : base(_monster) { }

            public override void OnStateEnter()
            {
                  base.OnStateEnter();
            }

            public override void OnStateUpdate()
            {
                  base.OnStateUpdate();
            }

            public override void OnStateExit()
            {
                  if (!animator) animator = monster.GetComponent<Animator>();
                  animator.SetTrigger("Awake");
            }
      }

      public class MoveState : ChargerState
      {
            public MoveState(Charger _monster) : base(_monster) { }

            private bool isStateExit = false;

            public override void OnStateEnter()
            {
                  base.OnStateEnter();

                  SetInputVec(5);
                  if (photonView.IsMine) SetInputVec();
            }

            public override void OnStateUpdate()
            {
                  if (monster.OnSenseForward(0.95f, "Wall", "Obstacle")) {
                        SetInputVec();
                  }

                  MoveMonster();
            }

            public override void OnStateExit()
            {
                  isStateExit = true;
            }

            private async void SetInputVec(int _time)
            {
                  if (_time < 1) {
                        Debug.LogError($"{monster.name}: SetInputVec 호출 시간이 잘못되었습니다. (0 이하)");
                        return;
                  }

                  for (int i = 0; i < _time; ++i) {
                        await Task.Delay(1000); // 1 second
                        if (isStateExit) return;
                  }

                  if (photonView.IsMine) SetInputVec();
                  SetInputVec(_time);
            }

            private void SetInputVec()
            {
                  // 0: up, 1: down, 2: right, 3: left
                  int nextDirection = UnityEngine.Random.Range(0, 4);
                  nextDirection = monster.inputVec.y == 1 && nextDirection == 0 ? 1 : nextDirection;
                  nextDirection = monster.inputVec.y == -1 && nextDirection == 1 ? 0 : nextDirection;
                  nextDirection = monster.inputVec.x == 1 && nextDirection == 2 ? 3 : nextDirection;
                  nextDirection = monster.inputVec.x == -1 && nextDirection == 3 ? 2 : nextDirection;

                  if (nextDirection < 2) {
                        monster.inputVec.x = 0;
                        monster.inputVec.y = nextDirection == 0 ? 1 : -1;
                  }
                  else {
                        monster.inputVec.x = nextDirection == 2 ? 1 : -1;
                        monster.inputVec.y = 0;
                  }

                  //SetSpriteDirection();
                  fSMRPCController.FSMRPC_SetSpriteDirection(monster.inputVec);
            }

            #region Disuse due to PunRPC
            private void SetSpriteDirection()
            {
                  if (spriteRenderer) {
                        if (monster.inputVec.x > 0) {
                              spriteRenderer.flipX = false;
                              shadow[0].gameObject.SetActive(true);
                              shadow[1].gameObject.SetActive(false);
                        }
                        else if (monster.inputVec.x < 0) {
                              spriteRenderer.flipX = true;
                              shadow[0].gameObject.SetActive(true);
                              shadow[1].gameObject.SetActive(false);
                        }
                        else if (monster.inputVec.y != 0) {
                              shadow[0].gameObject.SetActive(false);
                              shadow[1].gameObject.SetActive(true);
                        }
                  }
                  if (animator) {
                        animator.SetInteger("XAxisRaw", (int)monster.inputVec.x);
                        animator.SetInteger("YAxisRaw", (int)monster.inputVec.y);
                  }
            }
            #endregion

            private void MoveMonster()
            {
                  rigid.AddForce(monster.inputVec.normalized * monster.stat.moveForce, ForceMode2D.Force);
                  if (rigid.velocity.magnitude > monster.stat.maxVelocity) {
                        rigid.velocity = rigid.velocity.normalized * monster.stat.maxVelocity;
                  }
            }
      }

      public class AttackState : ChargerState
      {
            public AttackState(Charger _monster) : base(_monster) { }

            private Vector2 inputVec;

            private bool isNullPlayerHit = false;

            public override void OnStateEnter()
            {
                  base.OnStateEnter();

                  if (monster.playerHit) {
                        Vector2 direction = monster.playerHit.point - rigid.position;
                        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y)) {
                              inputVec.x = Mathf.Sign(direction.x);
                              inputVec.y = 0;
                        }
                        else {
                              inputVec.x = 0;
                              inputVec.y = Mathf.Sign(direction.y);
                        }
                        monster.inputVec = inputVec;

                        //SetSpriteDirection();
                        fSMRPCController.FSMRPC_SetSpriteDirection(inputVec);
                        animator.SetBool("isAttack", true);
                  }
                  else if (!photonView.IsMine) {
                        Debug.LogWarning($"{monster.name}: AttackState에서 monster.playerHit를 찾지 못했습니다.");
                        monster.IsAttack = true;
                        isNullPlayerHit = true;
                  }
            }

            public override void OnStateUpdate()
            {
                  if (isNullPlayerHit) return;

                  // 공격 도중 소유권이 바뀌었을 때 처리
                  if (inputVec == null) {
                        inputVec = monster.inputVec;
                  }
                  else if (monster.inputVec != inputVec) {
                        monster.inputVec = inputVec;
                  }

                  MoveMonster();

                  AttackPlayer();
            }

            public override void OnStateExit()
            {
                  if (isNullPlayerHit) return;

                  if (photonView.IsMine) {
                        animator.SetBool("isAttack", false);
                  }
            }

            #region Disuse due to PunRPC
            private void SetSpriteDirection()
            {
                  if (inputVec.x > 0) {
                        spriteRenderer.flipX = false;
                        shadow[0].gameObject.SetActive(true);
                        shadow[1].gameObject.SetActive(false);
                  }
                  else if (inputVec.x < 0) {
                        spriteRenderer.flipX = true;
                        shadow[0].gameObject.SetActive(true);
                        shadow[1].gameObject.SetActive(false);
                  }
                  else if (inputVec.y != 0) {
                        shadow[0].gameObject.SetActive(false);
                        shadow[1].gameObject.SetActive(true);
                  }
                  animator.SetInteger("XAxisRaw", (int)inputVec.x);
                  animator.SetInteger("YAxisRaw", (int)inputVec.y);
                  animator.SetBool("isAttack", true);
            }
            #endregion

            private void MoveMonster()
            {
                  rigid.AddForce(inputVec.normalized * (monster.stat.moveForce + 10), ForceMode2D.Force);
                  if (rigid.velocity.magnitude > monster.stat.maxVelocity + 2) {
                        rigid.velocity = rigid.velocity.normalized * (monster.stat.maxVelocity + 1);
                  }
            }

            private void AttackPlayer()
            {
                  RaycastHit2D playerHit = monster.OnSenseForward(0.45f, "Player");
                  if (playerHit && !monster.IsAttack) {
                        monster.IsAttack = true;

                        if (playerHit.transform.TryGetComponent<IsaacBody>(out var player)) {
                              if (!player.IsHurt) {
                                    player.Health -= monster.stat.attackDamage;
                                    player.IsHurt = true;
                                    GameManager.Instance.uiManager.setKilledPlayer = "Charger";
                              }
                        }
                  }
            }
      }

      public class DeadState : ChargerState
      {
            public DeadState(Charger _monster) : base(_monster) { }

            public override void OnStateEnter()
            {
                  // 
            }

            public override void OnStateUpdate()
            {
                  // 
            }

            public override void OnStateExit()
            {
                  // 
            }
      }
}
