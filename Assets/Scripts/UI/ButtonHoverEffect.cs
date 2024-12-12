using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
      private RectTransform rectTransform;

      private Vector3 originalRotation;
      [Tooltip("Z-Axis")] public float rotateAmount;

      private void Awake()
      {
            rectTransform = GetComponent<RectTransform>();

            originalRotation = rectTransform.eulerAngles; // 초기 회전값 저장
      }

      public void OnPointerEnter(PointerEventData eventData)
      {
            // Z축을 -5도 회전
            rectTransform.eulerAngles = new Vector3(
                originalRotation.x,
                originalRotation.y,
                originalRotation.z + rotateAmount
            );
      }

      public void OnPointerExit(PointerEventData eventData)
      {
            // 원래 회전값으로 복원
            rectTransform.eulerAngles = originalRotation;
      }
}
