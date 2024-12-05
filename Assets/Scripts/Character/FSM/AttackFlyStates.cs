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
        private float moveSpeedModifier = 1.0f; // 이동 속도 변경 계수
        private bool isPlayerHit = false; // 플레이어 피격 상태
        private float speedVariationTime = 0.35f; // 속도 변화 간격 (초)
        private float speedVariationTimer;

        public MoveState(AttackFly _monster) : base(_monster) { }

        public override void OnStateEnter()
        {
            base.OnStateEnter();

            moveForce = monster.stat.moveForce;
            maxVelocity = monster.stat.maxVelocity;
            speedVariationTimer = speedVariationTime;

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
                // 속도 변화 로직
                UpdateSpeedVariation();

                // 플레이어 추적
                Vector2 direction = ((Vector2)player.position - (Vector2)monster.transform.position).normalized;
                rigid.AddForce(direction * moveForce * moveSpeedModifier, ForceMode2D.Force);

                if (rigid.velocity.magnitude > maxVelocity)
                {
                    rigid.velocity = rigid.velocity.normalized * maxVelocity;
                }

                // 충돌 감지
                CheckCollisionWithPlayer();
            }
            else
            {
                rigid.velocity = Vector2.zero;
            }
        }

        private void UpdateSpeedVariation()
        {
            speedVariationTimer -= Time.deltaTime;
            if (speedVariationTimer <= 0)
            {
                // 속도 변동 폭을 크게 설정
                moveSpeedModifier = UnityEngine.Random.Range(0.7f, 5.0f); // 속도 변화 범위
                speedVariationTimer = speedVariationTime; // 타이머 초기화
            }
        }

        private void CheckCollisionWithPlayer()
        {
            if (isPlayerHit) return; // 이미 처리 중이면 중복 처리 방지

            Collider2D hit = Physics2D.OverlapCircle(monster.transform.position, collisionRadius, LayerMask.GetMask("Player"));
            if (hit != null)
            {
                IsaacBody playerBody = hit.GetComponent<IsaacBody>();
                if (playerBody != null && !playerBody.IsHurt) // 피격 상태 확인
                {
                    isPlayerHit = true; // 피격 처리 시작
                    playerBody.IsHurt = true; // 플레이어 무적 상태 활성화
                    playerBody.health -= monster.stat.attackDamage; // 플레이어 체력 감소

                    Debug.Log($"AttackFly hit the player! Remaining health: {playerBody.health}");

                    // 무적 상태 해제 코루틴 호출
                    monster.StartCoroutine(ResetPlayerHitCooldown(playerBody));
                }
            }
        }

        private System.Collections.IEnumerator ResetPlayerHitCooldown(IsaacBody playerBody)
        {
            yield return new WaitForSeconds(1.0f); // 1초 무적 상태
            playerBody.IsHurt = false; // 무적 해제
            isPlayerHit = false; // 피격 상태 해제
        }

        public override void OnStateExit()
        {
            base.OnStateExit();
            rigid.velocity = Vector2.zero;
            isPlayerHit = false; // 상태 초기화
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
