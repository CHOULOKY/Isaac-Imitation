using System.Collections.Generic;
using UnityEngine;

namespace AttackFlyStates
{
    public abstract class AttackFlyState : BaseState<AttackFly>
    {
        protected Rigidbody2D rigid;
        protected Animator animator;

        public AttackFlyState(AttackFly _monster) : base(_monster) { }

        public override void OnStateEnter()
        {
            rigid = monster.GetComponent<Rigidbody2D>();
            animator = monster.GetComponent<Animator>();
        }

        public override void OnStateUpdate()
        {
            if (!rigid)
            {
                rigid = monster.GetComponent<Rigidbody2D>();
            }
        }

        public override void OnStateExit() { }
    }

    public class IdleState : AttackFlyState
    {
        public IdleState(AttackFly _monster) : base(_monster) { }

        public override void OnStateEnter()
        {
            base.OnStateEnter();
            rigid.velocity = Vector2.zero; // 멈춤 상태
        }

        public override void OnStateUpdate()
        {
            base.OnStateUpdate();
        }

        public override void OnStateExit()
        {
            base.OnStateExit();
        }
    }

    public class MoveState : AttackFlyState
    {
        private Transform player;
        private float moveForce;
        private float maxVelocity;
        private float collisionRadius = 0.5f; // 충돌 감지 반경

        public MoveState(AttackFly _monster) : base(_monster) { }

        public override void OnStateEnter()
        {
            base.OnStateEnter();

            moveForce = monster.stat.moveForce;
            maxVelocity = monster.stat.maxVelocity;

            // 플레이어 찾기
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

        public override void OnStateUpdate()
        {
            base.OnStateUpdate();

            if (player != null)
            {
                // 플레이어 추적
                Vector2 direction = ((Vector2)player.position - (Vector2)monster.transform.position).normalized;
                rigid.AddForce(direction * moveForce, ForceMode2D.Force);

                if (rigid.velocity.magnitude > maxVelocity)
                {
                    rigid.velocity = rigid.velocity.normalized * maxVelocity;
                }

                // 충돌 감지
                CheckCollisionWithPlayer();
            }
            else
            {
                // 플레이어가 없으면 정지
                rigid.velocity = Vector2.zero;
            }
        }

        private void CheckCollisionWithPlayer()
        {
            Collider2D hit = Physics2D.OverlapCircle(monster.transform.position, collisionRadius, LayerMask.GetMask("Player"));
            if (hit != null)
            {
                IsaacBody playerBody = hit.GetComponent<IsaacBody>();
                if (playerBody != null && !playerBody.IsHurt)
                {
                    playerBody.IsHurt = true;
                    playerBody.health -= monster.stat.attackDamage; // 플레이어 체력 감소
                    Debug.Log($"AttackFly hit the player! Player's remaining health: {playerBody.health}");
                }
            }
        }

        public override void OnStateExit()
        {
            base.OnStateExit();
            rigid.velocity = Vector2.zero;
        }
    }

    public class DeadState : AttackFlyState
    {
        public DeadState(AttackFly _monster) : base(_monster) { }

        public override void OnStateEnter()
        {
            base.OnStateEnter();
            // 사망 처리 로직 추가
            monster.gameObject.SetActive(false); // 임시로 비활성화
        }

        public override void OnStateUpdate()
        {
            base.OnStateUpdate();
        }

        public override void OnStateExit()
        {
            base.OnStateExit();
        }
    }
}

