using Photon.Pun;
using UnityEngine;

namespace ItemSpace
{
      public class ItemVisual : MonoBehaviour
      {
            private PhotonView photonView;

            private IsaacBody body;
            private IsaacHead head;

            private SpriteRenderer spriteRenderer;
            private SpriteRenderer headRenderer;

            private Animator animator;

            public PassiveType passiveType;


            private void Awake()
            {
                  photonView = GetComponent<PhotonView>();

                  body = GetComponentInParent<IsaacBody>();
                  head = GetComponentInParent<IsaacHead>();

                  spriteRenderer = GetComponent<SpriteRenderer>();
                  headRenderer = head.GetComponent<SpriteRenderer>();

                  animator = GetComponent<Animator>();
            }

            private void OnEnable()
            {
                  // Head인데 소유권이 없으면 요청
                  if (!PhotonNetwork.IsMasterClient && photonView.Owner != PhotonNetwork.LocalPlayer) {
                        photonView.RequestOwnership();
                  }
            }

            private void Update()
            {
                  if (spriteRenderer.flipX != headRenderer.flipX) {
                        spriteRenderer.flipX = headRenderer.flipX;
                        photonView.RPC(nameof(RPC_SetSpriteFlipX), RpcTarget.Others);
                  }
            }

            [PunRPC]
            private void RPC_SetSpriteFlipX()
            {
                  spriteRenderer.flipX = headRenderer.flipX;
            }

            public void ControlAnimator(bool setTrigger = false)
            {
                  switch (passiveType) {
                        case PassiveType.Onion:
                        case PassiveType.InnerEye:
                              if (setTrigger) {
                                    animator.SetTrigger("Fire1");
                              }
                              else {
                                    animator.SetInteger("XAxisRaw", (int)head.inputVec.x);
                                    animator.SetInteger("YAxisRaw", (int)head.inputVec.y);
                              }
                              break;
                  }
            }
      }
}
