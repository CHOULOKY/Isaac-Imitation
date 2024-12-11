using Photon.Pun;
using System.Collections;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

// Photon applied complete
namespace ItemSpace
{
      public class Pickup : MonoBehaviour
      {
            protected PhotonView photonView;

            protected Animator animator;
            protected Collider2D thisCollider;

            public ItemFactory.Items ItemType;

            protected virtual void Awake()
            {
                  photonView = GetComponent<PhotonView>();

                  animator = GetComponent<Animator>();
                  thisCollider = GetComponent<Collider2D>();
            }

            protected virtual void OnEnable()
            {
                  // 마스터 클라이언트(Body)고 현재 오브젝트를 소유하고 있지 않으면
                  if (!PhotonNetwork.IsMasterClient) return;
                  else if (photonView.Owner != PhotonNetwork.LocalPlayer) {
                        photonView.RequestOwnership();
                  }

                  StartCoroutine(OnEnableAfterSpawn(animator, "AM_SpawnItem"));
            }

            protected virtual IEnumerator OnEnableAfterSpawn(Animator animator, string animName)
            {
                  yield return new WaitForSeconds(animator.runtimeAnimatorController.animationClips
                              .FirstOrDefault(clip => clip.name == animName)?.length ?? 0f);

                  //thisCollider.enabled = true;
                  photonView.RPC(nameof(RPC_SetColliderEnabled), RpcTarget.AllBuffered, true);
            }

            protected virtual void OnCollisionEnter2D(Collision2D collision)
            {
                  // 마스터 클라이언트(Body)가 아니면 return
                  if (!PhotonNetwork.IsMasterClient) return;

                  if (collision.collider.GetComponent<IsaacBody>() is IsaacBody player) {
                        HandlePickup();
                        switch (ItemType) {
                              case ItemFactory.Items.Heart:
                                    // Override in the heart script
                                    break;
                              case ItemFactory.Items.BombPickup:
                                    player.BombCount++;
                                    break;
                        }
                  }
            }
            
            protected virtual void HandlePickup()
            {
                  //thisCollider.enabled = false;
                  photonView.RPC(nameof(RPC_SetColliderEnabled), RpcTarget.AllBuffered, false);
                  //animator.SetTrigger("Pickup");
                  photonView.RPC(nameof(RPC_SetTrigger), RpcTarget.AllBuffered, "Pickup");
                  StartCoroutine(SetActiveAfterAnimation(animator, "AM_PickupItem", false));
            }
            [PunRPC]
            protected virtual void RPC_SetColliderEnabled(bool value)
            {
                  thisCollider.enabled = value;
            }
            [PunRPC]
            protected virtual void RPC_SetTrigger(string name)
            {
                  animator.SetTrigger(name);
            }

            protected virtual IEnumerator SetActiveAfterAnimation(Animator animator, string animName, bool active)
            {
                  // 애니메이션이 끝날 때까지 대기
                  yield return new WaitForSeconds(animator.runtimeAnimatorController.animationClips
                              .FirstOrDefault(clip => clip.name == animName)?.length ?? 0f);

                  //gameObject.SetActive(active);
                  photonView.RPC(nameof(RPC_SetObjectActive), RpcTarget.AllBuffered, active);
            }
            [PunRPC]
            protected virtual void RPC_SetObjectActive(bool active)
            {
                  gameObject.SetActive(active);
            }
      }
}
