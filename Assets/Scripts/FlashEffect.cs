using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public void Flash(Color _color)
    {
        if (flashRoutine != null) {
            StopCoroutine(flashRoutine);
        }

        flashRoutine = StartCoroutine(FlashRoutine(_color));
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
