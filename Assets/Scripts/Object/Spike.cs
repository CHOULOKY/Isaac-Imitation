using Photon.Pun;
using System.Threading;
using UnityEngine;

// Photon applied complete
namespace ObstacleSpace
{
      public class Spike : Obstacle
      {
            private PhotonView photonView;

            public int damage = 1;

            private void Awake()
            {
                  photonView = GetComponent<PhotonView>();
            }

            private void OnTriggerEnter2D(Collider2D collision)
            {
                  OnTriggerStay2D(collision);
            }

            private void OnTriggerStay2D(Collider2D collision)
            {
                  if (collision.TryGetComponent<IsaacBody>(out var player)) {
                        if (!PhotonNetwork.IsMasterClient) return; // ������ Ŭ���̾�Ʈ(body)�� �ƴϸ� return
                        else if (photonView.Owner != PhotonNetwork.LocalPlayer) {
                              photonView.RequestOwnership(); // ������ Ŭ���̾�Ʈ(Body)�� ���� ������Ʈ�� �����ϰ� ���� ������
                        }

                        if (player.IsHurt) { }
                        else {
                              player.Health -= damage;
                              player.IsHurt = true;
                              GameManager.Instance.uiManager.setKilledPlayer = "Spike";
                        }
                  }
            }
      }
}
