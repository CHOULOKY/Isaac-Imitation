using System.Threading;
using UnityEngine;

public class Spike : Obstacle
{
      public int damage = 1;

      private void OnTriggerEnter2D(Collider2D collision)
      {
            OnTriggerStay2D(collision);
      }

      private void OnTriggerStay2D(Collider2D collision)
      {
            if (collision.TryGetComponent<IsaacBody>(out var player)) {
                  if (player.IsHurt) { }
                  else {
                        player.health -= damage;
                        player.IsHurt = true;
                  }
            }
      }
}
