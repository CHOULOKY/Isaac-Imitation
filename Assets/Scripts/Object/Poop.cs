using ObstacleSpace.PoopSpace;
using Photon.Pun;
using UnityEngine;

// Photon applied complete
namespace ObstacleSpace
{
      namespace PoopSpace
      {
            [System.Serializable]
            public class PoopArray
            {
                  public Sprite[] poops;
                  public Sprite destroyed;
            }
      }

      public class Poop : Obstacle
      {
            private PhotonView photonView;

            public enum PoopType { Normal, Red, Fresh, Gold, Rainbow }
            public PoopType poopType;

            public PoopArray[] poopArray;
            private int curPoopIndex = 0;

            private SpriteRenderer spriteRenderer;

            private void Awake()
            {
                  photonView = GetComponent<PhotonView>();

                  spriteRenderer = GetComponent<SpriteRenderer>();
            }

            protected override void Start()
            {
                  base.Start();
                  spriteRenderer.sprite = poopArray[(int)poopType].poops[0];
            }

            private void OnTriggerEnter2D(Collider2D collision)
            {
                  if (collision.GetComponent<IsaacTear>()) {
                        // 마스터 클라이언트가 아니고 현재 오브젝트를 소유하고 있지 않으면
                        if (PhotonNetwork.IsMasterClient) return;
                        else if (photonView.Owner != PhotonNetwork.LocalPlayer) {
                              photonView.RequestOwnership();
                        }

                        if (curPoopIndex >= poopArray[(int)poopType].poops.Length - 1) { }
                        else {
                              //spriteRenderer.sprite = poopArray[(int)poopType].poops[++curPoopIndex];
                              //if (curPoopIndex == poopArray[(int)poopType].poops.Length - 1) {
                              //      GetComponent<Collider2D>().enabled = false;
                              //}
                              photonView.RPC(nameof(RPC_SetPoop), RpcTarget.AllBuffered, poopType, ++curPoopIndex);
                        }
                  }
            }
            [PunRPC]
            private void RPC_SetPoop(PoopType poopType, int index)
            {
                  curPoopIndex = index;
                  spriteRenderer.sprite = poopArray[(int)poopType].poops[index];
                  if (curPoopIndex == poopArray[(int)poopType].poops.Length - 1) {
                        GetComponent<Collider2D>().enabled = false;
                  }
            }
      }
}
