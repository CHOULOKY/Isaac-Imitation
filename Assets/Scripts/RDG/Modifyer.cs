using ExitGames.Client.Photon;
using ObstacleSpace;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class Modifyer : MonoBehaviour, IOnEventCallback
{
      private RoomTemplates templates;

      private List<int> directionList;


      private PhotonView photonView;

      private void Awake()
      {
            photonView = GetComponent<PhotonView>();

            templates = FindAnyObjectByType<RoomTemplates>();
            //templates = this.transform.parent.parent.GetComponent<RoomTemplates>();

            directionList = new List<int>();
      }

      private void OnTriggerEnter2D(Collider2D other)
      {
            if (templates.createdRooms) return;

            if (other.CompareTag("SpawnPoint")) {
                  directionList.Add(other.GetComponent<RoomSpawner>().openingDirection);
            }
      }

      public void RefreshRoom()
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
                        created.transform.parent = templates.transform;
                        int roomIndex = templates.rooms.IndexOf(this.transform.parent.gameObject);
                        templates.rooms[roomIndex] = created;
                        created.GetComponentInChildren<Modifyer>()
                              .SendFunctionExecution(nameof(SetRoomParentNetworked), new object[] {
                              created.GetComponent<PhotonView>().ViewID, templates.GetComponent<PhotonView>().ViewID, roomIndex, false });

                        Destroy(this.transform.parent.gameObject); // Room
                        //// 먼저 자식 포톤뷰 오브젝트를 제거한 다음, 부모(본인) 제거
                        //PhotonView roomPV = this.transform.parent.gameObject.GetComponent<PhotonView>();
                        //foreach (PhotonView PV in roomPV.GetComponentsInChildren<PhotonView>()) {
                        //      if (PV.gameObject == roomPV.gameObject) continue;
                        //      if (!PV.IsMine && PhotonNetwork.IsMasterClient) {
                        //            //PV.TransferOwnership(PhotonNetwork.LocalPlayer);
                        //            PV.RequestOwnership();
                        //      }
                        //      PhotonNetwork.Destroy(PV.gameObject);
                        //}
                        ////Debug.Log($"{transform.parent.gameObject.name} + " +
                        ////      $"{transform.parent.gameObject.GetComponent<PhotonView>().ViewID} +" +
                        ////      $"{transform.parent.gameObject.GetComponent<PhotonView>().Owner}");
                        //if (!roomPV.IsMine && PhotonNetwork.IsMasterClient) {
                        //      //roomPV.TransferOwnership(PhotonNetwork.LocalPlayer);
                        //      roomPV.RequestOwnership();
                        //}
                        //PhotonNetwork.Destroy(this.transform.parent.gameObject);
                        break;
                  }
            }
      }

      
      private void SetRoomParentNetworked(int childViewID, int parentViewID, int roomIndex = 0, bool isSpecialRoom = false)
      {
            //if (!transform.parent.GetComponent<AddRoom>().isSpecialRoom) return;

            PhotonView childView = PhotonView.Find(childViewID);
            PhotonView parentView = PhotonView.Find(parentViewID);

            //Debug.Log(childView.name + " + " + roomIndex);
            if (childView != null && parentView != null) {
                  childView.transform.parent = parentView.transform;
                  if (!isSpecialRoom) {
                        //Debug.Log(childView.name + " + " + roomIndex);
                        //templates.rooms[templates.rooms.IndexOf(this.transform.parent.gameObject)] = roomView.gameObject;
                        templates.rooms[roomIndex] = childView.gameObject;
                  }
            }
            else {
                  Debug.LogWarning($"PhotonView with ID {childViewID} or {parentViewID} has already been destroyed.");
                  return;
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
            created.transform.parent = thisRoom.transform;
            SendFunctionExecution(nameof(SetRoomParentNetworked), new object[] {
                              created.GetComponent<PhotonView>().ViewID, thisRoom.GetComponent<PhotonView>().ViewID, -1, true }); // -1 is null

            //transform.parent.transform = Current Room's Transform
            foreach (Transform target in transform.parent.GetComponentsInChildren<Transform>(true)) {
                  if (target.name == "Monsters" || target.name == "Obstacles") {
                        ////Debug.LogError(target.name);
                        //if (!target.gameObject.activeSelf) target.gameObject.SetActive(true);
                        //foreach (PhotonView pv in target.GetComponentsInChildren<PhotonView>(true)) {
                        //      //Debug.LogError(pv.gameObject.name);
                        //      PhotonNetwork.Destroy(pv.gameObject); // Special Room을 위해 몬스터들 삭제
                        //}
                        ////Debug.LogError(target.gameObject.name);
                        //Destroy(target.gameObject);
                        target.gameObject.SetActive(false);
                  }
            }
            SendFunctionExecution(nameof(DestroyForSpecial));
      }

      // 이벤트 수신 시 실행하는 함수
      private void DestroyForSpecial()
      {
            if (!transform.parent.GetComponent<AddRoom>().IsSpecialRoom) return;

            foreach (Transform target in transform.parent.GetComponentsInChildren<Transform>(true)) {
                  if (target.gameObject.name == "Monsters" || target.gameObject.name == "Obstacles") {
                        //if (!target.gameObject.activeSelf) target.gameObject.SetActive(true);
                        ////Debug.LogError(target.name + " + " +  target.gameObject.activeSelf);
                        //if (PhotonNetwork.IsMasterClient) {
                        //      while (target.GetComponentInChildren<PhotonView>(true)) {
                        //            foreach (PhotonView pv in target.GetComponentsInChildren<PhotonView>(true)) {
                        //                  if (!pv.IsMine) pv.RequestOwnership(); // 소유권 요청
                        //                  PhotonNetwork.Destroy(pv.gameObject); // Special Room을 위해 몬스터들 삭제
                        //            }
                        //            yield return null;
                        //      }
                        //}
                        //while (target.GetComponentInChildren<PhotonView>(true)) {
                        //      yield return null;
                        //}
                        //Destroy(target.gameObject);
                        target.gameObject.SetActive(false);
                  }
            }
      }



      // 이벤트 코드 정의
      const byte ExecuteFunctionEvent = 1;
      public void SendFunctionExecution(string functionName, object[] parameters = null)
      {
            // 1. 이벤트 데이터 생성
            object[] eventData = { functionName, parameters };

            //RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
            //if (functionName == nameof(SetRoomParentNetworked)) {
            //      raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
            //}
            //else if (functionName == nameof(DestroyForSpecial)) {
            //      raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            //}

            // 2. 이벤트 전송
            PhotonNetwork.RaiseEvent(
                ExecuteFunctionEvent,          // 이벤트 코드
                eventData,                     // 이벤트 데이터
                new RaiseEventOptions { Receivers = ReceiverGroup.Others }, // 수신 대상
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
                        if (parameters.Length == 4) {
                              SetRoomParentNetworked((int)parameters[0], (int)parameters[1], (int)parameters[2], (bool)parameters[3]);
                        }
                  }
                  else if (functionName == nameof(DestroyForSpecial)) {
                        DestroyForSpecial();
                        //StartCoroutine(DestroyForSpecial());
                  }
            }
      }


      private void OnEnable()
      {
            PhotonNetwork.AddCallbackTarget(this);
      }
      private bool isRemoveCallback = false;
      private void Update()
      {
            if (templates.RefreshedRooms && !isRemoveCallback) {
                  isRemoveCallback = true;
                  PhotonNetwork.RemoveCallbackTarget(this);
            }
      }
      private void OnDestroy()
      {
            PhotonNetwork.RemoveCallbackTarget(this);
      }
}
