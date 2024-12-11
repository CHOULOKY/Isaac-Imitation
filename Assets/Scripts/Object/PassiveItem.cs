using Photon.Pun;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace ItemSpace
{
      public enum PassiveType { Onion }

      public class PassiveItem : MonoBehaviour
      {
            public PassiveType passiveType;

            private void OnCollisionEnter2D(Collision2D collision)
            {
                  //if (!PhotonNetwork.IsMasterClient) return;

                  if (collision.collider.CompareTag("Player")) {
                        if (PhotonNetwork.IsMasterClient) {
                              ApplyAbility(collision.collider.gameObject);
                        }
                        FollowPlayer(collision.collider.gameObject);
                  }
            }

            private void ApplyAbility(GameObject player)
            {
                  IsaacBody isaacBody = player.GetComponent<IsaacBody>();
                  IsaacHead isaacHead = player.GetComponentInChildren<IsaacHead>();

                  switch (passiveType) {
                        case PassiveType.Onion:
                              float adjustedStat = isaacHead.AttackSpeed / 2;
                              isaacHead.AttackSpeed = adjustedStat >= 0.1f ? adjustedStat : 0.1f;
                              break;
                  }
            }

            private void FollowPlayer(GameObject player)
            {
                  GetComponent<Collider2D>().isTrigger = true;
                  transform.GetChild(0).gameObject.SetActive(true); // glow

                  this.transform.position = player.transform.position + Vector3.up;
                  this.transform.parent = player.transform;

                  if (PhotonNetwork.IsMasterClient) {
                        StartCoroutine(DestroyAfterPickup(player));
                  }
            }
            private IEnumerator DestroyAfterPickup(GameObject player)
            {
                  Animator animator = player.GetComponent<Animator>();

                  // 애니메이션이 끝날 때까지 대기
                  yield return new WaitForSeconds(animator.runtimeAnimatorController.animationClips
                              .FirstOrDefault(clip => clip.name == "AM_IasscBodyPickup")?.length ?? 0f);
                  yield return new WaitForSeconds(1);

                  PhotonNetwork.Destroy(this.gameObject);
            }
      }
}
