using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

namespace MonstroStates
{
      public abstract class MonstroState : BaseState<Monstro>
      {
            protected Rigidbody2D rigid;
            protected Animator animator;
            protected SpriteRenderer spriteRenderer;
            protected SpriteRenderer playerRenderer;

            protected Transform shadow;
            protected Collider2D collider;

            public MonstroState(Monstro _monster) : base(_monster) { }

            public override void OnStateEnter()
            {
                  rigid = monster.GetComponent<Rigidbody2D>();
                  animator = monster.GetComponent<Animator>();
                  spriteRenderer = monster.GetComponent<SpriteRenderer>();
                  playerRenderer = monster.player.GetComponent<SpriteRenderer>();

                  shadow = monster.transform.GetChild(0);
                  collider = shadow.GetComponent<Collider2D>();
            }
            
            public override void OnStateUpdate()
            {
                  if (!rigid || !animator || !spriteRenderer || !shadow || !collider) {
                        rigid = monster.GetComponent<Rigidbody2D>();
                        animator = monster.GetComponent<Animator>();
                        spriteRenderer = monster.GetComponent<SpriteRenderer>();

                        shadow = monster.transform.GetChild(0);
                        collider = shadow.GetComponent<Collider2D>();
                  }
            }

            protected virtual void OnCollisionEnter2D()
            {
                  if (monster.player.IsHurt) return;

                  if (monster.collisionRectangle == default) monster.collisionRectangle = new Vector2(2.2f, 0.75f);
                  if (Physics2D.BoxCast(rigid.position, monster.collisionRectangle, 0, Vector2.zero, 0, 
                        LayerMask.GetMask("Player"))) {
                        // Debug.Log("Player is on Monster collision!");
                        monster.player.health -= monster.stat.attackDamage;
                        monster.player.IsHurt = true;
                  }
            }

            // Exclude Layers에 레이어가 존재하는지 확인하는 함수
            protected virtual bool IsLayerExcluded(int layer)
            {
                  // 비트 연산으로 레이어가 포함되어 있는지 확인
                  return (collider.excludeLayers & (1 << layer)) != 0;
            }

            // Exclude Layers에 레이어를 추가하는 함수
            protected virtual void AddExcludeLayerToCollider(LayerMask layerToAdd)
            {
                  // 현재 excludeLayers에 layerToAdd를 추가
                  collider.excludeLayers |= layerToAdd;
            }

            // Exclude Layers에 레이어를 제거하는 함수
            protected virtual void RemoveExcludeLayerFromCollider(LayerMask layerToRemove)
            {
                  // 현재 excludeLayers에서 layerToRemove를 제거
                  collider.excludeLayers &= ~layerToRemove;
            }
      }

      public class IdleState : MonstroState
      {
            public IdleState(Monstro _monster) : base(_monster) { }

            public override void OnStateEnter()
            {
                  base.OnStateEnter();

                  monster.player = GetPlayerObject();
            }

            public override void OnStateUpdate()
            {
                  base.OnStateUpdate();

                  if (monster.player == null) monster.player = GetPlayerObject();
                  else if (playerRenderer == null) playerRenderer = monster.player.GetComponent<SpriteRenderer>();
                  else monster.sortRendererBy.SortBy(spriteRenderer, playerRenderer, false);

                  OnCollisionEnter2D();
            }

            public override void OnStateExit()
            {
                  animator.SetTrigger("Awake");
            }

            private IsaacBody GetPlayerObject()
            {
                  if (monster.playerSearchBox == default) monster.playerSearchBox = Vector2.one * 40;
                  if (Physics2D.BoxCast(rigid.position, monster.playerSearchBox, 0, Vector2.zero, 0,
                      LayerMask.GetMask("Player")) is RaycastHit2D _player) {
                        return _player.transform.GetComponent<IsaacBody>();
                  }
                  else {
                        return null;
                  }
            }
      }

      public class SmallJumpState : MonstroState
      {
            public SmallJumpState(Monstro _monster) : base(_monster) { }

            private int curjumpCount;
            private int maxJumpCount;

