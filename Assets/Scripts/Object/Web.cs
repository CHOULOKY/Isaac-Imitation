using Photon.Pun;
using UnityEngine;

// Photon applied complete
namespace ObstacleSpace
{
      public class Web : Obstacle
      {
            private PhotonView photonView;

            public float slowAmount = 10;

            public Sprite[] webs;
            public Sprite destroyed;
            private int curWebIndex = 0;

            private SpriteRenderer spriteRenderer;

            private void Awake()
            {
                  photonView = GetComponent<PhotonView>();

                  spriteRenderer = GetComponent<SpriteRenderer>();
            }

            protected override void Start()
            {
                  spriteRenderer.sprite = webs[0];
            }

            private void OnTriggerEnter2D(Collider2D collision)
            {
                  if (collision.TryGetComponent<IsaacBody>(out var player)) {
                        if (!PhotonNetwork.IsMasterClient) return; // 마스터 클라이언트(Body)가 아니면 return
                        else if (photonView.Owner != PhotonNetwork.LocalPlayer) {
                              photonView.RequestOwnership(); // 마스터 클라이언트(Body)고 소유권이 없으면, 소유권 요청
                        }

                        if (player.IsHurt || curWebIndex >= webs.Length - 1) { }
                        else {
                              player.curMoveForce -= slowAmount;
                              player.curMaxVelocity = player.curMoveForce / 5 + 1;
                        }
                  }
                  if (collision.GetComponent<IsaacTear>()) {
                        if (PhotonNetwork.IsMasterClient) return; // 마스터 클라이언트(Body)면 return
                        else if (photonView.Owner != PhotonNetwork.LocalPlayer) {
                              photonView.RequestOwnership(); // 마스터 클라이언트(Body)가 아니고 소유권이 없으면, 소유권 요청
                        }

                        if (curWebIndex >= webs.Length - 1) { }
                        else {
                              //spriteRenderer.sprite = webs[++curWebIndex];
                              photonView.RPC(nameof(RPC_SetWeb), RpcTarget.AllBuffered, ++curWebIndex);
                        }
                  }
            }
            [PunRPC]
            private void RPC_SetWeb(int index)
            {
                  curWebIndex = index;
                  spriteRenderer.sprite = webs[index];
            }

            private void OnTriggerExit2D(Collider2D collision)
            {
                  if (collision.TryGetComponent<IsaacBody>(out var player)) {
                        if (!PhotonNetwork.IsMasterClient) return; // 마스터 클라이언트(Body)가 아니면 return
                        else if (photonView.Owner != PhotonNetwork.LocalPlayer) {
                              photonView.RequestOwnership(); // 마스터 클라이언트(Body)고 소유권이 없으면, 소유권 요청
                        }

                        if (player.IsHurt || curWebIndex >= webs.Length - 1) { }
                        else {
                              player.curMoveForce += slowAmount;
                              player.curMaxVelocity = player.curMoveForce / 5 + 1;
                        }
                  }
            }
      }
}
