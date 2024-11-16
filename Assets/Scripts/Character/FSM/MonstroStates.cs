using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

namespace MonstroStates
{
      public abstract class MonstroState : BaseState<Monstro>
      {
            protected Rigidbody2D rigid;
            protected Animator animator;
            protected SpriteRenderer spriteRenderer;

            protected Transform shadow;

            public MonstroState(Monstro _monster) : base(_monster) { }

            public override void OnStateEnter()
            {
                  rigid = monster.GetComponent<Rigidbody2D>();
                  animator = monster.GetComponent<Animator>();
                  spriteRenderer = monster.GetComponent<SpriteRenderer>();

                  shadow = monster.transform.GetChild(0);
            }

            public override void OnStateUpdate()
            {
                  if (!rigid || !animator || !spriteRenderer) {
                        rigid = monster.GetComponent<Rigidbody2D>();
                        animator = monster.GetComponent<Animator>();
                        spriteRenderer = monster.GetComponent<SpriteRenderer>();
                  }
            }

            protected virtual void OnCollisionEnter2D()
            {
                  if (monster.player.IsHurt) return;

                  if (monster.collisionRectangle == default) monster.collisionRectangle = new Vector2(1.75f, 1.25f);
                  if (Physics2D.BoxCast(rigid.position, monster.collisionRectangle, 0, Vector2.zero,
                        LayerMask.GetMask("Player"))) {
                        monster.player.health -= monster.stat.attackDamage;
                        monster.player.IsHurt = true;
                  }
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

            public override void OnStateEnter()
            {
                  base.OnStateEnter();
                  animator.SetTrigger("SmallJump");
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

      public class BigJumpState : MonstroState
      {
            public BigJumpState(Monstro _monster) : base(_monster) { }

            private Vector2 playerLatePosition = default;

            private Vector2 jumpUpPosition;
            private Vector2 shadowOffset;
            private float jumpDownSpeed = 25f;

            private float jumpDownDelay = 3f;
            private float curJumpDownDelayTime;

            public override void OnStateEnter()
            {
                  base.OnStateEnter();

                  jumpUpPosition = rigid.position + Vector2.up * 15;
                  shadowOffset = rigid.position - (Vector2)shadow.position;
                  shadow.parent = null;

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
                        if (monster.isOnLand) OnCollisionEnter2D();
                  }
            }

            public override void OnStateExit()
            {
                  shadow.parent = monster.transform;
                  shadow.localPosition = shadowOffset;
                  monster.isOnLand = false;
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
                  rigid.position = Vector2.MoveTowards(rigid.position, (Vector2)shadow.position + shadowOffset,
                        jumpDownSpeed * Time.deltaTime);
            }
      }

      public class TearSprayState : MonstroState
      {
            public TearSprayState(Monstro _monster) : base(_monster) { }

            public override void OnStateEnter()
            {
                  base.OnStateEnter();
                  animator.SetTrigger("TearSpray");
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
