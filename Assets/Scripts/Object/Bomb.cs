using Photon.Realtime;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using ObstacleSpace;
using Photon.Pun;
using Unity.Burst.CompilerServices;

namespace ItemSpace
{
      public class Bomb : MonoBehaviour
      {
            private PhotonView photonView;

            private FlashEffect flashEffect;

            private SpriteRenderer spriteRenderer;

            private Transform[] childs;
            private Animator explosionAnimator;

            public int damage = 2;
            public int knockPower = 3;

            private void Awake()
            {
                  photonView = GetComponent<PhotonView>();

                  flashEffect = GetComponent<FlashEffect>();

                  spriteRenderer = GetComponent<SpriteRenderer>();

                  // 자식 오브젝트 중 이름이 "Explosion"이 아닌 오브젝트 가져오기
                  childs = transform.GetComponentsInChildren<Transform>()
                        .Where(child => (child.name != "Explosion") && (child.gameObject != gameObject)).ToArray();
                  explosionAnimator = GetComponentsInChildren<Animator>()
                        .FirstOrDefault(anim => anim.gameObject != gameObject);
            }

            private void OnEnable()
            {
                  // Body인데 소유권이 없으면, 소유권 요청
                  if (PhotonNetwork.IsMasterClient && photonView.Owner != PhotonNetwork.LocalPlayer) {
                        photonView.RequestOwnership();
                  }

                  spriteRenderer.color = Color.white;
                  foreach (Transform child in childs) {
                        child.gameObject.SetActive(true);
                  }
            }

            #region For animation event
            public void StartFlash(int color)
            {
                  // 0 == Red, 1 == Yellow
                  if (color == 0) flashEffect.Flash(1f, 0f, 0f, 1f); // red
                  else flashEffect.Flash(1f, 0.92f, 0.016f, 1f); // yellow
            }

            public void StartExplosion()
            {
                  spriteRenderer.color = Color.clear;
                  foreach (Transform child in childs) {
                        child.gameObject.SetActive(false);
                  }

                  explosionAnimator.SetTrigger("Explosion");
                  StartCoroutine(SetActiveAfterAnimation(explosionAnimator, "AM_BombExplosion", false));

                  ApplyBombImpact();
            }
            #endregion

            private IEnumerator SetActiveAfterAnimation(Animator animator, string animName, bool active)
            {
                  // 애니메이션이 끝날 때까지 대기
                  yield return new WaitForSeconds(animator.runtimeAnimatorController.animationClips
                              .FirstOrDefault(clip => clip.name == animName)?.length ?? 0f);

                  gameObject.SetActive(active);
            }

            private void ApplyBombImpact()
            {
                  foreach (RaycastHit2D hit in Physics2D.CircleCastAll(transform.position, 1, Vector2.zero, 0,
                        LayerMask.GetMask("Player", "Monster", "Obstacle"))) {
                        switch (LayerMask.LayerToName(hit.transform.gameObject.layer)) {
                              case "Player":
                                    // 마스터 클라이언트(Body)가 아니라면 return
                                    if (!PhotonNetwork.IsMasterClient) return;

                                    if (hit.transform.GetComponent<IsaacBody>() is IsaacBody player) {
                                          if (player.IsHurt) { }
                                          else {
                                                player.Health -= damage;
                                                player.IsHurt = true;
                                                ApplyKnockTo(player.GetComponent<Rigidbody2D>());
                                                GameManager.Instance.uiManager.setKilledPlayer = "Bomb";
                                          }
                                    }
                                    break;
                              case "Monster":
                                    // 마스터 클라이언트(Body)가 아니라면 return
                                    if (!PhotonNetwork.IsMasterClient) return;

                                    if (TryGetMonsterFields(hit.collider, out MonoBehaviour script, out FieldInfo statField,
                                          out PropertyInfo isHurtProperty, out FieldInfo monsterTypeField)) {
                                          if (statField.GetValue(script) is MonsterStat monsterStat &&
                                                monsterTypeField.GetValue(script) is MonsterType monsterType) {
                                                monsterStat.health -= damage;
                                                isHurtProperty.SetValue(script, true);
                                                ApplyKnockTo(script.GetComponent<Rigidbody2D>(), monsterType);
                                          }
                                    }
                                    else {
                                          Debug.LogWarning("stat 필드 또는 IsHurt 프로퍼티를 찾을 수 없습니다.");
                                    }
                                    break;
                              case "Obstacle":
                                    SpriteRenderer hitSR = hit.collider.GetComponent<SpriteRenderer>();
                                    switch (hit.collider.GetComponent<Obstacle>().GetType().ToString()) {
                                          case "Poop":
                                                Poop poopScript = hit.collider.GetComponent<Poop>();
                                                hitSR.sprite = poopScript.poopArray[(int)poopScript.poopType].destroyed;
                                                break;
                                          case "Rock":
                                                Rock rockScript = hit.collider.GetComponent<Rock>();
                                                Sprite destroyedSprite = rockScript.rockArray[(int)rockScript.rockType].destroyed;
                                                if (destroyedSprite) hitSR.sprite = destroyedSprite;
                                                break;
                                          case "Web":
                                                Web webScript = hit.collider.GetComponent<Web>();
                                                hitSR.sprite = webScript.destroyed;
                                                break;
                                    }
                                    break;
                        }
                  }
            }

            private bool TryGetMonsterFields(Collider2D collision, out MonoBehaviour script, out FieldInfo statField,
                  out PropertyInfo isHurtProperty, out FieldInfo monsterTypeField)
            {
                  // 초기화
                  script = null;
                  statField = null;
                  isHurtProperty = null;
                  monsterTypeField = null;

                  MonoBehaviour[] scripts = collision.GetComponents<MonoBehaviour>();
                  if (scripts.Length <= 1) scripts = collision.GetComponentsInParent<MonoBehaviour>();
                  foreach (MonoBehaviour s in scripts) {
                        Type baseType = s.GetType()?.BaseType; // 부모: Monster
                        if (baseType != null && baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(Monster<>)) {
                              // stat 필드와 IsHurt 프로퍼티 찾기
                              statField = baseType.GetField("stat", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                              isHurtProperty = baseType.GetProperty("IsHurt", BindingFlags.Instance | BindingFlags.Public);
                              // MonsterType 열거형 필드 찾기
                              monsterTypeField = baseType.GetField("monsterType", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

                              if (statField != null && isHurtProperty != null && monsterTypeField != null) {
                                    script = s;
                                    return true;
                              }
                              break;
                        }
                  }
                  return false;
            }

            private void ApplyKnockTo(Rigidbody2D targetRigid, MonsterType? monsterType = null)
            {
                  if (targetRigid.GetComponent<PhotonView>() is PhotonView view) {
                        if (!view.IsMine) view.RequestOwnership();
                  }

                  switch (monsterType) {
                        case MonsterType.Monstro:
                              // Knockback not applied
                              return;
                  }

                  targetRigid.velocity = Vector2.zero;
                  targetRigid.AddForce((targetRigid.position - (Vector2)transform.position).normalized * knockPower, ForceMode2D.Impulse);
            }


            //public float radius;
            private void OnDrawGizmos()
            {
                  Gizmos.color = Color.white;
                  //Gizmos.DrawWireSphere(transform.position, radius);
            }
      }
}
