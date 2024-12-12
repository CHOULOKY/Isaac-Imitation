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

            originalRotation = rectTransform.eulerAngles; // �ʱ� ȸ���� ����
      }

      public void OnPointerEnter(PointerEventData eventData)
      {
            // Z���� -5�� ȸ��
            rectTransform.eulerAngles = new Vector3(
                originalRotation.x,
                originalRotation.y,
                originalRotation.z + rotateAmount
            );
      }

      public void OnPointerExit(PointerEventData eventData)
      {
            // ���� ȸ�������� ����
            rectTransform.eulerAngles = originalRotation;
      }
}
