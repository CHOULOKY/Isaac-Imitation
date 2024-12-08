using Photon.Pun;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace MonstroStates
{
      public abstract class MonstroState : BaseState<Monstro>, ITearShooter
      {
            protected PhotonView photonView;
            protected MonstroFSMRPC monstroFSMRPC;

            protected Rigidbody2D rigid;
            protected Collider2D monsterCollider;
            protected Animator animator;
            protected SpriteRenderer spriteRenderer;
            //protected SpriteRenderer playerRenderer;

            protected Transform shadow;
            protected Collider2D shadowCollider;

            protected const TearFactory.Tears tearType = TearFactory.Tears.Boss;

            public MonstroState(Monstro _monster) : base(_monster) { }

            public override void OnStateEnter()
            {
                  photonView = monster.GetComponent<PhotonView>();
                  monstroFSMRPC = monster.GetComponent<MonstroFSMRPC>();

                  rigid = monster.GetComponent<Rigidbody2D>();
                  monsterCollider = monster.GetComponent<Collider2D>();
                  animator = monster.GetComponent<Animator>();
                  spriteRenderer = monster.GetComponent<SpriteRenderer>();

                  //if (monster.player == null) monster.player = Object.FindObjectsOfType<IsaacBody>(true).FirstOrDefault();
                  //playerRenderer = monster.player.GetComponent<SpriteRenderer>();
                  //Debug.LogError($"{monster} + {monster.player} + {playerRenderer}");

                  foreach (Transform child in monster.GetComponentsInChildren<Transform>()) {
                        if (child.name == "Shadow") {
                              shadow = child;
                              shadowCollider = shadow.GetComponent<Collider2D>();
                              break;
                        }
                  }
            }

            public override void OnStateExit()
            {
                  if (!animator) {
                        animator = monster.GetComponent<Animator>();
                  }
            }

            protected virtual void SpriteXToTarget(Transform target)
            {
                  //directionVec = (Vector2)target.position - rigid.position;
                  monstroFSMRPC.FSMRPC_SetDirectionVec((Vector2)target.position - rigid.position);
                  if (Mathf.Sign(monstroFSMRPC.directionVec.x) > 0) {
                        //spriteRenderer.flipX = true;
                        monstroFSMRPC.FSMRPC_SetSpriteDirection(Vector2.left);
                  }
                  else {
                        //spriteRenderer.flipX = false;
                        monstroFSMRPC.FSMRPC_SetSpriteDirection(Vector2.right);
                  }
            }

            protected virtual void OnCollisionEnter2D()
            {
                  if (monstroFSMRPC.player.IsHurt) return;

                  if (Physics2D.BoxCast(monsterCollider.bounds.center, monsterCollider.bounds.size, 0, Vector2.zero, 0,
                        LayerMask.GetMask("Player"))) {
                        // Debug.Log("Player is on Monster collision!");
                        monstroFSMRPC.player.Health -= monster.stat.attackDamage;
                        monstroFSMRPC.player.IsHurt = true;
                        GameManager.Instance.uiManager.setKilledPlayer = "Monstro";
                  }
            }

            #region Control Exclude Layers
            // Exclude Layers에 레이어가 존재하는지 확인하는 함수
            protected virtual bool IsLayerExcluded(Collider2D collider, int layer)
            {
                  // 비트 연산으로 레이어가 포함되어 있는지 확인
                  return (collider.excludeLayers & (1 << layer)) != 0;
            }

            // Exclude Layers에 레이어를 추가하는 함수
            protected virtual void AddExcludeLayerToCollider(Collider2D collider, int layer)
            {
                  bool isMonsterColl = false;
                  if (collider.transform.name.Contains("Shadow")) isMonsterColl = true;
                  else isMonsterColl = false;

                  // 현재 excludeLayers에 layerToAdd를 추가
                  //collider.excludeLayers |= (1 << layer);
                  monstroFSMRPC.FSMRPC_AddExcludeLayerToCollider(isMonsterColl: isMonsterColl, layer);
            }

            // Exclude Layers에 레이어를 제거하는 함수
            protected virtual void RemoveExcludeLayerFromCollider(Collider2D collider, int layer)
            {
                  bool isMonsterColl = false;
                  if (collider.transform.name.Contains("Shadow")) isMonsterColl = true;
                  else isMonsterColl = false;

                  // 현재 excludeLayers에서 layerToRemove를 제거
                  //collider.excludeLayers &= ~(1 << layer);
                  monstroFSMRPC.FSMRPC_RemoveExcludeLayerFromCollider(isMonsterColl: isMonsterColl, layer);
            }
            #endregion

            #region Tear - Disuse due to PunRPC
            //protected Vector2 directionVec;

            public void AttackUsingTear(GameObject curTear = default)
            {
                  SetTearPositionAndDirection(curTear, out Rigidbody2D tearRigid);
                  if (tearRigid == default) {
                        Debug.LogWarning($"{monster.name}'s tears don't have Rigidbody2D!");
                        return;
                  }

                  SetTearVelocity(out Vector2 tearVelocity, tearRigid);
                  ShootSettedTear(curTear, tearRigid, tearVelocity);
            }

            public void SetTearPositionAndDirection(GameObject curTear, out Rigidbody2D tearRigid)
            {
                  if (curTear.GetComponent<Tear>() is Tear tear &&
                        curTear.GetComponent<Rigidbody2D>() is Rigidbody2D curRigid) {
                        Vector2 offset = new Vector2(0, -0.3f);
                        // Up: 0, Down: 1, Right: 2, Left: 3
                        if (monstroFSMRPC.directionVec.x > 0) {
                              tear.tearDirection = 2;
                        }
                        else if (monstroFSMRPC.directionVec.x < 0) {
                              tear.tearDirection = 3;
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
                  // Not used
                  tearVelocity = Vector2.zero;

                  tearRigid.velocity = Vector2.zero;
            }

            public void ShootSettedTear(GameObject curTear, Rigidbody2D tearRigid, Vector2 tearVelocity)
            {
                  float rotateAngle = default;
                  if (GetType() == typeof(TearSprayState)) {
                        rotateAngle = UnityEngine.Random.Range(-25f, 25f);
                  }
                  else if (GetType() == typeof(BigJumpState)) {
                        rotateAngle = UnityEngine.Random.Range(-180f, 180f);
                  }

                  Vector2 inputVec = RotateVector(monstroFSMRPC.directionVec.normalized, rotateAngle);
                  float adjustedSpeed = UnityEngine.Random.Range(monster.stat.tearSpeed - 1, monster.stat.tearSpeed + 2);
                  tearRigid.AddForce(inputVec * adjustedSpeed + tearVelocity, ForceMode2D.Impulse);
            }

            protected Vector2 RotateVector(Vector2 v, float angle)
            {
                  // 1. 각도를 라디안으로 변환
                  float radian = angle * Mathf.Deg2Rad;

                  // 2. 회전 행렬의 요소 계산
                  float cos = Mathf.Cos(radian);
                  float sin = Mathf.Sin(radian);

                  // 3. 회전 행렬 적용
                  return new Vector2(
                      v.x * cos - v.y * sin,
                      v.x * sin + v.y * cos
                  );
            }
            #endregion

            protected async void DelaySpawnBlood(float time = 0)
            {
                  await Task.Delay((int)(1000 * time));

                  //monster.SpawnBloodEffects();
                  monstroFSMRPC.FSMRPC_SpawnBloodEffects();
            }
      }

      public class IdleState : MonstroState
      {
            public IdleState(Monstro _monster) : base(_monster) { }

            public override void OnStateEnter()
            {
                  //if (monster.player == null) monster.player = GetPlayerObject();

                  base.OnStateEnter();
            }

            public override void OnStateUpdate()
            {
                  //if (!rigid || !monsterCollider || !animator || !spriteRenderer) {
                  //      rigid = monster.GetComponent<Rigidbody2D>();
                  //      monsterCollider = monster.GetComponent<Collider2D>();
                  //      animator = monster.GetComponent<Animator>();
                  //      spriteRenderer = monster.GetComponent<SpriteRenderer>();
                  //      return;
                  //}

                  //if (monster.player == null) monster.player = GetPlayerObject();
                  //else if (playerRenderer == null) playerRenderer = monster.player.GetComponent<SpriteRenderer>();
                  //else monster.sortRendererBy.SortBy(spriteRenderer, playerRenderer, false);

                  //OnCollisionEnter2D();
            }

            public override void OnStateExit()
            {
                  //if (monster.player == null) monster.player = GetPlayerObject();
                  //if (monster.player == null) monster.player = Object.FindObjectsOfType<IsaacBody>(true).FirstOrDefault();
                  //else if (playerRenderer == null) playerRenderer = monster.player.GetComponent<SpriteRenderer>();

                  base.OnStateExit();
                  //Debug.LogError(animator);
                  animator.SetTrigger("Awake");
                  //monstroFSMRPC.FSMRPC_SetTrigger("Awake");
            }

            //private IsaacBody GetPlayerObject()
            //{
            //      if (monster.playerSearchBox == default) monster.playerSearchBox = Vector2.one * 40;
            //      if (Physics2D.BoxCast(rigid.position, monster.playerSearchBox, 0, Vector2.zero, 0,
            //          LayerMask.GetMask("Player")) is RaycastHit2D _player) {
            //            return _player.transform.GetComponent<IsaacBody>();
            //      }
            //      else {
            //            return null;
            //      }
            //}
      }

      public class SmallJumpState : MonstroState
      {
            public SmallJumpState(Monstro _monster) : base(_monster) { }

            private Vector2 shadowOffset;
            
            private float animationLength;

            public override void OnStateEnter()
            {
                  base.OnStateEnter();

                  if (photonView.IsMine) {
                        // 점프 횟수 초기 설정
                        switch (UnityEngine.Random.Range(0, 5)) {
                              case 0: monstroFSMRPC.maxJumpCount = 1; break;
                              case 1: case 2: case 3: monstroFSMRPC.maxJumpCount = 3; break;
                              case 4: monstroFSMRPC.maxJumpCount = 5; break;
                        }
                        monstroFSMRPC.FSMRPC_SetMaxJumpCount(monstroFSMRPC.maxJumpCount);
                  }

                  shadowOffset = shadow.localPosition;
                  shadow.parent = null;

                  // 애니메이션의 길이 가져오기
                  animationLength = animator.runtimeAnimatorController.animationClips
                      .FirstOrDefault(clip => clip.name == "AM_MonstroSmallJump")?.length ?? 0f;
                  monstroFSMRPC.elapsedAnimationTime = 0;

                  if (photonView.IsMine) {
                        animator.SetBool("SmallJump", true);
                        monstroFSMRPC.FSMRPC_SetElapsedTime(0);
                  }
            }

            public override void OnStateUpdate()
            {
                  SetBeforeNextJump();
                  MoveShadow();
                  JumpToShadow();

                  if (monster.IsOnLand) {
                        OnCollisionEnter2D();
                        if (IsLayerExcluded(monsterCollider, LayerMask.NameToLayer("Tear")) ||
                              IsLayerExcluded(shadowCollider, LayerMask.NameToLayer("Player"))) {
                              RemoveExcludeLayerFromCollider(monsterCollider, LayerMask.NameToLayer("Tear"));
                              RemoveExcludeLayerFromCollider(shadowCollider, LayerMask.NameToLayer("Player"));
                        }
                  }

                  //monster.sortRendererBy.SortBy(spriteRenderer, playerRenderer, false);
                  monstroFSMRPC.FSMRPC_SoryBy();
            }

            public override void OnStateExit()
            {
                  monster.IsOnLand = false;

                  shadow.parent = monster.transform;
                  shadow.localPosition = shadowOffset;
                  if (IsLayerExcluded(monsterCollider, LayerMask.NameToLayer("Tear")) ||
                        IsLayerExcluded(shadowCollider, LayerMask.NameToLayer("Player"))) {
                        RemoveExcludeLayerFromCollider(monsterCollider, LayerMask.NameToLayer("Tear"));
                        RemoveExcludeLayerFromCollider(shadowCollider, LayerMask.NameToLayer("Player"));
                  }

                  if (photonView.IsMine) {
                        animator.SetBool("SmallJump", false);
                  }

                  // monstroFSMRPC Variables
                  monstroFSMRPC.curJumpCount = 0;
                  monstroFSMRPC.maxJumpCount = 0;
                  monstroFSMRPC.elapsedAnimationTime = 0;
            }

            private void SetBeforeNextJump()
            {
                  //elapsedAnimationTime += Time.deltaTime;
                  // 애니메이션이 끝나면 다시 재생
                  if (monstroFSMRPC.elapsedAnimationTime >= animationLength || monstroFSMRPC.curJumpCount == monstroFSMRPC.maxJumpCount) {
                        if (monstroFSMRPC.curJumpCount > 0) {
                              //elapsedAnimationTime = 0f;
                              monstroFSMRPC.FSMRPC_SetElapsedTime(0);
                              //curjumpCount--;
                              monstroFSMRPC.FSMRPC_SetCurJumpCount(monstroFSMRPC.curJumpCount--);

                              //animator.Play("AM_MonstroSmallJump", 0, 0f); // 0프레임부터 재생
                              //Debug.LogError(1);
                              monstroFSMRPC.FSMRPC_AnimatorPlay("AM_MonstroSmallJump", 0f);
                              SpriteXToTarget(monstroFSMRPC.player.transform);

                              // 다음 위치로 이동하기 위한 설정
                              //nextPosition = GetNextPosition(2.5f);
                              monstroFSMRPC.FSMRPC_SetNextPosition(shadow, 2.5f);

                              monster.IsOnLand = false;
                              if (!IsLayerExcluded(monsterCollider, LayerMask.NameToLayer("Tear")) ||
                                    !IsLayerExcluded(shadowCollider, LayerMask.NameToLayer("Player"))) {
                                    AddExcludeLayerToCollider(monsterCollider, LayerMask.NameToLayer("Tear"));
                                    AddExcludeLayerToCollider(shadowCollider, LayerMask.NameToLayer("Player"));
                              }
                        }
                        else {
                              // 점프 횟수 모두 소진되면 상태 변경
                              monster.IsSmallJump = false;
                        }
                  }
            }

            #region Disuse due to PunRPC
            private Vector2 GetNextPosition(float distance)
            {
                  Vector3 nextDirection = monstroFSMRPC.player.transform.position - shadow.position;
                  Vector3 nextPosition = nextDirection.normalized * distance;
                  return shadow.position + nextPosition;
            }
            #endregion

            private void MoveShadow()
            {
                  // 그림자 등속 이동
                  shadow.position = Vector2.Lerp(shadow.position, monstroFSMRPC.nextPosition, 
                        monstroFSMRPC.elapsedAnimationTime / animationLength);
            }

            private void JumpToShadow()
            {
                  // 몬스터 등속 이동
                  if (monstroFSMRPC.elapsedAnimationTime < animationLength / 2) {
                        rigid.position = Vector2.Lerp(rigid.position, monstroFSMRPC.nextPosition + Vector2.up * 3f,
                              monstroFSMRPC.elapsedAnimationTime / (animationLength / 2));
                  }
                  else {
                        rigid.position = Vector2.Lerp(rigid.position, monstroFSMRPC.nextPosition + Vector2.up * -shadowOffset,
                              monstroFSMRPC.elapsedAnimationTime / animationLength);
                  }
            }
      }

      public class BigJumpState : MonstroState
      {
            public BigJumpState(Monstro _monster) : base(_monster) { }

            private Vector2 jumpUpPosition;
            private Vector2 shadowOffset;
            private float jumpDownSpeed = 40f;

            private float jumpDownDelay = 2f;

            private float animationLength;

            public override void OnStateEnter()
            {
                  base.OnStateEnter();

                  jumpUpPosition = rigid.position + Vector2.up * 15;
                  shadowOffset = shadow.localPosition;
                  shadow.parent = null;

                  // 애니메이션의 길이 가져오기
                  animationLength = animator.runtimeAnimatorController.animationClips
                      .FirstOrDefault(clip => clip.name == "AM_MonstroBigJumpDown")?.length / 0.65f ?? 0f;

                  if (photonView.IsMine) {
                        //animator.SetTrigger("BigJumpUp");
                        monstroFSMRPC.FSMRPC_SetTrigger("BigJumpUp");
                  }
            }

            public override void OnStateUpdate()
            {
                  if (monster.IsJumpUp) {
                        if (!IsLayerExcluded(monsterCollider, LayerMask.NameToLayer("Tear")) ||
                              !IsLayerExcluded(shadowCollider, LayerMask.NameToLayer("Player"))) {
                              AddExcludeLayerToCollider(monsterCollider, LayerMask.NameToLayer("Tear"));
                              AddExcludeLayerToCollider(shadowCollider, LayerMask.NameToLayer("Player"));
                        }

                        // 몬스터 화면 밖으로 점프
                        rigid.position = Vector2.Lerp(rigid.position, jumpUpPosition, 0.01f);

                        monstroFSMRPC.curJumpDownDelayTime += Time.deltaTime;
                        if (monstroFSMRPC.curJumpDownDelayTime > jumpDownDelay) {
                              rigid.position = new Vector2(shadow.position.x, rigid.position.y); // 그림자와 X값만 일치
                              monster.IsJumpUp = false;
                              //animator.SetTrigger("BigJumpDown");
                              monstroFSMRPC.FSMRPC_SetTrigger("BigJumpDown");
                              SpriteXToTarget(monstroFSMRPC.player.transform);
                        }
                        MoveShadow();
                  }
                  else if (monstroFSMRPC.curJumpDownDelayTime > jumpDownDelay) {
                        LandOnShadow();
                        if (monster.IsOnLand) {
                              if (!monstroFSMRPC.isTearSparied) {
                                    //isTearSparied = true;
                                    monstroFSMRPC.FSMRPC_SetisTearSparied(true);
                                    TearSpray();
                                    DelaySpawnBlood();
                                    monstroFSMRPC.FSMRPC_SetElapsedTime(0);
                              }
                              if (IsLayerExcluded(monsterCollider, LayerMask.NameToLayer("Tear")) ||
                                    IsLayerExcluded(shadowCollider, LayerMask.NameToLayer("Player"))) {
                                    RemoveExcludeLayerFromCollider(monsterCollider, LayerMask.NameToLayer("Tear"));
                                    RemoveExcludeLayerFromCollider(shadowCollider, LayerMask.NameToLayer("Player"));
                              }

                              OnCollisionEnter2D();

                              //elapsedAnimationTime += Time.deltaTime;
                              // AM_MonstroBigJumpDown 애니메이션이 끝나면 상태 변경
                              if (monstroFSMRPC.elapsedAnimationTime >= animationLength && monstroFSMRPC.isTearSparied) {
                                    monster.IsBigJump = false;
                              }
                        }
                  }

                  monstroFSMRPC.FSMRPC_SoryBy();
            }

            public override void OnStateExit()
            {
                  monster.IsOnLand = false;

                  shadow.parent = monster.transform;
                  shadow.localPosition = shadowOffset;
                  if (IsLayerExcluded(monsterCollider, LayerMask.NameToLayer("Tear")) ||
                        IsLayerExcluded(shadowCollider, LayerMask.NameToLayer("Player"))) {
                        RemoveExcludeLayerFromCollider(monsterCollider, LayerMask.NameToLayer("Tear"));
                        RemoveExcludeLayerFromCollider(shadowCollider, LayerMask.NameToLayer("Player"));
                  }

                  monstroFSMRPC.curJumpDownDelayTime = 0;
                  monstroFSMRPC.playerLatePosition = default;
                  monstroFSMRPC.isTearSparied = false;
                  monstroFSMRPC.elapsedAnimationTime = 0;
            }

            private void MoveShadow()
            {
                  Vector2 nextPosition = monstroFSMRPC.player.transform.position;

                  // 플레이어의 과거 위치 저장
                  if (monstroFSMRPC.curJumpDownDelayTime > jumpDownDelay / 2 && monstroFSMRPC.playerLatePosition == default) {
                        //playerLatePosition = nextPosition;
                        monstroFSMRPC.FSMRPC_SetPlayerLatePosition(nextPosition);
                  }

                  // 그림자 보간 이동
                  float lerpTime = Mathf.Clamp01(monstroFSMRPC.curJumpDownDelayTime / jumpDownDelay);
                  shadow.position = Vector2.Lerp(shadow.position, 
                        monstroFSMRPC.playerLatePosition == default ? nextPosition : monstroFSMRPC.playerLatePosition, lerpTime);
            }

            private void LandOnShadow()
            {
                  // 몬스터 그림자 위로 착지
                  rigid.position = Vector2.MoveTowards(rigid.position, (Vector2)shadow.position - shadowOffset,
                        jumpDownSpeed * Time.deltaTime);
            }

            private void TearSpray()
            {
                  int tearCount = UnityEngine.Random.Range(15, 20);
                  //for (int i = 0; i < tearCount; i++) {
                  //      AttackUsingTear(GameManager.Instance.monsterTearFactory.GetTear(tearType, true));
                  //}
                  monstroFSMRPC.FSMRPC_GetTearAndAttack(tearType, tearCount, false);
            }
      }

      public class TearSprayState : MonstroState
      {
            public TearSprayState(Monstro _monster) : base(_monster) { }

            private float animationLength;

            public override void OnStateEnter()
            {
                  base.OnStateEnter();

                  if (monstroFSMRPC.player && photonView.IsMine) {
                        //sprayCount = UnityEngine.Random.Range(0, 5) == 0 ? 1 : 2;
                        //curSprayCount = sprayCount;
                        monstroFSMRPC.FSMRPC_SetSprayCount(UnityEngine.Random.Range(0, 5) == 0 ? 1 : 2);
                  }
                  else if (photonView.IsMine) {
                        Debug.LogWarning($"{monster.name}: TearSprayState에서 monster.player를 찾지 못했습니다.");
                  }

                  monstroFSMRPC.elapsedAnimationTime = 0;
                  animationLength = animator.runtimeAnimatorController.animationClips
                              .FirstOrDefault(clip => clip.name == "AM_MonstroTearSpray")?.length / 0.65f ?? 0f;
            }

            public override void OnStateUpdate()
            {
                  monstroFSMRPC.FSMRPC_SoryBy();

                  if (monster.IsTearTiming && monstroFSMRPC.curSprayCount > 0) {
                        monster.IsTearTiming = false;
                        //curSprayCount--;
                        monstroFSMRPC.FSMRPC_SetCurSprayCount(monstroFSMRPC.curSprayCount--);
                        TearSpray();
                        DelaySpawnBlood();
                  }

                  //elapsedAnimationTime += Time.deltaTime;
                  // 애니메이션이 끝나면 다시 재생
                  if (monstroFSMRPC.elapsedAnimationTime >= animationLength 
                        || monstroFSMRPC.curSprayCount == monstroFSMRPC.sprayCount) {
                        // 눈물 분사 횟수 모두 소진되면 상태 변경
                        if (monstroFSMRPC.curSprayCount == 0) {
                              monster.IsTearSpray = false;
                        }
                        else {
                              //elapsedAnimationTime = 0f;
                              monstroFSMRPC.FSMRPC_SetElapsedTime(0);
                              if (monstroFSMRPC.curSprayCount == monstroFSMRPC.sprayCount) {
                                    animator.SetBool("TearSpray", true);
                              }
                              else {
                                    //animator.Play("AM_MonstroTearSpray", 0, 0f); // 0프레임부터 다시 재생
                                    monstroFSMRPC.FSMRPC_AnimatorPlay("AM_MonstroTearSpray", 0);
                              }
                              SpriteXToTarget(monstroFSMRPC.player.transform);
                        }
                  }

                  OnCollisionEnter2D();
            }

            public override void OnStateExit()
            {
                  if (photonView.IsMine) {
                        animator.SetBool("TearSpray", false);
                  }

                  monstroFSMRPC.sprayCount = 0;
                  monstroFSMRPC.curSprayCount = 0;
                  monstroFSMRPC.elapsedAnimationTime = 0;
            }

            private void TearSpray()
            {
                  int tearCount = UnityEngine.Random.Range(10, 16);
                  //for (int i = 0; i < tearCount; i++) {
                  //      AttackUsingTear(GameManager.Instance.monsterTearFactory.GetTear(tearType, true));
                  //}
                  monstroFSMRPC.FSMRPC_GetTearAndAttack(tearType, tearCount, true);
            }
      }

      public class DeadState : MonstroState
      {
            public DeadState(Monstro _monster) : base(_monster) { }

            private float explosionAnimationLength;
            private float deadAnimationLength;

            public override void OnStateEnter()
            {
                  base.OnStateEnter();
                  //deadEffectAnimators = monster.GetComponentsInChildren<Animator>(true)
                  //      .Where(anim => anim.gameObject != monster.gameObject).ToArray();
                  monstroFSMRPC.deadEffectAnimators = null;
                  monstroFSMRPC.FSMRPC_SetDeadAnimators();

                  explosionAnimationLength = monstroFSMRPC.deadEffectAnimators[0].runtimeAnimatorController.animationClips
                        .FirstOrDefault(clip => clip.name == "AM_BloodExplosion")?.length ?? 0f;
                  deadAnimationLength = animator.runtimeAnimatorController.animationClips
                              .FirstOrDefault(clip => clip.name == "AM_MonstroDead")?.length ?? 0f;

                  if (photonView.IsMine) {
                        for (int i = 0; i < monstroFSMRPC.deadEffectAnimators.Length; i++) {
                              DelaySetTrigger(monstroFSMRPC.deadEffectAnimators[i], "Dead",
                                    deadAnimationLength * (i / (float)monstroFSMRPC.deadEffectAnimators.Length));
                              // Debug.Log(i + " / " + deadAnimationLength * (i / deadEffectAnimators.Length));
                        }
                        for (int i = 0; i < 5; i++) {
                              DelaySpawnBlood(deadAnimationLength * (i / 5f));
                        }
                        monstroFSMRPC.FSMRPC_SetTrigger("Dead");
                  }

                  //animator.SetTrigger("Dead");
            }

            public override void OnStateUpdate()
            {
                  if (monster.IsDeadFinish) {
                        monster.IsDeadFinish = false;
                        ControlExplosionEffect();
                        for (int i = 0; i < 3; i++) DelaySpawnBlood(0.1f);
                        //monster.gameObject.layer = LayerMask.NameToLayer("Destroyed");
                        //monster.gameObject.SetActive(false);
                        monstroFSMRPC.FSMRPC_AfterIsDeadFinish();
                  }
            }

            public override void OnStateExit()
            {
                  // 
            }

            private async void DelaySetTrigger(Animator anim, string name, float time = 1)
            {
                  await Task.Delay((int)(1000 * time));

                  anim.SetTrigger(name);
                  //monstroFSMRPC.FSMRPC_SetTrigger(name);
            }

            private void ControlExplosionEffect()
            {
                  //deadEffectAnimators[0].SetTrigger("Finish");
                  //for (int i = 1; i < deadEffectAnimators.Length; i++) {
                  //      deadEffectAnimators[i].Play("New State", 0, 0);
                  //}

                  //DelaySetParent(deadEffectAnimators[0].transform, monster.transform, explosionAnimationLength);
                  monstroFSMRPC.FSMRPC_ControlExplosionEffect(explosionAnimationLength);
            }

            #region Disuse due to PunRPC
            private async void DelaySetParent(Transform target, Transform parent, float time = 1)
            {
                  target.parent = null;

                  await Task.Delay((int)(1000 * time));

                  target.parent = parent;
            }
            #endregion
      }
}
