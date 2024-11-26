using UnityEngine;
using ObstacleSpace.RockSpace;

namespace ObstacleSpace
{
      namespace RockSpace
      {
            [System.Serializable]
            public class RockArray
            {
                  public Sprite[] rocks;
                  public Sprite destroyed;
            }
      }

      public class Rock : Obstacle
      {
            public enum RockType { Normal, Hard, InBomb, Pot }
            public RockType rockType;

            public RockArray[] rockArray;

            private SpriteRenderer spriteRenderer;

            private void Awake()
            {
                  spriteRenderer = GetComponent<SpriteRenderer>();
            }

            protected override void Start()
            {
                  base.Start();
                  spriteRenderer.sprite = rockArray[(int)rockType].
                        rocks[UnityEngine.Random.Range(0, rockArray[(int)rockType].rocks.Length)];
                  //Debug.Log(rockArray[(int)rockType].
                  //      rocks[UnityEngine.Random.Range(0, rockArray[(int)rockType].rocks.Length)]);
            }
      }
}
