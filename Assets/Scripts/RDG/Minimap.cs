using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minimap : MonoBehaviour
{
      public GameObject[] miniRooms;
      public List<GameObject> miniRoomsList;
      public GameObject boss;

      public RoomTemplates templates;
      private bool isSetMinimap;

      public MainCamera mainCamera;
      private Vector3 offsetFromMain;

      private void Awake()
      {
            miniRoomsList = new List<GameObject>();

            if (!templates) {
                  templates = FindAnyObjectByType<RoomTemplates>();
            }

            mainCamera = mainCamera != null ? mainCamera : Camera.main.GetComponent<MainCamera>();
            offsetFromMain = transform.parent.position - Camera.main.transform.position;
      }

      private void OnEnable()
      {
            SetResolutionaMinimap();
      }

      private void Update()
      {
            transform.parent.position = mainCamera.transform.position + offsetFromMain;

            if (templates.refreshedRooms && !isSetMinimap) {
                  isSetMinimap = true;

                  for (int i = 0; i < templates.rooms.Count; i++) {
                        string roomName = templates.rooms[i].name.Replace("(Clone)", "");
                        for (int j = 0; j < miniRooms.Length; j++) {
                              if (roomName == miniRooms[j].name) {
                                    float roomX = this.transform.position.x + templates.rooms[i].transform.position.x / 40 * 0.5f;
                                    float roomY = this.transform.position.y + templates.rooms[i].transform.position.y / 40 * 0.5f;
                                    miniRoomsList.Add(Instantiate(miniRooms[j], new Vector2(roomX, roomY), miniRooms[j].transform.rotation, this.transform));
                                    break;
                              }
                        }
                  }
                  Instantiate(boss, miniRoomsList[^1].transform.position, Quaternion.identity, miniRoomsList[^1].transform);
            }
      }

      private float deviceWidth, deviceHeight;
      private void SetResolutionaMinimap()
      {
            deviceWidth = SetResolution.deviceWidth;
            deviceHeight = SetResolution.deviceHeight;

            // 미니맵 카메라의 기본 비율 설정
            float miniMapX = 0.74f;
            float miniMapY = 0.74f;
            float miniMapW = 0.25f;
            float miniMapH = 0.25f;

            // 메인 카메라의 Rect 비율을 참고해 미니맵 비율도 맞춤
            Rect mainRect = Camera.main.rect;
            this.transform.parent.GetComponent<Camera>().rect = new Rect(
                mainRect.x + miniMapX * mainRect.width,
                mainRect.y + miniMapY * mainRect.height,
                miniMapW * mainRect.width,
                miniMapH * mainRect.height
            );

            StartCoroutine(CheckResolution());
      }

      protected virtual IEnumerator CheckResolution()
      {
            WaitForSeconds waitSeconds = new WaitForSeconds(0.5f);
            while (deviceWidth == SetResolution.deviceWidth || deviceHeight == SetResolution.deviceHeight) {
                  yield return waitSeconds;
            }
            SetResolutionaMinimap();
      }

      private void OnDisable()
      {
            StopCoroutine(nameof(CheckResolution));
      }
}
