using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using ObstacleSpace;
using Photon.Pun;

namespace PooterStates
{
      public abstract class PooterState : BaseState<Pooter>
      {
            protected PhotonView photonView;
            protected PooterFSMRPC pooterFSMRPC;

            protected Rigidbody2D rigid;
            protected Animator animator;
            protected SpriteRenderer spriteRenderer;

            public PooterState(Pooter _monster) : base(_monster) { }

            public override void OnStateEnter()
            {
                  photonView = monster.GetComponent<PhotonView>();
                  pooterFSMRPC = monster.GetComponent<PooterFSMRPC>();

                  rigid = monster.GetComponent<Rigidbody2D>();
                  animator = monster.GetComponent<Animator>();
                  spriteRenderer = monster.GetComponent<SpriteRenderer>();
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

      public class IdleState : PooterState
      {
            public IdleState(Pooter _monster) : base(_monster) { }

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
                  base.OnStateExit();
                  //Debug.LogError(animator);
                  animator.SetTrigger("Awake");
            }
      }

      public class MoveState : PooterState
      {
            public MoveState(Pooter _monster) : base(_monster) { }

            private bool isStateExit = false;

            public override void OnStateEnter()
            {
                  base.OnStateEnter();
                  // (애니메이션 이름, 현재 활성화 중인 레이어, 0~1까지로 0.5는 절반)
                  animator.Play("AM_PooterMove", -1, UnityEngine.Random.Range(0f, 1f));

                  SetInputVec(1);
                  SetInputVec();
            }

            public override void OnStateUpdate()
            {
                  MoveMonster();
            }

            public override void OnStateExit()
            {
                  isStateExit = true;
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

                  monster.inputVec = new Vector2(UnityEngine.Random.Range(-1, 2), UnityEngine.Random.Range(-1, 2));

                  //SetSpriteDirection();
                  pooterFSMRPC.FSMRPC_SetSpriteDirection(monster.inputVec);
            }