            private Vector2 shadowOffset;
            private Vector2 nextPosition;

            private float animationLength;
            private float elapsedAnimationTime;

            public override void OnStateEnter()
            {
                  base.OnStateEnter();

                  // 점프 횟수 초기 설정
                  switch (UnityEngine.Random.Range(0, 5)) {
                        case 0: maxJumpCount = 1; break;
                        case 1: case 2: maxJumpCount = 3; break;
                        case 3: case 4: maxJumpCount = 5; break;
                  }
                  curjumpCount = maxJumpCount;

                  shadowOffset = shadow.localPosition;
                  shadow.parent = null;

                  // 애니메이션의 길이 가져오기
                  animationLength = animator.runtimeAnimatorController.animationClips
                      .FirstOrDefault(clip => clip.name == "AM_MonstroSmallJump")?.length ?? 0f;

                  animator.SetBool("SmallJump", true);
            }

            public override void OnStateUpdate()
            {
                  SetBeforeNextJump();
                  MoveShadow();
                  JumpToShadow();

                  if (monster.isOnLand) {
                        OnCollisionEnter2D();
                        if (IsLayerExcluded(LayerMask.NameToLayer("Tear"))) {
                              RemoveExcludeLayerFromCollider(LayerMask.NameToLayer("Tear"));
                        }
                  }

                  monster.sortRendererBy.SortBy(spriteRenderer, playerRenderer, false);
            }

            public override void OnStateExit()
            {
                  monster.isOnLand = false;

                  shadow.parent = monster.transform;
                  shadow.localPosition = shadowOffset;
                  RemoveExcludeLayerFromCollider(LayerMask.NameToLayer("Tear"));

                  animator.SetBool("SmallJump", false);
            }

            private void SetBeforeNextJump()
            {
                  elapsedAnimationTime += Time.deltaTime;
                  // 애니메이션이 끝나면 다시 재생
                  if (elapsedAnimationTime >= animationLength || curjumpCount == maxJumpCount) {
                        if (curjumpCount > 0) {
                              animator.Play("AM_MonstroSmallJump", 0, 0f); // 0프레임부터 재생
                              elapsedAnimationTime = 0f;

                              // 다음 위치로 이동하기 위한 설정
                              nextPosition = GetNextPosition(2f);

                              curjumpCount--;

                              monster.isOnLand = false;
                              AddExcludeLayerToCollider(LayerMask.NameToLayer("Tear"));
                        }
                        else {
                              // 점프 횟수 모두 소진되면 상태 변경
                              monster.isSmallJump = false;
                        }
                  }
            }

            private Vector2 GetNextPosition(float distance)
            {
                  Vector3 nextDirection = monster.player.transform.position - shadow.position;
                  Vector3 nextPosition = nextDirection.normalized * distance;
                  return shadow.position + nextPosition;
            }

            private void MoveShadow()
            {
                  // 그림자 등속 이동
                  shadow.position = Vector2.Lerp(shadow.position, nextPosition, elapsedAnimationTime / animationLength);
            }

            private void JumpToShadow()
            {
                  // 몬스터 등속 이동
                  if (elapsedAnimationTime < animationLength / 2) {
                        rigid.position = Vector2.Lerp(rigid.position, nextPosition + Vector2.up * 3f, 
                              elapsedAnimationTime / (animationLength / 2));
                  }
                  else {
                        rigid.position = Vector2.Lerp(rigid.position, nextPosition + Vector2.up * -shadowOffset, 
                              elapsedAnimationTime / animationLength);
                  }
            }
      }

      public class BigJumpState : MonstroState
      {
            public BigJumpState(Monstro _monster) : base(_monster) { }

            private Vector2 playerLatePosition = default;

            private Vector2 jumpUpPosition;
            private Vector2 shadowOffset;
            private float jumpDownSpeed = 25f;

            private float jumpDownDelay = 3f;
            private float curJumpDownDelayTime;

            private float animationLength;
            private float elapsedAnimationTime;

