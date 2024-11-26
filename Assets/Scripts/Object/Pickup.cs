using System.Collections;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace ItemSpace
{
      public class Pickup : MonoBehaviour
      {
            protected Animator animator;
            protected Collider2D thisCollider;

            public ItemFactory.Items ItemType;

            protected virtual void Awake()
            {
                  animator = GetComponent<Animator>();
                  thisCollider = GetComponent<Collider2D>();
            }

            protected virtual void OnEnable()
            {
                  StartCoroutine(OnEnableAfterSpawn(animator, "AM_SpawnItem"));
            }

            protected virtual IEnumerator OnEnableAfterSpawn(Animator animator, string animName)
            {
                  yield return new WaitForSeconds(animator.runtimeAnimatorController.animationClips
                              .FirstOrDefault(clip => clip.name == animName)?.length ?? 0f);

                  thisCollider.enabled = true;
            }

            protected virtual void OnCollisionEnter2D(Collision2D collision)
            {
                  if (collision.collider.GetComponent<IsaacBody>() is IsaacBody player) {
                        HandlePickup();
                        switch (ItemType) {
                              case ItemFactory.Items.Heart:
                                    // Override in the heart script
                                    break;
                              case ItemFactory.Items.Bomb:
                                    player.bombCount++;
                                    break;
                        }
                  }
            }

            protected virtual void HandlePickup()
            {
                  thisCollider.enabled = false;
                  animator.SetTrigger("Pickup");
                  StartCoroutine(SetActiveAfterAnimation(animator, "AM_PickupItem", false));
            }

            protected virtual IEnumerator SetActiveAfterAnimation(Animator animator, string animName, bool active)
            {
                  // 애니메이션이 끝날 때까지 대기
                  yield return new WaitForSeconds(animator.runtimeAnimatorController.animationClips
                              .FirstOrDefault(clip => clip.name == animName)?.length ?? 0f);

                  gameObject.SetActive(active);
            }
      }
}
