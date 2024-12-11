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
                        if (!PhotonNetwork.IsMasterClient) return; // 마스터 클라이언트(body)가 아니면 return
                        else if (photonView.Owner != PhotonNetwork.LocalPlayer) {
                              photonView.RequestOwnership(); // 마스터 클라이언트(Body)고 현재 오브젝트를 소유하고 있지 않으면
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