            public override void OnStateEnter()
            {
                  base.OnStateEnter();

                  jumpUpPosition = rigid.position + Vector2.up * 15;
                  shadowOffset = shadow.localPosition;
                  shadow.parent = null;

                  animationLength = animator.runtimeAnimatorController.animationClips
                      .FirstOrDefault(clip => clip.name == "AM_MonstroBigJumpDown")?.length ?? 0f;

                  animator.SetTrigger("BigJumpUp");
            }

            public override void OnStateUpdate()
            {
                  if (monster.isJumpUp) {
                        // 몬스터 화면 밖으로 점프
                        rigid.position = Vector2.Lerp(rigid.position, jumpUpPosition, 0.01f);

                        curJumpDownDelayTime += Time.deltaTime;

                        MoveShadow();

                        // 착지 트리거
                        LandTrigger();
                  }
                  else {
                        LandOnShadow();
                        if (monster.isOnLand) {
                              OnCollisionEnter2D();
                              if (IsLayerExcluded(LayerMask.NameToLayer("Tear"))) {
                                    RemoveExcludeLayerFromCollider(LayerMask.NameToLayer("Tear"));
                              }

                              elapsedAnimationTime += Time.deltaTime;
                              // AM_MonstroBigJumpDown 애니메이션이 끝나면 상태 변경
                              if (elapsedAnimationTime >= animationLength) {
                                    monster.isSmallJump = false;
                              }
                        }
                  }

                  monster.sortRendererBy.SortBy(spriteRenderer, playerRenderer, false);
            }

            public override void OnStateExit()
            {
                  monster.isOnLand = false;

                  shadow.parent = monster.transform;
                  shadow.localPosition = shadowOffset;
                  AddExcludeLayerToCollider(LayerMask.NameToLayer("Tear"));

                  animator.SetTrigger("BigJumpUp");
            }

            private void MoveShadow()
            {
                  Vector2 nextPosition = monster.player.transform.position;

                  // 플레이어의 과거 위치 저장
                  if (curJumpDownDelayTime > jumpDownDelay / 2 && playerLatePosition == default) {
                        playerLatePosition = nextPosition;
                  }

                  // 그림자 보간 이동
                  float t = Mathf.Clamp01(curJumpDownDelayTime / jumpDownDelay);
                  shadow.position =
                        Vector2.Lerp(shadow.position, playerLatePosition == default ? nextPosition : playerLatePosition, t);
            }

            private void LandTrigger()
            {
                  if (curJumpDownDelayTime > jumpDownDelay) {
                        rigid.position = new Vector2(shadow.position.x, rigid.position.y); // 그림자와 X값만 일치
                        monster.isJumpUp = false;
                        animator.SetTrigger("BigJumpDown");
                  }
            }

            private void LandOnShadow()
            {
                  // 몬스터 그림자 위로 착지
                  rigid.position = Vector2.MoveTowards(rigid.position, (Vector2)shadow.position - shadowOffset,
                        jumpDownSpeed * Time.deltaTime);
            }

            // 눈물 분사 추가
      }

      public class TearSprayState : MonstroState
      {
            public TearSprayState(Monstro _monster) : base(_monster) { }

            private const TearFactory.Tears tearType = TearFactory.Tears.Boss;

            private Vector2 directionVec = Vector2.zero;

            public override void OnStateEnter()
            {
                  base.OnStateEnter();

                  animator.SetBool("TearSpray", true);
                  // Debug.Log("TearSpray");
            }

            public override void OnStateUpdate()
            {
                  monster.sortRendererBy.SortBy(spriteRenderer, playerRenderer, false);

                  // animator.Play("AM_MonstroTearSpary", 0, 0f); // 0프레임부터 재생
                  // GameManager.Instance.monsterTearFactory.GetTear(tearType, true);

                  OnCollisionEnter2D();
            }

            public override void OnStateExit()
            {
                  // monster.isTearSpray = false;

                  animator.SetBool("TearSpray", false);
            }
      }

      public class DeadState : MonstroState
      {
            public DeadState(Monstro _monster) : base(_monster) { }

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
