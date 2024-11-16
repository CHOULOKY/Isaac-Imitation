using System.Collections;
using UnityEngine;

public class SpriteShake : MonoBehaviour
{
      public float duration = 0.25f;   // 흔들림 지속 시간
      public float magnitude = 0.1f; // 흔들림 강도

      private Vector3 originalPosition; // 원래 위치
      private float elapsed = 0.0f;     // 경과 시간

      public void StartShake()
      {
            originalPosition = transform.localPosition;
            elapsed = 0.0f;
            StartCoroutine(Shake());
      }

      private IEnumerator Shake()
      {
            float offsetX, offsetY;
            while (elapsed < duration) {
                  elapsed += Time.deltaTime;

                  // 랜덤한 위치 생성
                  offsetX = UnityEngine.Random.Range(-1f, 1f) * magnitude;
                  offsetY = UnityEngine.Random.Range(-1f, 1f) * magnitude;

                  transform.localPosition = originalPosition + new Vector3(offsetX, offsetY, 0);

                  yield return null; // 다음 프레임까지 대기
            }

            // 흔들림 종료 후 원래 위치로 복구
            transform.localPosition = originalPosition;
      }
}
