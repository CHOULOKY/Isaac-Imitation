using Photon.Pun;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace MonstroStates
{
      public abstract class MonstroState : BaseState<Monstro>, ITearShooter
      {
            protected PhotonView photonView;

            protected Rigidbody2D rigid;
            protected Collider2D monsterCollider;
            protected Animator animator;
            protected SpriteRenderer spriteRenderer;
            protected SpriteRenderer playerRenderer;

            protected Transform shadow;
            protected Collider2D shadowCollider;

            protected const TearFactory.Tears tearType = TearFactory.Tears.Boss;

            public MonstroState(Monstro _monster) : base(_monster) { }

            public override void OnStateEnter()
            {
                  photonView = monster.GetComponent<PhotonView>();

                  rigid = monster.GetComponent<Rigidbody2D>();
                  monsterCollider = monster.GetComponent<Collider2D>();
                  animator = monster.GetComponent<Animator>();
                  spriteRenderer = monster.GetComponent<SpriteRenderer>();
                  playerRenderer = monster.player.GetComponent<SpriteRenderer>();

                  foreach (Transform child in monster.GetComponentsInChildren<Transform>()) {
                        if (child.name == "Shadow") {
                              shadow = child;
                              shadowCollider = shadow.GetComponent<Collider2D>();
                              break;
                        }
                  }
            }

            protected virtual void SpriteXToTarget(Transform target)
            {
                  directionVec = (Vector2)target.position - rigid.position;
                  if (Mathf.Sign(directionVec.x) > 0) spriteRenderer.flipX = true;
                  else spriteRenderer.flipX = false;
            }

            protected virtual void OnCollisionEnter2D()
            {
                  if (monster.player.IsHurt) return;

                  if (Physics2D.BoxCast(monsterCollider.bounds.center, monsterCollider.bounds.size, 0, Vector2.zero, 0,
                        LayerMask.GetMask("Player"))) {
                        // Debug.Log("Player is on Monster collision!");
                        monster.player.Health -= monster.stat.attackDamage;
                        monster.player.IsHurt = true;
                        GameManager.Instance.uiManager.setKilledPlayer = "Monstro";
                  }
            }

            #region Check Exclude Layers
            // Exclude Layers에 레이어가 존재하는지 확인하는 함수
            protected virtual bool IsLayerExcluded(Collider2D collider, int layer)
            {
                  // 비트 연산으로 레이어가 포함되어 있는지 확인
                  return (collider.excludeLayers & (1 << layer)) != 0;
            }

            // Exclude Layers에 레이어를 추가하는 함수
            protected virtual void AddExcludeLayerToCollider(Collider2D collider, int layer)
            {
                  // 현재 excludeLayers에 layerToAdd를 추가
                  collider.excludeLayers |= (1 << layer);
            }

            // Exclude Layers에 레이어를 제거하는 함수
            protected virtual void RemoveExcludeLayerFromCollider(Collider2D collider, int layer)
            {
                  // 현재 excludeLayers에서 layerToRemove를 제거
                  collider.excludeLayers &= ~(1 << layer);
            }
            #endregion

            #region Tear
            protected Vector2 directionVec;

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
                        if (directionVec.x > 0) {
                              tear.tearDirection = 2;
                        }
                        else if (directionVec.x < 0) {
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

                  Vector2 inputVec = RotateVector(directionVec.normalized, rotateAngle);
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

                  monster.SpawnBloodEffects();
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
                  if (!rigid || !monsterCollider || !animator || !spriteRenderer) {
                        rigid = monster.GetComponent<Rigidbody2D>();
                        monsterCollider = monster.GetComponent<Collider2D>();
                        animator = monster.GetComponent<Animator>();
                        spriteRenderer = monster.GetComponent<SpriteRenderer>();
                        return;
                  }

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
                        case 1: case 2: case 3: maxJumpCount = 3; break;
                        case 4: maxJumpCount = 5; break;
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
                        if (IsLayerExcluded(monsterCollider, LayerMask.NameToLayer("Tear")) ||
                              IsLayerExcluded(shadowCollider, LayerMask.NameToLayer("Player"))) {
                              RemoveExcludeLayerFromCollider(monsterCollider, LayerMask.NameToLayer("Tear"));
                              RemoveExcludeLayerFromCollider(shadowCollider, LayerMask.NameToLayer("Player"));
                        }
                  }

                  monster.sortRendererBy.SortBy(spriteRenderer, playerRenderer, false);
            }

            public override void OnStateExit()
            {
                  monster.isOnLand = false;

                  shadow.parent = monster.transform;
                  shadow.localPosition = shadowOffset;
                  if (IsLayerExcluded(monsterCollider, LayerMask.NameToLayer("Tear")) ||
                        IsLayerExcluded(shadowCollider, LayerMask.NameToLayer("Player"))) {
                        RemoveExcludeLayerFromCollider(monsterCollider, LayerMask.NameToLayer("Tear"));
                        RemoveExcludeLayerFromCollider(shadowCollider, LayerMask.NameToLayer("Player"));
                  }

                  animator.SetBool("SmallJump", false);
            }

            private void SetBeforeNextJump()
            {
                  elapsedAnimationTime += Time.deltaTime;
                  // 애니메이션이 끝나면 다시 재생
                  if (elapsedAnimationTime >= animationLength || curjumpCount == maxJumpCount) {
                        if (curjumpCount > 0) {
                              elapsedAnimationTime = 0f;
                              curjumpCount--;

                              animator.Play("AM_MonstroSmallJump", 0, 0f); // 0프레임부터 재생
                              SpriteXToTarget(monster.player.transform);

                              // 다음 위치로 이동하기 위한 설정
                              nextPosition = GetNextPosition(2.5f);

                              monster.isOnLand = false;
                              if (!IsLayerExcluded(monsterCollider, LayerMask.NameToLayer("Tear")) ||
                                    !IsLayerExcluded(shadowCollider, LayerMask.NameToLayer("Player"))) {
                                    AddExcludeLayerToCollider(monsterCollider, LayerMask.NameToLayer("Tear"));
                                    AddExcludeLayerToCollider(shadowCollider, LayerMask.NameToLayer("Player"));
                              }
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
            private float jumpDownSpeed = 40f;

            private float jumpDownDelay = 2f;
            private float curJumpDownDelayTime = 0;

            private bool isTearSparied = false;

            private float animationLength;
            private float elapsedAnimationTime;

            public override void OnStateEnter()
            {
                  base.OnStateEnter();

                  jumpUpPosition = rigid.position + Vector2.up * 15;
                  shadowOffset = shadow.localPosition;
                  shadow.parent = null;

                  // 애니메이션의 길이 가져오기
                  animationLength = animator.runtimeAnimatorController.animationClips
                      .FirstOrDefault(clip => clip.name == "AM_MonstroBigJumpDown")?.length / 0.65f ?? 0f;

                  animator.SetTrigger("BigJumpUp");
            }

            public override void OnStateUpdate()
            {
                  if (monster.isJumpUp) {
                        if (!IsLayerExcluded(monsterCollider, LayerMask.NameToLayer("Tear")) ||
                              !IsLayerExcluded(shadowCollider, LayerMask.NameToLayer("Player"))) {
                              AddExcludeLayerToCollider(monsterCollider, LayerMask.NameToLayer("Tear"));
                              AddExcludeLayerToCollider(shadowCollider, LayerMask.NameToLayer("Player"));
                        }

                        // 몬스터 화면 밖으로 점프
                        rigid.position = Vector2.Lerp(rigid.position, jumpUpPosition, 0.01f);

                        curJumpDownDelayTime += Time.deltaTime;
                        if (curJumpDownDelayTime > jumpDownDelay) {
                              rigid.position = new Vector2(shadow.position.x, rigid.position.y); // 그림자와 X값만 일치
                              monster.isJumpUp = false;
                              animator.SetTrigger("BigJumpDown");
                              SpriteXToTarget(monster.player.transform);
                        }
                        MoveShadow();
                  }
                  else if (curJumpDownDelayTime > jumpDownDelay) {
                        LandOnShadow();
                        if (monster.isOnLand) {
                              if (!isTearSparied) {
                                    isTearSparied = true;
                                    TearSpray();
                                    DelaySpawnBlood();
                              }
                              if (IsLayerExcluded(monsterCollider, LayerMask.NameToLayer("Tear")) ||
                                    IsLayerExcluded(shadowCollider, LayerMask.NameToLayer("Player"))) {
                                    RemoveExcludeLayerFromCollider(monsterCollider, LayerMask.NameToLayer("Tear"));
                                    RemoveExcludeLayerFromCollider(shadowCollider, LayerMask.NameToLayer("Player"));
                              }

                              OnCollisionEnter2D();

                              elapsedAnimationTime += Time.deltaTime;
                              // AM_MonstroBigJumpDown 애니메이션이 끝나면 상태 변경
                              if (elapsedAnimationTime >= animationLength) {
                                    monster.isBigJump = false;
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
                  if (IsLayerExcluded(monsterCollider, LayerMask.NameToLayer("Tear")) ||
                        IsLayerExcluded(shadowCollider, LayerMask.NameToLayer("Player"))) {
                        RemoveExcludeLayerFromCollider(monsterCollider, LayerMask.NameToLayer("Tear"));
                        RemoveExcludeLayerFromCollider(shadowCollider, LayerMask.NameToLayer("Player"));
                  }
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

            private void LandOnShadow()
            {
                  // 몬스터 그림자 위로 착지
                  rigid.position = Vector2.MoveTowards(rigid.position, (Vector2)shadow.position - shadowOffset,
                        jumpDownSpeed * Time.deltaTime);
            }

            private void TearSpray()
            {
                  int tearCount = UnityEngine.Random.Range(15, 20);
                  for (int i = 0; i < tearCount; i++) {
                        AttackUsingTear(GameManager.Instance.monsterTearFactory.GetTear(tearType, true));
                  }
            }
      }

      public class TearSprayState : MonstroState
      {
            public TearSprayState(Monstro _monster) : base(_monster) { }

            private float animationLength;
            private float elapsedAnimationTime = 0;

            private int sprayCount;
            private int curSprayCount;

            public override void OnStateEnter()
            {
                  base.OnStateEnter();

                  if (monster.player) {
                        sprayCount = UnityEngine.Random.Range(0, 5) == 0 ? 1 : 2;
                        curSprayCount = sprayCount;
                        animationLength = animator.runtimeAnimatorController.animationClips
                              .FirstOrDefault(clip => clip.name == "AM_MonstroTearSpray")?.length / 0.65f ?? 0f;
                  }
                  else {
                        Debug.LogWarning($"{monster.name}: TearSprayState에서 monster.player를 찾지 못했습니다.");
                  }
            }

            public override void OnStateUpdate()
            {
                  monster.sortRendererBy.SortBy(spriteRenderer, playerRenderer, false);

                  if (monster.isTearTiming && curSprayCount > 0) {
                        monster.isTearTiming = false;
                        curSprayCount--;
                        TearSpray();
                        DelaySpawnBlood();
                  }

                  elapsedAnimationTime += Time.deltaTime;
                  // 애니메이션이 끝나면 다시 재생
                  if (elapsedAnimationTime >= animationLength || curSprayCount == sprayCount) {
                        // 눈물 분사 횟수 모두 소진되면 상태 변경
                        if (curSprayCount == 0) {
                              monster.isTearSpray = false;
                        }
                        else {
                              elapsedAnimationTime = 0f;
                              if (curSprayCount == sprayCount) animator.SetBool("TearSpray", true);
                              else animator.Play("AM_MonstroTearSpray", 0, 0f); // 0프레임부터 다시 재생
                              SpriteXToTarget(monster.player.transform);
                        }
                  }

                  OnCollisionEnter2D();
            }

            public override void OnStateExit()
            {
                  animator.SetBool("TearSpray", false);
            }

            private void TearSpray()
            {
                  int tearCount = UnityEngine.Random.Range(10, 16);
                  for (int i = 0; i < tearCount; i++) {
                        AttackUsingTear(GameManager.Instance.monsterTearFactory.GetTear(tearType, true));
                  }
            }
      }

      public class DeadState : MonstroState
      {
            public DeadState(Monstro _monster) : base(_monster) { }

            private Animator[] deadEffectAnimators;
            private float explosionAnimationLength;

            private float deadAnimationLength;

            public override void OnStateEnter()
            {
                  base.OnStateEnter();
                  deadEffectAnimators = monster.GetComponentsInChildren<Animator>(true)
                        .Where(anim => anim.gameObject != monster.gameObject).ToArray();
                  explosionAnimationLength = deadEffectAnimators[0].runtimeAnimatorController.animationClips
                        .FirstOrDefault(clip => clip.name == "AM_BloodExplosion")?.length ?? 0f;

                  deadAnimationLength = animator.runtimeAnimatorController.animationClips
                              .FirstOrDefault(clip => clip.name == "AM_MonstroDead")?.length ?? 0f;

                  for (int i = 0; i < deadEffectAnimators.Length; i++) {
                        DelaySetTrigger(deadEffectAnimators[i], "Dead",
                              deadAnimationLength * (i / (float)deadEffectAnimators.Length));
                        // Debug.Log(i + " / " + deadAnimationLength * (i / deadEffectAnimators.Length));
                  }
                  for (int i = 0; i < 5; i++) {
                        DelaySpawnBlood(deadAnimationLength * (i / 5f));
                  }

                  animator.SetTrigger("Dead");
            }

            public override void OnStateUpdate()
            {
                  if (monster.isDeadFinish) {
                        monster.isDeadFinish = false;
                        ControlExplosionEffect();
                        for (int i = 0; i < 3; i++) DelaySpawnBlood(0.1f);
                        monster.gameObject.layer = LayerMask.NameToLayer("Destroyed");
                        monster.gameObject.SetActive(false);
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
            }

            private void ControlExplosionEffect()
            {
                  deadEffectAnimators[0].SetTrigger("Finish");
                  for (int i = 1; i < deadEffectAnimators.Length; i++) {
                        deadEffectAnimators[i].Play("New State", 0, 0);
                  }

                  DelaySetParent(deadEffectAnimators[0].transform, monster.transform, explosionAnimationLength);
            }

            private async void DelaySetParent(Transform target, Transform parent, float time = 1)
            {
                  target.parent = null;

                  await Task.Delay((int)(1000 * time));

                  target.parent = parent;
            }
      }
}
