using ObstacleSpace.PoopSpace;
using UnityEngine;

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
            public enum PoopType { Normal, Red, Fresh, Gold, Rainbow }
            public PoopType poopType;

            public PoopArray[] poopArray;
            private int curPoopIndex = 0;

            private SpriteRenderer spriteRenderer;

            private void Awake()
            {
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
                        if (curPoopIndex >= poopArray[(int)poopType].poops.Length - 1) { }
                        else {
                              spriteRenderer.sprite = poopArray[(int)poopType].poops[++curPoopIndex];
                              if (curPoopIndex == poopArray[(int)poopType].poops.Length - 1) {
                                    GetComponent<Collider2D>().enabled = false;
                              }
                        }
                  }
            }
      }
}
