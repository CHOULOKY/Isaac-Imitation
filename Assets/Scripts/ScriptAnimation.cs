using System.Collections;
using TMPro;
using UnityEngine;

public class ScriptAnimation : MonoBehaviour
{
      protected virtual IEnumerator TypingAnimation(TMP_Text tmpText, string text, float speed, bool unTyping)
      {
            WaitForSecondsRealtime waitTime = new WaitForSecondsRealtime(speed);

            if (!unTyping) {
                  tmpText.text = "";
                  for (int i = 0; i < text.Length; i++) {
                        tmpText.text += text[i];
                        yield return waitTime;
                  }
            }
            else {
                  int length = tmpText.text.Length;
                  if (length <= 0) yield break;
                  for (int i = length-1; i >= 0; i--) {
                        tmpText.text = tmpText.text.Substring(0, i); // ���ڿ��� i��° ���� ����
                        yield return waitTime;
                  }
            }
      }

      protected virtual IEnumerator RotateAnimation(
            RectTransform rectTransform, float spaceTime = 0.75f, float rotateAmount = -2f)
      {
            Vector3 originalRotation = rectTransform.eulerAngles; // �ʱ� ȸ���� ����

            WaitForSecondsRealtime waitTime = new WaitForSecondsRealtime(spaceTime < 0.1f ? 1 : spaceTime);
            while (true) {
                  // if (NetworkManager.Instance.canStartGame) break;

                  yield return waitTime;
                  // Z�� ȸ��
                  rectTransform.eulerAngles = new Vector3(
                      originalRotation.x,
                      originalRotation.y,
                      originalRotation.z + rotateAmount
                  );
                  yield return waitTime;
                  // ���� ȸ�������� ����
                  rectTransform.eulerAngles = originalRotation;
            }
      }
}
