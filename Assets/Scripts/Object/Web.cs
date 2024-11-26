using UnityEngine;

namespace ObstacleSpace
{
      public class Web : Obstacle
      {
            public float slowAmount = 10;

            public Sprite[] webs;
            public Sprite destroyed;
            private int curWebIndex = 0;

            private SpriteRenderer spriteRenderer;

            private void Awake()
            {
                  spriteRenderer = GetComponent<SpriteRenderer>();
            }

            protected override void Start()
            {
                  spriteRenderer.sprite = webs[0];
            }

            private void OnTriggerEnter2D(Collider2D collision)
            {
                  if (collision.TryGetComponent<IsaacBody>(out var player)) {
                        if (player.IsHurt || curWebIndex >= webs.Length - 1) { }
                        else {
                              player.curMoveForce -= slowAmount;
                              player.curMaxVelocity = player.curMoveForce / 5 + 1;
                        }
                  }
                  if (collision.GetComponent<IsaacTear>()) {
                        if (curWebIndex >= webs.Length - 1) { }
                        else {
                              spriteRenderer.sprite = webs[++curWebIndex];
                        }
                  }
            }

            private void OnTriggerExit2D(Collider2D collision)
            {
                  if (collision.TryGetComponent<IsaacBody>(out var player)) {
                        if (player.IsHurt || curWebIndex >= webs.Length - 1) { }
                        else {
                              player.curMoveForce += slowAmount;
                              player.curMaxVelocity = player.curMoveForce / 5 + 1;
                        }
                  }
            }
      }
}
