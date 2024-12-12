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
                        if (!PhotonNetwork.IsMasterClient) return; // ������ Ŭ���̾�Ʈ(Body)�� �ƴϸ� return
                        else if (photonView.Owner != PhotonNetwork.LocalPlayer) {
                              photonView.RequestOwnership(); // ������ Ŭ���̾�Ʈ(Body)�� �������� ������, ������ ��û
                        }

                        if (player.IsHurt || curWebIndex >= webs.Length - 1) { }
                        else {
                              player.curMoveForce -= slowAmount;
                              player.curMaxVelocity = player.curMoveForce / 5 + 1;
                        }
                  }
                  if (collision.GetComponent<IsaacTear>()) {
                        if (PhotonNetwork.IsMasterClient) return; // ������ Ŭ���̾�Ʈ(Body)�� return
                        else if (photonView.Owner != PhotonNetwork.LocalPlayer) {
                              photonView.RequestOwnership(); // ������ Ŭ���̾�Ʈ(Body)�� �ƴϰ� �������� ������, ������ ��û
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
                        if (!PhotonNetwork.IsMasterClient) return; // ������ Ŭ���̾�Ʈ(Body)�� �ƴϸ� return
                        else if (photonView.Owner != PhotonNetwork.LocalPlayer) {
                              photonView.RequestOwnership(); // ������ Ŭ���̾�Ʈ(Body)�� �������� ������, ������ ��û
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
