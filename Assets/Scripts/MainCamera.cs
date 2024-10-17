using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : MonoBehaviour
{
    public IsaacBody Isaac;
    public float lerpSpeed;

    private Vector3 offset;
    private Vector3 targetPos;
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

    private void LateUpdate()
    {
        targetPos = Isaac.transform.position + offset;
        targetPos.x = Mathf.Clamp(targetPos.x, minBoundary.x, maxBoundary.x);
        targetPos.y = Mathf.Clamp(targetPos.y, minBoundary.y, maxBoundary.y);
        transform.position = Vector3.Lerp(transform.position, targetPos, lerpSpeed);
    }

    private void SetResolution()
    {
        int setWidth = 1280; // 사용자 설정 너비
        int setHeight = 720; // 사용자 설정 높이

        int deviceWidth = Screen.width; // 기기 너비 저장
        int deviceHeight = Screen.height; // 기기 높이 저장

        Screen.SetResolution(setWidth, (int)(((float)deviceHeight / deviceWidth) * setWidth), true); // SetResolution 함수 제대로 사용하기

        if ((float)setWidth / setHeight < (float)deviceWidth / deviceHeight) // 기기의 해상도 비가 더 큰 경우
        {
            float newWidth = ((float)setWidth / setHeight) / ((float)deviceWidth / deviceHeight); // 새로운 너비
            Camera.main.rect = new Rect((1f - newWidth) / 2f, 0f, newWidth, 1f); // 새로운 Rect 적용
        } else // 게임의 해상도 비가 더 큰 경우
          {
            float newHeight = ((float)deviceWidth / deviceHeight) / ((float)setWidth / setHeight); // 새로운 높이
            Camera.main.rect = new Rect(0f, (1f - newHeight) / 2f, 1f, newHeight); // 새로운 Rect 적용
        }
    }
}
