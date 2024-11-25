using UnityEngine;

public class Bomb : MonoBehaviour
{
      private FlashEffect flashEffect;

      private SpriteRenderer spriteRenderer;

      private void Awake()
      {
            flashEffect = GetComponent<FlashEffect>();

            spriteRenderer = GetComponent<SpriteRenderer>();
      }

      private void OnEnable()
      {
            spriteRenderer.color = Color.white;
      }

      // For animation event
      public void StartFlash(int color)
      {
            // 0 == Red, 1 == Yellow
            if (color == 0) flashEffect.Flash(Color.red);
            else flashEffect.Flash(Color.yellow);
      }

      public void StartExplosion()
      {
            spriteRenderer.color = Color.clear;
      }
}
