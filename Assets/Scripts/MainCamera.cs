using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : MonoBehaviour
{
    public IsaacBody Isaac;
    public float lerpSpeed;

    private Vector3 offset;
    private Vector3 targetPos = Vector3.zero;
    public Vector2 maxBoundary;
    public Vector2 minBoundary;

    private void Awake()
    {
        Isaac = Isaac != null ? Isaac : FindAnyObjectByType<IsaacBody>();
    }

    private void Start()
    {
        SetResolution();

        offset = Vector3.back * 10;
        this.transform.position = Vector3.zero + offset;
    }

    private float _lerpSpeed, _distance;
    private void LateUpdate()
    {
        _distance = (transform.position - targetPos).magnitude;
        _lerpSpeed = _distance >= 10 ? 1 : lerpSpeed;

        targetPos = Isaac.transform.position + offset;
        targetPos.x = Mathf.Clamp(targetPos.x, minBoundary.x, maxBoundary.x);
        targetPos.y = Mathf.Clamp(targetPos.y, minBoundary.y, maxBoundary.y);
        transform.position = Vector3.Lerp(transform.position, targetPos, _lerpSpeed);
    }

    private void SetResolution()
    {
        int setWidth = 1280; // ����� ���� �ʺ�
        int setHeight = 720; // ����� ���� ����

        int deviceWidth = Screen.width; // ��� �ʺ� ����
        int deviceHeight = Screen.height; // ��� ���� ����

        Screen.SetResolution(setWidth, (int)(((float)deviceHeight / deviceWidth) * setWidth), true); // SetResolution �Լ� ����� ����ϱ�

        if ((float)setWidth / setHeight < (float)deviceWidth / deviceHeight) // ����� �ػ� �� �� ū ���
        {
            float newWidth = ((float)setWidth / setHeight) / ((float)deviceWidth / deviceHeight); // ���ο� �ʺ�
            Camera.main.rect = new Rect((1f - newWidth) / 2f, 0f, newWidth, 1f); // ���ο� Rect ����
        } else // ������ �ػ� �� �� ū ���
          {
            float newHeight = ((float)deviceWidth / deviceHeight) / ((float)setWidth / setHeight); // ���ο� ����
            Camera.main.rect = new Rect(0f, (1f - newHeight) / 2f, 1f, newHeight); // ���ο� Rect ����
        }
    }
}