using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Modifyer : MonoBehaviour, IOnEventCallback
{
      private RoomTemplates templates;

      private List<int> directionList;


      private void Awake()
      {
            templates = FindAnyObjectByType<RoomTemplates>();
            //templates = this.transform.parent.parent.GetComponent<RoomTemplates>();

            directionList = new List<int>();
      }

      private void OnTriggerEnter2D(Collider2D other)
      {
            // 마스터 클라이언트만 실행 (RDG 진행)
            if (!PhotonNetwork.IsMasterClient) return;

            if (templates.CreatedRooms) return;

            if (other.CompareTag("SpawnPoint")) {
                  directionList.Add(other.GetComponent<RoomSpawner>().openingDirection);
            }
      }

      public void RefreshRoom(int roomIndex)
      {
            string thisName = this.transform.parent.name;
            if (!thisName.Contains("Closed")) {
                  if (thisName.Contains('B')) directionList.Add(1);
                  if (thisName.Contains('T')) directionList.Add(2);
                  if (thisName.Contains('L')) directionList.Add(3);
                  if (thisName.Contains('R')) directionList.Add(4);
                  directionList = directionList.Distinct().OrderBy(n => n).ToList(); // 중복 제거 & 오름차순 정렬
            }

            char dirB = default;
            string roomName = "";
            foreach (int direction in directionList) {
                  switch (direction) {
                        case 1:
                              dirB = 'B';
                              break;
                        case 2:
                              roomName += "T";
                              break;
                        case 3:
                              roomName += "L";
                              break;
                        case 4:
                              roomName += "R";
                              break;
                  }
            }
            roomName += dirB == default ? "" : dirB;

            for (int i = 0; i < templates.allRooms.Length; i++) {
                  if (templates.allRooms[i].name.Equals(roomName)) {
                        //templates.rooms[templates.rooms.IndexOf(this.transform.parent.gameObject)] =
                        //    Instantiate(templates.allRooms[i], transform.position, templates.allRooms[i].transform.rotation, templates.transform);
                        GameObject created = PhotonNetwork.Instantiate(templates.allRooms[i].name + " Variant",
                              transform.position, templates.allRooms[i].transform.rotation);
                        //created.transform.parent = templates.transform;
                        //templates.rooms[templates.rooms.IndexOf(this.transform.parent.gameObject)] = created;
                        SendFunctionExecution(nameof(SetRoomParentNetworked),
                              new object[] {created.GetComponent<PhotonView>().ViewID,
                                    templates.GetComponent<PhotonView>().ViewID, roomIndex, false });

                        //Destroy(this.transform.parent.gameObject); // Room
                        // 먼저 자식 포톤뷰 오브젝트를 제거한 다음, 부모(본인) 제거
                        foreach (PhotonView PV in this.transform.parent.gameObject.GetComponentsInChildren<PhotonView>()) {
                              if (PV.gameObject == this.transform.parent.gameObject) continue;
                              PhotonNetwork.Destroy(PV.gameObject);
                        }
                        PhotonNetwork.Destroy(this.transform.parent.gameObject);
                        break;
                  }
            }
      }


      private void SetRoomParentNetworked(int roomViewID, int parentViewID, int roomIndex = 0, bool isSpecialRoom = false)
      {
            PhotonView roomView = PhotonView.Find(roomViewID);
            PhotonView parentView = PhotonView.Find(parentViewID);

            if (roomView != null && parentView != null) {
                  roomView.transform.parent = parentView.transform;
                  if (!isSpecialRoom) {
                        //templates.rooms[templates.rooms.IndexOf(this.transform.parent.gameObject)] = roomView.gameObject;
                        templates.rooms[roomIndex] = roomView.gameObject;
                  }
            }
      }



      public void SetSpecialRoom(RoomType roomType)
      {
            GameObject created = null;

            AddRoom thisRoom = GetComponentInParent<AddRoom>();
            switch (roomType) {
                  case RoomType.Gold:
                        thisRoom.IsClear = true;
                        //Instantiate(templates.GoldRoomSet, thisRoom.transform.position, Quaternion.identity, thisRoom.transform);
                        created = PhotonNetwork.Instantiate(templates.GoldRoomSet.name + " Variant",
                              thisRoom.transform.position, Quaternion.identity);
                        break;
                  case RoomType.Boss:
                        //Instantiate(templates.BossRoomSet, thisRoom.transform.position, Quaternion.identity, thisRoom.transform);
                        created = PhotonNetwork.Instantiate(templates.BossRoomSet.name + " Variant",
                              thisRoom.transform.position, Quaternion.identity);
                        break;
            }
            //created.transform.parent = thisRoom.transform;
            SendFunctionExecution(nameof(SetRoomParentNetworked), new object[] {
                              created.GetComponent<PhotonView>().ViewID, thisRoom.GetComponent<PhotonView>().ViewID, true });

            // transform.parent.transform = Current Room's Transform
            foreach (Transform target in transform.parent.GetComponentsInChildren<Transform>(true)) {
                  if (target.name == "Monsters" || target.name == "Obstacles") {
                        foreach (PhotonView pv in target.GetComponentsInChildren<PhotonView>()) {
                              PhotonNetwork.Destroy(pv.gameObject); // Special Room을 위해 몬스터들 삭제
                        }
                        //Destroy(target.gameObject);
                  }
            }
            SendFunctionExecution(nameof(DestroyForSpecial));
      }

      // 이벤트 수신 시 실행하는 함수
      private void DestroyForSpecial()
      {
            foreach (Transform target in transform.parent.GetComponentsInChildren<Transform>(true)) {
                  if (target.name == "Monsters" || target.name == "Obstacles") {
                        Destroy(target.gameObject);
                  }
            }
      }



      // 이벤트 코드 정의
      const byte ExecuteFunctionEvent = 1;
      public void SendFunctionExecution(string functionName, object[] parameters = null)
      {
            // 1. 이벤트 데이터 생성
            object[] eventData = { functionName, parameters };

            // 2. 이벤트 전송
            PhotonNetwork.RaiseEvent(
                ExecuteFunctionEvent,          // 이벤트 코드
                eventData,                     // 이벤트 데이터
                new RaiseEventOptions { Receivers = ReceiverGroup.All }, // 수신 대상
                SendOptions.SendReliable       // 전송 옵션
            );

            //Debug.Log($"Sent event to execute function: {functionName}");
      }
      public void OnEvent(EventData photonEvent)
      {
            if (photonEvent.Code == ExecuteFunctionEvent) {
                  // 1. 이벤트 데이터 수신
                  object[] data = (object[])photonEvent.CustomData;
                  string functionName = (string)data[0];
                  object[] parameters = (object[])data[1];

                  // 2. 해당 함수 실행
                  //Invoke(functionName, parameters);
                  if (functionName == nameof(SetRoomParentNetworked)) {
                        SetRoomParentNetworked((int)parameters[0], (int)parameters[1], (int)parameters[2]);
                  }
                  else {
                        Invoke(functionName, 0);
                  }
            }
      }


      private void OnEnable()
      {
            PhotonNetwork.AddCallbackTarget(this);
      }
      private void Update()
      {
            if (templates.RefreshedRooms) {
                  PhotonNetwork.RemoveCallbackTarget(this);
            }
      }
      private void OnDestroy()
      {
            PhotonNetwork.RemoveCallbackTarget(this);
      }
}
