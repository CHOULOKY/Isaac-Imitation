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

            private IsaacBody isaacBody;
            private IsaacHead isaacHead;

            private PhotonView photonView;

            private void Awake()
            {
                  isaacBody = FindAnyObjectByType<IsaacBody>(FindObjectsInactive.Include);
                  isaacHead = FindAnyObjectByType<IsaacHead>(FindObjectsInactive.Include);

                  photonView = GetComponent<PhotonView>();
            }

            private void OnCollisionEnter2D(Collision2D collision)
            {
                  if (!PhotonNetwork.IsMasterClient) return;

                  if (collision.collider.CompareTag("Player")) {
                        GameManager.Instance.uiManager.SetActivePassiveItem(passiveType);
                        ApplyAbility();
                        FollowPlayer();
                  }
            }

            private void ApplyAbility()
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

            private void FollowPlayer()
            {
                  photonView.RPC(nameof(RPC_FollowPlayer), RpcTarget.AllBuffered);
            }
            [PunRPC]
            private void RPC_FollowPlayer()
            {
                  GetComponent<Collider2D>().isTrigger = true;
                  transform.GetChild(0).gameObject.SetActive(true); // glow

                  this.transform.position = isaacBody.transform.position + Vector3.up;
                  this.transform.parent = isaacBody.transform;

                  StartCoroutine(DestroyAfterPickup());
            }

            private IEnumerator DestroyAfterPickup()
            {
                  Animator animator = isaacBody.GetComponent<Animator>();

                  // 애니메이션이 끝날 때까지 대기
                  yield return new WaitForSeconds(animator.runtimeAnimatorController.animationClips
                              .FirstOrDefault(clip => clip.name == "AM_IasscBodyPickup")?.length ?? 0f);
                  yield return new WaitForSeconds(1);

                  // 아이작에 아이템 변화 활성화
                  SetActiveItem(true);

                  foreach (SpriteRenderer renderer in GetComponentsInChildren<SpriteRenderer>()) {
                        renderer.color = Color.clear;
                  }

                  yield return new WaitForSeconds(1);

                  if (PhotonNetwork.IsMasterClient) {
                        PhotonNetwork.Destroy(this.gameObject);
                  }
            }

            private void SetActiveItem(bool active = true)
            {
                  switch (passiveType) {
                        case PassiveType.Onion:
                              isaacHead.onionVisual.passiveType = passiveType;
                              isaacHead.onionVisual.gameObject.SetActive(active);
                              break;
                        case PassiveType.InnerEye:
                              isaacHead.innerEyeVisual.passiveType = passiveType;
                              isaacHead.innerEyeVisual.gameObject.SetActive(active);
                              break;
                  }
            }
      }
}
