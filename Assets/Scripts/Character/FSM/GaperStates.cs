using Photon.Pun;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace GaperStates
{
      public abstract class GaperState : BaseState<Gaper>
      {
            protected PhotonView photonView;
            protected GaperFSMRPC gaperFSMRPC;

            protected Rigidbody2D rigid;
            protected Animator animator;
            protected SpriteRenderer spriteRenderer;

            protected GameObject monsterHead;

            public GaperState(Gaper _monster) : base(_monster) { }

            public override void OnStateEnter()
            {
                  photonView = monster.GetComponent<PhotonView>();
                  gaperFSMRPC = monster.GetComponent<GaperFSMRPC>();

                  rigid = monster.GetComponent<Rigidbody2D>();
                  animator = monster.GetComponent<Animator>();
                  spriteRenderer = monster.GetComponent<SpriteRenderer>();

                  foreach (FlashEffect component in monster.GetComponentsInChildren<FlashEffect>(true)) {
                        if (component == monster.GetComponent<FlashEffect>()) continue;
                        monsterHead = component.gameObject;
                        break;
                  }
            }

            public override void OnStateUpdate()
            {
                  if (!rigid || !animator || !spriteRenderer) {
                        rigid = monster.GetComponent<Rigidbody2D>();
                        animator = monster.GetComponent<Animator>();
                        spriteRenderer = monster.GetComponent<SpriteRenderer>();
                  }
            }

            public override void OnStateExit()
            {
                  if (!animator) {
                        animator = monster.GetComponent<Animator>();
                  }
            }
      }

      public class IdleState : GaperState
      {
            public IdleState(Gaper _monster) : base(_monster) { }

            public override void OnStateEnter()
            {
                  base.OnStateEnter();

                  monsterHead.SetActive(true);
            }

            public override void OnStateUpdate()
            {
                  base.OnStateUpdate();
            }

            public override void OnStateExit()
            {
                  base.OnStateExit();
                  //Debug.LogError(animator);
                  animator.SetTrigger("Awake");
            }
      }

      public class MoveState : GaperState
      {
            public MoveState(Gaper _monster) : base(_monster) { }

            private float moveForce, maxVelocity;

            private IsaacBody player;
            private Rigidbody2D playerRigid;
            private Queue<Vector2> followQueue = new();

            private bool isJustBody = false;
            private bool isStateExit = false;

            public override void OnStateEnter()
            {
                  base.OnStateEnter();
                  moveForce = monster.stat.moveForce;
                  maxVelocity = monster.stat.maxVelocity;

                  GetPlayerObject();
            }

            public override void OnStateUpdate()
            {
                  if (player == null) {
                        GetPlayerObject();
                        return;
                  }

                  if (OnHalfHealth() && !isJustBody) {
                        isJustBody = true;
                        //monsterHead.SetActive(false);
                        //monster.SpawnBloodEffects();
                        if (!gaperFSMRPC.alreadyJustBody) {
                              gaperFSMRPC.FSMRPC_OnceIsJustBody();
                        }

                        moveForce = monster.stat.moveForce - 1;
                        maxVelocity = monster.stat.maxVelocity - 0.5f;
                        SetInputVec(1);
                  }

                  if (!isJustBody) {
                        followQueue.Enqueue(playerRigid.position);
                        if (!photonView.IsMine) {
                              followQueue = gaperFSMRPC.followQueue;
                        }
                        else if (followQueue.Count > monster.followDelay) {
                              SetInputVec();
                              MoveMonster();
                        }
                  }
                  else {
                        MoveMonster();
                  }

                  OnCollisionEnter2D();
            }

            public override void OnStateExit()
            {
                  isStateExit = true;
            }

            private void GetPlayerObject()
            {
                  if (monster.playerSearchBox == default) monster.playerSearchBox = Vector2.one * 40;
                  if (Physics2D.BoxCast(rigid.position, monster.playerSearchBox, 0, Vector2.zero, 0,
                      LayerMask.GetMask("Player")) is RaycastHit2D _player) {
                        player = _player.transform.GetComponent<IsaacBody>();
                        playerRigid = player.GetComponent<Rigidbody2D>();
                        followQueue.Enqueue(playerRigid.position);
                  }
            }

            private async void SetInputVec(int _time = 2)
            {
                  if (_time < 1) {
                        Debug.LogError($"{monster.name}: SetInputVec 호출 시간이 잘못되었습니다. (0 이하)");
                        return;
                  }

                  for (int i = 0; i < _time; ++i) {
                        await Task.Delay(1000); // 1 second
                        if (isStateExit) return;
                  }

                  SetInputVec();
                  SetInputVec(_time);
            }

            private void SetInputVec()
            {
                  if (!photonView.IsMine) return;

                  if (isJustBody) {
                        monster.inputVec = new Vector2(UnityEngine.Random.Range(-1, 2), UnityEngine.Random.Range(-1, 2));
                        //SetSpriteDirection(monster.inputVec.x > monster.inputVec.y);
                        gaperFSMRPC.FSMRPC_SetSpriteDirection(monster.inputVec, monster.inputVec.x > monster.inputVec.y);
                  }
                  else {
                        Vector2 chaseDirection = followQueue.Dequeue() - this.rigid.position;
                        monster.inputVec.x = Mathf.Sign(chaseDirection.x);
                        monster.inputVec.y = Mathf.Sign(chaseDirection.y);
                        //SetSpriteDirection(Mathf.Abs(chaseDirection.x) > Mathf.Abs(chaseDirection.y));
                        gaperFSMRPC.FSMRPC_SetSpriteDirection(monster.inputVec, Mathf.Abs(chaseDirection.x) > Mathf.Abs(chaseDirection.y));
                  }
            }

            #region Disuse due to PunRPC
            private void SetSpriteDirection(bool xGreaterThanY)
            {
                  if (spriteRenderer) {
                        if (monster.inputVec.x > 0) {
                              spriteRenderer.flipX = false;
                        }
                        else if (monster.inputVec.x < 0) {
                              spriteRenderer.flipX = true;
                        }
                  }

                  if (animator) {
                        if (xGreaterThanY) {
                              animator.SetInteger("XAxisRaw", (int)monster.inputVec.x);
                              animator.SetInteger("YAxisRaw", 0);
                        }
                        else {
                              animator.SetInteger("XAxisRaw", 0);
                              animator.SetInteger("YAxisRaw", (int)monster.inputVec.y);
                        }
                  }
            }
            #endregion

            private void MoveMonster()
            {
                  rigid.AddForce(monster.inputVec.normalized * moveForce, ForceMode2D.Force);
                  if (rigid.velocity.magnitude > maxVelocity) {
                        rigid.velocity = rigid.velocity.normalized * maxVelocity;
                  }
            }

            private void OnCollisionEnter2D()
            {
                  if (player.IsHurt) return;

                  if (monster.collisionCircle == default) monster.collisionCircle = 0.35f;
                  if (Physics2D.CircleCast(rigid.position, monster.collisionCircle, Vector2.zero, 0,
                      LayerMask.GetMask("Player"))) {
                        player.Health -= monster.stat.attackDamage;
                        player.IsHurt = true;
                        GameManager.Instance.uiManager.setKilledPlayer = "Gaper";
                  }
            }

            private bool OnHalfHealth()
            {
                  return monster.stat.health <= monster.stat.maxHealth / 2;
            }
      }

      public class DeadState : GaperState
      {
            public DeadState(Gaper _monster) : base(_monster) { }

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