            #region Disuse due to PunRPC
            private void SetSpriteDirection()
            {
                  if (!spriteRenderer) return;

                  if (monster.inputVec.x > 0) {
                        spriteRenderer.flipX = false;
                  }
                  else if (monster.inputVec.x < 0) {
                        spriteRenderer.flipX = true;
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

      public class AttackState : PooterState, ITearShooter
      {
            public AttackState(Pooter _monster) : base(_monster) { }

            private const TearFactory.Tears tearType = TearFactory.Tears.Basic;
            //private GameObject firstTear, secondTear;

            public override void OnStateEnter()
            {
                  base.OnStateEnter();

                  if (monster.playerHit is RaycastHit2D playerHit && photonView.IsMine) {
                        //directionVec = playerHit.point - rigid.position;
                        pooterFSMRPC.FSMRPC_SetDirectionVec(playerHit.point - rigid.position);

                        if (Mathf.Sign(pooterFSMRPC.directionVec.x) > 0) {
                              //spriteRenderer.flipX = false;
                              pooterFSMRPC.FSMRPC_SetSpriteDirection(Vector2.right);
                        }
                        else {
                              //spriteRenderer.flipX = true;
                              pooterFSMRPC.FSMRPC_SetSpriteDirection(Vector2.left);
                        }

                        //animator.SetTrigger("Attack");
                        pooterFSMRPC.FSMRPC_SetTrigger("Attack");
                  }
                  else if (photonView.IsMine) {
                        Debug.LogWarning($"{monster.name}: AttackState에서 monster.playerHit를 찾지 못했습니다.");
                        // (애니메이션 이름, 현재 활성화 중인 레이어, 0~1까지로 0.5는 절반)
                        animator.Play("AM_PooterAttack", -1, 0.9f); // AM_PooterAttack 마지막에는 상태를 바꾸는 이벤트 존재
                  }
            }

            public override void OnStateUpdate()
            {
                  // 공격 도중 소유권이 바뀌었을 때 처리

                  //if (monster.isAttackTiming[0] && !firstTear) {
                  //      firstTear = GameManager.Instance.monsterTearFactory.GetTear(tearType, true);
                  //      AttackUsingTear(firstTear);
                  //}
                  //else if (monster.isAttackTiming[1] && !secondTear) {
                  //      secondTear = GameManager.Instance.monsterTearFactory.GetTear(tearType, true);
                  //      AttackUsingTear(secondTear);
                  //}
                  if (monster.IsAttackTiming[0]) {
                        //Debug.LogError(1);
                        monster.IsAttackTiming = new bool[] { false, monster.IsAttackTiming[1] };
                        pooterFSMRPC.FSMRPC_GetTearAndAttack(tearType);
                  }
                  else if (monster.IsAttackTiming[1]) {
                        //Debug.LogError(2);
                        monster.IsAttackTiming = new bool[] { monster.IsAttackTiming[0], false };
                        pooterFSMRPC.FSMRPC_GetTearAndAttack(tearType);
                  }
            }

            public override void OnStateExit()
            {
                  pooterFSMRPC.directionVec = Vector2.zero;

                  monster.IsAttackTiming = new bool[] { false, false };
            }

            #region Disuse due to PunRPC
            public void AttackUsingTear(GameObject curTear = default)
            {
                  TearIgnoreObstacle(curTear);

                  SetTearPositionAndDirection(curTear, out Rigidbody2D tearRigid);
                  if (tearRigid == default) {
                        Debug.LogWarning($"{monster.name}'s tears don't have Rigidbody2D!");
                        return;
                  }

                  SetTearVelocity(out Vector2 tearVelocity, tearRigid);
                  ShootSettedTear(curTear, tearRigid, tearVelocity);
            }

            private void TearIgnoreObstacle(GameObject curTear)
            {
                  // 현재 몬스터가 있는 방 찾기
                  if (monster.GetComponentInParent<AddRoom>() is AddRoom room) {
                        // 현재 방에서 이름이 Obstacles인 오브젝트 찾기
                        Transform obstacles = default;
                        foreach (Transform child in room.transform) {
                              if (child.name == "Obstacles") {
                                    obstacles = child;
                                    break;
                              }
                        }

                        if (obstacles == null) {
                              Debug.LogWarning("No object named 'Obstacles' found in the room.");
                              return;
                        }

                        Collider2D thisCollider = curTear.GetComponent<Collider2D>();
                        foreach (Obstacle obstacle in obstacles.GetComponentsInChildren<Obstacle>(true)) {
                              Physics2D.IgnoreCollision(thisCollider, obstacle.GetComponent<Collider2D>(), true);
                        }
                  }
            }

            public void SetTearPositionAndDirection(GameObject curTear, out Rigidbody2D tearRigid)
            {
                  if (curTear.GetComponent<Tear>() is Tear tear &&
                        curTear.GetComponent<Rigidbody2D>() is Rigidbody2D curRigid) {
                        Vector2 offset = new Vector2(0, -0.35f);
                        // Up: 0, Down: 1, Right: 2, Left: 3
                        if (pooterFSMRPC.directionVec.x > 0) {
                              tear.tearDirection = 2;
                        }
                        else if (pooterFSMRPC.directionVec.x < 0) {
                              tear.tearDirection = 3;
                        }
                        else if (pooterFSMRPC.directionVec.y > 0) {
                              tear.tearDirection = 0;
                        }
                        else {
                              tear.tearDirection = 1;
                        }

                        tearRigid = curRigid;
                        tearRigid.position = rigid.position + offset;
                  }
                  else {
                        tearRigid = default;
                  }
            }

            public void SetTearVelocity(out Vector2 tearVelocity, Rigidbody2D tearRigid)
            {
                  tearVelocity.x = Mathf.Clamp(pooterFSMRPC.directionVec.x, -1, 1);
                  tearVelocity.y = Mathf.Clamp(pooterFSMRPC.directionVec.y, -1, 1);

                  tearRigid.velocity = Vector2.zero;
            }

            public void ShootSettedTear(GameObject curTear, Rigidbody2D tearRigid, Vector2 tearVelocity)
            {
                  Vector2 inputVec = Mathf.Abs(pooterFSMRPC.directionVec.x) > Mathf.Abs(pooterFSMRPC.directionVec.y) ?
                        Vector2.right * Mathf.Sign(pooterFSMRPC.directionVec.x) : Vector2.up * Mathf.Sign(pooterFSMRPC.directionVec.y);
                  float adjustedSpeed = inputVec.y < 0 ? monster.stat.tearSpeed * 0.75f : monster.stat.tearSpeed;
                  tearRigid.AddForce(inputVec * adjustedSpeed + tearVelocity, ForceMode2D.Impulse);
            }
            #endregion
      }

      public class DeadState : PooterState
      {
            public DeadState(Pooter _monster) : base(_monster) { }

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
