using Photon.Pun;
using System.Collections;
using UnityEngine;

// Photon applied complete
public class FlashEffect : MonoBehaviour
{
      public Material flashMaterial;
      public float flashDuration;

      private SpriteRenderer spriteRenderer;
      private Material originalMaterial;

      private Coroutine flashRoutine;

      private void Awake()
      {
            spriteRenderer = GetComponent<SpriteRenderer>();
            originalMaterial = spriteRenderer.material;
            flashMaterial = new Material(flashMaterial);
      }

      private void OnEnable()
      {
            spriteRenderer.material = originalMaterial;
      }

      //public void Flash(Color color)
      //{
      //      if (flashRoutine != null) {
      //            StopCoroutine(flashRoutine);
      //      }

      //      flashRoutine = StartCoroutine(FlashRoutine(color));
      //}
      [PunRPC]
      public void Flash(float r, float g, float b, float a)
      {
            if (flashRoutine != null) {
                  StopCoroutine(flashRoutine);
            }

            flashRoutine = StartCoroutine(FlashRoutine(new Color(r, g, b, a)));
      }

      private IEnumerator FlashRoutine(Color _color)
      {
            spriteRenderer.material = flashMaterial;

            flashMaterial.color = _color;

            yield return new WaitForSeconds(flashDuration);

            spriteRenderer.material = originalMaterial;

            flashRoutine = null;
      }
}
