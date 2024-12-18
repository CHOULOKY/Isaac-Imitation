using System.Collections;
using UnityEngine;

public class SetResolution : MonoBehaviour
{
      public static float deviceWidth, deviceHeight;

      protected virtual void Start()
      {
            SetResolutionCamera(1280, 720);
      }

      protected virtual void SetResolutionCamera(int width, int height)
      {
            int setWidth = width; // 사용자 설정 너비
            int setHeight = height; // 사용자 설정 높이

            deviceWidth = Screen.width; // 기기 너비 저장
            deviceHeight = Screen.height; // 기기 높이 저장

            Screen.SetResolution(setWidth, (int)(((float)deviceHeight / deviceWidth) * setWidth), false);

            if ((float)setWidth / setHeight < (float)deviceWidth / deviceHeight) // 기기의 해상도 비가 더 큰 경우
            {
                  float newWidth = ((float)setWidth / setHeight) / ((float)deviceWidth / deviceHeight); // 새로운 너비
                  Camera.main.rect = new Rect((1f - newWidth) / 2f, 0f, newWidth, 1f); // 새로운 Rect 적용
            }
            else // 게임의 해상도 비가 더 큰 경우
              {
                  float newHeight = ((float)deviceWidth / deviceHeight) / ((float)setWidth / setHeight); // 새로운 높이
                  Camera.main.rect = new Rect(0f, (1f - newHeight) / 2f, 1f, newHeight); // 새로운 Rect 적용
            }

            StartCoroutine(CheckResolution());
      }

      protected virtual IEnumerator CheckResolution()
      {
            WaitForSeconds waitSeconds = new WaitForSeconds(0.5f);
            while (deviceWidth == Screen.width || deviceHeight == Screen.height) {
                  yield return waitSeconds;
            }
            SetResolutionCamera(1280, 720);
      }

      protected virtual void OnDisable()
      {
            StopCoroutine(nameof(CheckResolution));
      }
}
