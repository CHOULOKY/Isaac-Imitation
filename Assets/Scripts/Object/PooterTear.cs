using Photon.Pun;
using System.Reflection;
using UnityEngine;

public class PooterTear : Tear
{
      protected override void OnEnable()
      {
            // Body�� ����
            if (PhotonNetwork.IsMasterClient && photonView.Owner != PhotonNetwork.LocalPlayer)
                  photonView.RequestOwnership();

            base.OnEnable();
      }

      private void OnTriggerEnter2D(Collider2D collision)
      {
            // Head�� return
            if (!PhotonNetwork.IsMasterClient) return;

            if (collision.CompareTag("Wall") || collision.CompareTag("Obstacle")) {
                  DisableTear();
            }
            else if (collision.CompareTag("Player")) {
                  DisableTear();

                  if (collision.TryGetComponent<IsaacBody>(out var player)) {
                        if (player.IsHurt) return;
                        player.Health -= tearDamage;
                        player.IsHurt = true;
                        GameManager.Instance.uiManager.setKilledPlayer = "Pooter";
                  }
                  else {
                        Debug.LogWarning("Player(IsaacBody)�� ã�� �� �����ϴ�.");
                  }
            }
      }
}
