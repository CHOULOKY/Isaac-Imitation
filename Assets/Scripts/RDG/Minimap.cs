using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class Minimap : MonoBehaviour, IPunObservable
{
      public GameObject[] miniRooms;
      public List<GameObject> miniRoomsList;
      public GameObject boss;

      public RoomTemplates templates;
      private bool isSetMinimap = false;

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
            if (PhotonNetwork.IsMasterClient) {
                  transform.parent.position = mainCamera.transform.position + offsetFromMain;
            }

            if (templates.RefreshedRooms && !isSetMinimap) {
                  isSetMinimap = true;

                  for (int i = 0; i < templates.rooms.Count; i++) {
                        string roomName = templates.rooms[i].name.Replace("(Clone)", "").Replace(" Variant", "");
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

                  Hashtable miniProps = new Hashtable { { "SetMinimap", true } };
                  PhotonNetwork.LocalPlayer.SetCustomProperties(miniProps);
            }
      }

      private float deviceWidth, deviceHeight;
      private void SetResolutionaMinimap()
      {
            deviceWidth = SetResolution.deviceWidth;
            deviceHeight = SetResolution.deviceHeight;

            // �̴ϸ� ī�޶��� �⺻ ���� ����
            float miniMapX = 0.74f;
            float miniMapY = 0.74f;
            float miniMapW = 0.25f;
            float miniMapH = 0.25f;

            // ���� ī�޶��� Rect ������ ������ �̴ϸ� ������ ����
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

      public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
      {
            //if (stream.IsWriting) {
            //      stream.SendNext(transform.parent.position);
            //}
            //else {
            //      transform.parent.position = (Vector3)stream.ReceiveNext();
            //}
      }
}
