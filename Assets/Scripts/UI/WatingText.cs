using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;

public class WatingText : MonoBehaviour
{
      public float spaceTime = 1;

      private RectTransform rectTransform;
      private Vector3 originalRotation;
      [Tooltip("Z-Axis")] public float rotateAmount;

      private void Awake()
      {
            rectTransform = GetComponent<RectTransform>();
            originalRotation = rectTransform.eulerAngles; // 초기 회전값 저장
      }

      private void Start()
      {
            StartCoroutine(TextAnimation());
      }

      private IEnumerator TextAnimation()
      {
            WaitForSeconds waitTime = new WaitForSeconds(spaceTime);
            while (true) {
                  if (NetworkManager.Instance.canStartGame) break;

                  yield return waitTime;
                  // Z축 회전
                  rectTransform.eulerAngles = new Vector3(
                      originalRotation.x,
                      originalRotation.y,
                      originalRotation.z + rotateAmount
                  );
                  yield return waitTime;
                  // 원래 회전값으로 복원
                  rectTransform.eulerAngles = originalRotation;
            }
      }
}
