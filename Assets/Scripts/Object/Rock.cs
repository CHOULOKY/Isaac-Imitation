using UnityEngine;
using RockSpace;

namespace RockSpace
{
      [System.Serializable]
      public class RockArray
      {
            public Sprite[] rocks;
      }
}

public class Rock : Obstacle
{
      private enum RockType { Normal, Hard, InBomb, Pot}
      [SerializeField] private RockType rockType;

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
