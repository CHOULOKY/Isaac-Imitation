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

                  // �ڽ� ������Ʈ �� �̸��� "Explosion"�� �ƴ� ������Ʈ ��������
                  childs = transform.GetComponentsInChildren<Transform>()
                        .Where(child => (child.name != "Explosion") && (child.gameObject != gameObject)).ToArray();
                  explosionAnimator = GetComponentsInChildren<Animator>()
                        .FirstOrDefault(anim => anim.gameObject != gameObject);
            }

            private void OnEnable()
            {
                  // Body�ε� �������� ������, ������ ��û
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
                  // �ִϸ��̼��� ���� ������ ���
                  yield return new WaitForSeconds(animator.runtimeAnimatorController.animationClips
                              .FirstOrDefault(clip => clip.name == animName)?.length ?? 0f);

                  gameObject.SetActive(active);
            }

            private void ApplyBombImpact()
            {
                  int mask = LayerMask.GetMask("Player", "Monster", "Obstacle");
                  foreach (RaycastHit2D hit in Physics2D.CircleCastAll(transform.position, 1.1f, Vector2.zero, 0, mask)) {
                        //Debug.LogError(hit.collider.gameObject.name + " + "
                        //      + LayerMask.LayerToName(hit.collider.gameObject.layer));
                        switch (LayerMask.LayerToName(hit.collider.gameObject.layer)) {
                              case "Player":
                                    // ������ Ŭ���̾�Ʈ(Body)�� �ƴ϶�� return
                                    if (!PhotonNetwork.IsMasterClient) return;

                                    if (hit.collider.GetComponent<IsaacBody>() is IsaacBody player) {
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
                                    // ������ Ŭ���̾�Ʈ(Body)�� �ƴ϶�� return
                                    if (!PhotonNetwork.IsMasterClient) return;

                                    if (TryGetMonsterFields(hit.collider, out MonoBehaviour script, out PropertyInfo healthProperty,
                                          out PropertyInfo isHurtProperty, out FieldInfo monsterTypeField)) {
                                          if (monsterTypeField.GetValue(script) is MonsterType monsterType) {
                                                //if (hit.collider.GetComponent<PhotonView>() is PhotonView pv) {
                                                //      if (!pv.IsMine) pv.RequestOwnership();
                                                //}
                                                //monsterStat.health -= damage;
                                                if (healthProperty.GetValue(script) is float currentHealth) { // float Ÿ������ ����
                                                      float newHealth = currentHealth - damage;
                                                      healthProperty.SetValue(script, newHealth);
                                                }
                                                isHurtProperty.SetValue(script, true);

                                                ApplyKnockTo(script.GetComponent<Rigidbody2D>(), monsterType);
                                          }
                                    }
                                    else {
                                          Debug.LogWarning("stat �ʵ� �Ǵ� IsHurt ������Ƽ�� ã�� �� �����ϴ�.");
                                    }
                                    break;
                              case "Obstacle":
                                    SpriteRenderer hitSR = hit.collider.GetComponent<SpriteRenderer>();
                                    string hitName = hit.collider.GetComponent<Obstacle>().GetType().ToString();
                                    if (hitName.Contains("Poop")) {
                                          Poop poopScript = hit.collider.GetComponent<Poop>();
                                          hitSR.sprite = poopScript.poopArray[(int)poopScript.poopType].destroyed;
                                    }
                                    else if (hitName.Contains("Rock")) {
                                          Rock rockScript = hit.collider.GetComponent<Rock>();
                                          Sprite destroyedSprite = rockScript.rockArray[(int)rockScript.rockType].destroyed;
                                          if (destroyedSprite) hitSR.sprite = destroyedSprite;
                                    }
                                    else if (hitName.Contains("Web")) {
                                          Web webScript = hit.collider.GetComponent<Web>();
                                          hitSR.sprite = webScript.destroyed;
                                    }
                                    else if (hitName.Contains("Spike")) {
                                          break;
                                    }
                                    hitSR.GetComponent<Collider2D>().enabled = false;
                                    break;
                        }
                  }
            }

            private bool TryGetMonsterFields(Collider2D collision, out MonoBehaviour script, out PropertyInfo healthProperty,
                  out PropertyInfo isHurtProperty, out FieldInfo monsterTypeField)
            {
                  // �ʱ�ȭ
                  script = null;
                  healthProperty = null;
                  isHurtProperty = null;
                  monsterTypeField = null;

                  MonoBehaviour[] scripts = collision.GetComponents<MonoBehaviour>();
                  if (scripts.Length <= 1) scripts = collision.GetComponentsInParent<MonoBehaviour>();
                  foreach (MonoBehaviour s in scripts) {
                        Type baseType = s.GetType()?.BaseType; // �θ�: Monster
                        if (baseType != null && baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(Monster<>)) {
                              // stat �ʵ�� IsHurt ������Ƽ ã��
                              //statField = baseType.GetField("stat", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                              healthProperty = baseType.GetProperty("Health", BindingFlags.Instance | BindingFlags.Public);
                              isHurtProperty = baseType.GetProperty("IsHurt", BindingFlags.Instance | BindingFlags.Public);
                              // MonsterType ������ �ʵ� ã��
                              monsterTypeField = baseType.GetField("monsterType", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

                              if (healthProperty != null && isHurtProperty != null && monsterTypeField != null) {
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
