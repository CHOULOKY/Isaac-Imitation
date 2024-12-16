using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace ItemSpace
{
      public enum PassiveType { Onion, InnerEye }

      public class PassiveItem : MonoBehaviour
      {
            public PassiveType passiveType;

            private void OnCollisionEnter2D(Collision2D collision)
            {
                  //if (!PhotonNetwork.IsMasterClient) return;

                  if (collision.collider.CompareTag("Player")) {
                        IsaacBody isaacBody = collision.collider.GetComponent<IsaacBody>();
                        IsaacHead isaacHead = collision.collider.GetComponentInChildren<IsaacHead>();

                        GameManager.Instance.uiManager.SetActivePassiveItem(passiveType);
                        if (PhotonNetwork.IsMasterClient) {
                              ApplyAbility(isaacBody, isaacHead);
                        }
                        FollowPlayer(isaacBody, isaacHead);
                  }
            }

            private void ApplyAbility(IsaacBody isaacBody, IsaacHead isaacHead)
            {
                  switch (passiveType) {
                        case PassiveType.Onion:
                              float adjustedStat = isaacHead.AttackSpeed / 2;
                              isaacHead.AttackSpeed = adjustedStat >= 0.1f ? adjustedStat : 0.1f;
                              break;
                        case PassiveType.InnerEye:
                              isaacHead.AttackCount = 3;
                              break;
                  }
            }

            private void FollowPlayer(IsaacBody isaacBody, IsaacHead isaacHead)
            {
                  GetComponent<Collider2D>().isTrigger = true;
                  transform.GetChild(0).gameObject.SetActive(true); // glow

                  this.transform.position = isaacBody.transform.position + Vector3.up;
                  this.transform.parent = isaacBody.transform;

                  StartCoroutine(DestroyAfterPickup(isaacBody, isaacHead));
            }
            private IEnumerator DestroyAfterPickup(IsaacBody isaacBody, IsaacHead isaacHead)
            {
                  Animator animator = isaacBody.GetComponent<Animator>();

                  // �ִϸ��̼��� ���� ������ ���
                  yield return new WaitForSeconds(animator.runtimeAnimatorController.animationClips
                              .FirstOrDefault(clip => clip.name == "AM_IasscBodyPickup")?.length ?? 0f);
                  yield return new WaitForSeconds(1);

                  SetActiveItem(isaacBody, isaacHead, true);
                  this.gameObject.GetComponent<SpriteRenderer>().color = Color.clear;

                  yield return new WaitForSeconds(1);

                  if (PhotonNetwork.IsMasterClient) {
                        PhotonNetwork.Destroy(this.gameObject);
                  }
            }

            private void SetActiveItem(IsaacBody isaacBody, IsaacHead isaacHead, bool active = true)
            {
                  switch (passiveType) {
                        case PassiveType.Onion:
                              isaacHead.onionVisual.passiveType = passiveType;
                              isaacHead.onionVisual.gameObject.SetActive(active);
                              break;
                        case PassiveType.InnerEye:

                              break;
                  }
            }
      }
}
