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
            int setWidth = width; // ����� ���� �ʺ�
            int setHeight = height; // ����� ���� ����

            deviceWidth = Screen.width; // ��� �ʺ� ����
            deviceHeight = Screen.height; // ��� ���� ����

            Screen.SetResolution(setWidth, (int)(((float)deviceHeight / deviceWidth) * setWidth), false);

            if ((float)setWidth / setHeight < (float)deviceWidth / deviceHeight) // ����� �ػ� �� �� ū ���
            {
                  float newWidth = ((float)setWidth / setHeight) / ((float)deviceWidth / deviceHeight); // ���ο� �ʺ�
                  Camera.main.rect = new Rect((1f - newWidth) / 2f, 0f, newWidth, 1f); // ���ο� Rect ����
            }
            else // ������ �ػ� �� �� ū ���
              {
                  float newHeight = ((float)deviceWidth / deviceHeight) / ((float)setWidth / setHeight); // ���ο� ����
                  Camera.main.rect = new Rect(0f, (1f - newHeight) / 2f, 1f, newHeight); // ���ο� Rect ����
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
