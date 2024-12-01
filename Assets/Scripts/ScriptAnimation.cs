using System.Collections;
using TMPro;
using UnityEngine;

public class ScriptAnimation : MonoBehaviour
{
      protected virtual IEnumerator TypingAnimation(TMP_Text tmpText, string text, float speed, bool unTyping)
      {
            WaitForSeconds waitTime = new WaitForSeconds(speed);

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
                        tmpText.text = tmpText.text.Substring(0, i); // 문자열의 i번째 문자 제거
                        yield return waitTime;
                  }
            }
      }

      protected virtual IEnumerator RotateAnimation(
            RectTransform rectTransform, float spaceTime = 0.75f, float rotateAmount = -2f)
      {
            Vector3 originalRotation = rectTransform.eulerAngles; // 초기 회전값 저장

            WaitForSecondsRealtime waitTime = new WaitForSecondsRealtime(spaceTime < 0.1f ? 1 : spaceTime);
            while (true) {
                  // if (NetworkManager.Instance.canStartGame) break;

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
