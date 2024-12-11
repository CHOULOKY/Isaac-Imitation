using System.Threading.Tasks;
using UnityEngine;

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
    //
    public class MoveState : PooterState
    {
        public MoveState(Pooter _monster) : base(_monster) { }

        private bool isStateExit = false;
        private Vector2 spawnPosition; // ��ȯ ��ġ ����
        private float maxDistanceFromSpawn = 2f; // �̵� ���� ���

        public override void OnStateEnter()
        {
            base.OnStateEnter();
            spawnPosition = monster.transform.position; // ��ȯ ��ġ ����
            animator.Play("AM_PooterMove", -1, UnityEngine.Random.Range(0f, 1f)); // �̵� �ִϸ��̼� ����

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
            if (_time < 1)
            {
                Debug.LogError($"{monster.name}: SetInputVec ȣ�� �ð��� �߸��Ǿ����ϴ�. (0 ����)");
                return;
            }

            for (int i = 0; i < _time; ++i)
            {
                if (isStateExit) return;
                await Task.Delay(1000); // 1�� ���
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
            if (monster.inputVec.x > 0)
            {
                spriteRenderer.flipX = false;
            }
            else if (monster.inputVec.x < 0)
            {
                spriteRenderer.flipX = true;
            }
        }

        private void MoveMonster()
        {
            // �÷��̾� Ž��
            var playerHit = Physics2D.Raycast(rigid.position, Vector2.zero, 0f, LayerMask.GetMask("Player"));

            if (playerHit.collider != null)
            {
                // �÷��̾ Ž���Ǹ� �̵��� ����
                rigid.velocity = Vector2.zero;
                monster.inputVec = Vector2.zero;

                // �÷��̾� ������ ����
                monster.playerHit = playerHit;
                return;
            }

            // ���ѵ� �������� ����
            Vector2 currentPosition = rigid.position;
            if (Vector2.Distance(spawnPosition, currentPosition) > maxDistanceFromSpawn)
            {
                // ������ ����� �ϸ� ��ȯ ��ġ�� ���ƿ��� �������� �̵�
                Vector2 directionToSpawn = (spawnPosition - currentPosition).normalized;
                rigid.AddForce(directionToSpawn * monster.stat.moveForce, ForceMode2D.Force);
            }
            else
            {
                // ���� ������ ���� �̵� ����
                rigid.AddForce(monster.inputVec.normalized * monster.stat.moveForce, ForceMode2D.Force);
            }

            // �ִ� �ӵ� ����
            if (rigid.velocity.magnitude > monster.stat.maxVelocity)
            {
                rigid.velocity = rigid.velocity.normalized * monster.stat.maxVelocity;
            }
        }
    }


    public class AttackState : PooterState, ITearShooter
      {
            public AttackState(Pooter _monster) : base(_monster) { }

            private const TearFactory.Tears tearType = TearFactory.Tears.Basic;
            private GameObject firstTear, secondTear;

            private Vector2 directionVec = Vector2.zero;

            public override void OnStateEnter()
            {
                  base.OnStateEnter();

                  if (monster.playerHit is RaycastHit2D playerHit) {
                        directionVec = playerHit.point - rigid.position;

                        if (Mathf.Sign(directionVec.x) > 0) spriteRenderer.flipX = false;
                        else spriteRenderer.flipX = true;

                        animator.SetTrigger("Attack");
                  }
                  else {
                        Debug.LogWarning($"{monster.name}: AttackState���� monster.playerHit�� ã�� ���߽��ϴ�.");
                        // (�ִϸ��̼� �̸�, ���� Ȱ��ȭ ���� ���̾�, 0~1������ 0.5�� ����)
                        animator.Play("AM_PooterAttack", -1, 0.9f); // AM_PooterAttack ���������� ���¸� �ٲٴ� �̺�Ʈ ����
                  }
            }

            public override void OnStateUpdate()
            {
                  if (monster.isAttackTiming[0] && !firstTear) {
                        firstTear = GameManager.Instance.monsterTearFactory.GetTear(tearType, true);
                        AttackUsingTear(firstTear);
                  }
                  else if (monster.isAttackTiming[1] && !secondTear) {
                        secondTear = GameManager.Instance.monsterTearFactory.GetTear(tearType, true);
                        AttackUsingTear(secondTear);
                  }
            }

            public override void OnStateExit()
            {
                  monster.isAttackTiming[0] = false;
                  monster.isAttackTiming[1] = false;
            }

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
                        Vector2 offset = new Vector2(0, -0.35f);
                        // Up: 0, Down: 1, Right: 2, Left: 3
                        if (directionVec.x > 0) {
                              tear.tearDirection = 2;
                        }
                        else if (directionVec.x < 0) {
                              tear.tearDirection = 3;
                        }
                        else if (directionVec.y > 0) {
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
                  tearVelocity.x = Mathf.Clamp(directionVec.x, -1, 1);
                  tearVelocity.y = Mathf.Clamp(directionVec.y, -1, 1);

                  tearRigid.velocity = Vector2.zero;
            }

            public void ShootSettedTear(GameObject curTear, Rigidbody2D tearRigid, Vector2 tearVelocity)
            {
                  Vector2 inputVec = Mathf.Abs(directionVec.x) > Mathf.Abs(directionVec.y) ?
                        Vector2.right * Mathf.Sign(directionVec.x) : Vector2.up * Mathf.Sign(directionVec.y);
                  float adjustedSpeed = inputVec.y < 0 ? monster.stat.tearSpeed * 0.75f : monster.stat.tearSpeed;
                  tearRigid.AddForce(inputVec * adjustedSpeed + tearVelocity, ForceMode2D.Impulse);
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
