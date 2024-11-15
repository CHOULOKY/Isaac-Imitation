using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

namespace PooterStates
{
      public abstract class PooterState : BaseState<Pooter>
      {
            protected Rigidbody2D rigid;
            protected Animator animator;
            protected SpriteRenderer spriteRenderer;

            public PooterState(Pooter _monster) : base(_monster) { }

            public override void OnStateEnter()
            {
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
                        if (isStateExit) return;
                        await Task.Delay(1000); // 1 second
                  }

                  SetInputVec();
                  SetInputVec(_time);
            }

            private void SetInputVec()
            {
                  monster.inputVec = new Vector2(UnityEngine.Random.Range(-1, 2), UnityEngine.Random.Range(-1, 2));

                  SetSpriteDirection();
            }

            private void SetSpriteDirection()
            {
                  if (monster.inputVec.x > 0) {
                        spriteRenderer.flipX = false;
                  }
                  else if (monster.inputVec.x < 0) {
                        spriteRenderer.flipX = true;
                  }
            }

            private void MoveMonster()
            {
                  rigid.AddForce(monster.inputVec.normalized * monster.stat.moveForce, ForceMode2D.Force);
                  if (rigid.velocity.magnitude > monster.stat.maxVelocity) {
                        rigid.velocity = rigid.velocity.normalized * monster.stat.maxVelocity;
                  }
            }
      }

      public class AttackState : PooterState, TearShooter
      {
            public AttackState(Pooter _monster) : base(_monster) { }

            public override void OnStateEnter()
            {
                  base.OnStateEnter();

                  if (monster.playerHit is RaycastHit2D playerHit) {
                        float direction = playerHit.point.x - rigid.position.x;
                        if (Mathf.Sign(direction) > 0) spriteRenderer.flipX = false;
                        else spriteRenderer.flipX = true;

                        AttackPlayer();
                  }
                  else {
                        Debug.LogWarning($"{monster.name}: AttackState에서 monster.playerHit를 찾지 못했습니다.");
                        // (애니메이션 이름, 현재 활성화 중인 레이어, 0~1까지로 0.5는 절반)
                        animator.Play("AM_PooterAttack", -1, 0.9f); // AM_PooterAttack 마지막에는 상태를 바꾸는 이벤트 존재
                  }
            }

            public override void OnStateUpdate()
            {
                  // 
            }

            public override void OnStateExit()
            {
                  // 
            }

            private void AttackPlayer()
            {
                  animator.SetTrigger("Attack");

                  //RaycastHit2D playerHit = monster.OnSenseForward(0.45f, "Player");
                  //if (playerHit && !monster.isAttack) {
                  //      monster.isAttack = true;

                  //      if (playerHit.transform.TryGetComponent<IsaacBody>(out var player)) {
                  //            if (!player.IsHurt) {
                  //                  player.health -= monster.stat.attackDamage;
                  //                  player.IsHurt = true;
                  //            }
                  //      }
                  //}
            }


            //private IsaacBody body;

            //private Animator animator;
            //private SpriteRenderer spriteRenderer;

            //private Vector2 inputVec;

            //private TearFactory.Tears tearType = TearFactory.Tears.Basic;
            //[Tooltip("= tearRange")] public float tearSpeed = 6;
            //private int tearWhatEye = 1;

            //public float attackSpeed = 0.25f;
            //private float curAttackTime = 0.25f;

            //private void Update()
            //{
            //      if (body.IsHurt) return;

            //      GetInputVec();

            //      SetHeadDirection();

            //      AttackUsingTear();
            //}

            public void AttackUsingTear()
            {
                  //curAttackTime += Time.deltaTime;
                  //if (curAttackTime > attackSpeed) {
                  //      if (Input.GetButton("Horizontal Arrow") || Input.GetButton("Vertical Arrow")) {
                  //            curAttackTime = 0;

                  //            GameObject curTear = GameManager.Instance.isaacTearFactory.GetTear(tearType, false);
                  //            SetTearPositionAndGravityTime(curTear, out float x, out float y);
                  //            SetTearVelocity(out Vector2 tearVelocity);
                  //            ShootSettedTear(curTear, tearVelocity, x, y);
                  //      }
                  //}
            }

            public void SetTearPositionAndGravityTime(GameObject curTear, out float x, out float y)
            {
                  //if (curTear.GetComponent<IsaacTear>() is IsaacTear tear) {
                  //      tearWhatEye *= -1;
                  //      // Up: 0, Down: 1, Right: 2, Left: 3
                  //      if (inputVec.x == 1) {
                  //            x = 0.3f;
                  //            y = 0.2f * tearWhatEye;
                  //            tear.tearDirection = 2;
                  //      }
                  //      else if (inputVec.x == -1) {
                  //            x = -0.3f;
                  //            y = 0.2f * tearWhatEye;
                  //            tear.tearDirection = 3;
                  //      }
                  //      else if (inputVec.y == 1) {
                  //            x = 0.2f * tearWhatEye;
                  //            y = 0.3f;
                  //            tear.tearDirection = 0;
                  //      }
                  //      else {
                  //            // inputVec.y == -1
                  //            x = 0.2f * tearWhatEye;
                  //            y = -0.3f;
                  //            tear.tearDirection = 1;
                  //      }
                  //}
                  //else {
                  //      x = y = default;
                  //}
                  x = y = default;
            }

            public void SetTearVelocity(out Vector2 tearVelocity)
            {
                  tearVelocity = Vector2.zero;

                  //if (body.GetComponent<Rigidbody2D>() is Rigidbody2D bodyRigid) {
                  //      tearVelocity.x = body.inputVec.x == -inputVec.x ? bodyRigid.velocity.x * 0.25f : bodyRigid.velocity.x * 0.5f;
                  //      tearVelocity.y = body.inputVec.y == -inputVec.y ? bodyRigid.velocity.y * 0.25f : bodyRigid.velocity.y * 0.5f;
                  //}
            }

            public void ShootSettedTear(GameObject curTear, Vector2 tearVelocity, float x, float y)
            {
                  //if (curTear.GetComponent<Rigidbody2D>() is Rigidbody2D tearRigid) {
                  //      curTear.SetActive(true);
                  //      tearRigid.position = (Vector2)this.transform.position + new Vector2(x, y);
                  //      tearRigid.velocity = Vector2.zero;

                  //      float adjustedSpeed = inputVec.y < 0 ? tearSpeed * 0.75f : tearSpeed;
                  //      // tearRigid.AddForce(inputVec * tearSpeed + Vector2.up / 2 + tearVelocity, ForceMode2D.Impulse);
                  //      tearRigid.AddForce(inputVec * adjustedSpeed + tearVelocity, ForceMode2D.Impulse);
                  //}
            }
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
