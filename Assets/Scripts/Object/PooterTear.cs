using System.Reflection;
using UnityEngine;

public class PooterTear : Tear
{
      private void OnTriggerEnter2D(Collider2D collision)
      {
            if (collision.CompareTag("Wall") || collision.CompareTag("Obstacle")) {
                  DisableTear();
            }
            else if (collision.CompareTag("Player")) {
                  DisableTear();

                  if (collision.TryGetComponent<IsaacBody>(out var player)) {
                        if (player.IsHurt) return;
                        player.Health -= tearDamage;
                        player.IsHurt = true;
                  }
                  else {
                        Debug.LogWarning("Player(IsaacBody)를 찾을 수 없습니다.");
                  }
            }
      }
}
