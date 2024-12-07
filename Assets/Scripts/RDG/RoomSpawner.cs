using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class RoomSpawner : MonoBehaviour//, IOnEventCallback
{
      public int openingDirection;
      // 1 --> need bottom door
      // 2 --> need top door
      // 3 --> need left door
      // 4 --> need right door


      private RoomTemplates templates;
      private int rand;
      public bool spawned = false;

      public float waitTime = 4f;


      private void Awake()
      {
            templates = FindAnyObjectByType<RoomTemplates>();
            //templates = this.transform.parent.parent.parent.GetComponent<RoomTemplates>();
      }

      private void Start()
      {
            Destroy(gameObject, waitTime);

            // 마스터 클라이언트만 실행 (RDG 진행)
            if (PhotonNetwork.IsMasterClient) {
                  Invoke(nameof(Spawn), 0.1f);
            }
      }


      private void Spawn()
      {
            if (templates.createdRooms) return;

            GameObject created = null;
            if (!spawned) {
                  if (templates.rooms.IndexOf(transform.parent.parent.gameObject) == 0 &&
                      openingDirection == transform.parent.parent.GetComponent<AddRoom>().goldRoomDirection) {
                        created = CreateRoomWithDoor(openingDirection, 0);
                  }
                  else {
                        int randStart = templates.rooms.Count < templates.minRoomCount ? 1 : 0;
                        int randIndex = Random.Range(randStart, GetRoomArrayLength(openingDirection));
                        randIndex = templates.rooms.Count > templates.maxRoomCount ? 0 : randIndex;
                        created = CreateRoomWithDoor(openingDirection, randIndex);
                  }

                  if (created != null) {
                        //SendFunctionExecution(nameof(SetRoomParentNetworked), new object[] { 
                        //      created.GetComponent<PhotonView>().ViewID, templates.GetComponent<PhotonView>().ViewID });
                        spawned = true;
                  }
            }
      }

      private GameObject CreateRoomWithDoor(int direction, int index)
      {
            //switch (direction) {
            //      case 1:
            //            return PhotonNetwork.Instantiate(templates.bottomRooms[index].name + " Variant",
            //                transform.position, templates.bottomRooms[index].transform.rotation);
            //      case 2:
            //            return PhotonNetwork.Instantiate(templates.topRooms[index].name + " Variant",
            //                transform.position, templates.topRooms[index].transform.rotation);
            //      case 3:
            //            return PhotonNetwork.Instantiate(templates.leftRooms[index].name + " Variant",
            //                transform.position, templates.leftRooms[index].transform.rotation);
            //      case 4:
            //            return PhotonNetwork.Instantiate(templates.rightRooms[index].name + " Variant",
            //                transform.position, templates.rightRooms[index].transform.rotation);
            //}
            switch (direction) {
                  case 1:
                        return Instantiate(templates.bottomRooms[index], 
                              transform.position, templates.bottomRooms[index].transform.rotation, templates.transform);
                  case 2:
                        return Instantiate(templates.topRooms[index],
                            transform.position, templates.topRooms[index].transform.rotation, templates.transform);
                  case 3:
                        return Instantiate(templates.leftRooms[index],
                            transform.position, templates.leftRooms[index].transform.rotation, templates.transform);
                  case 4:
                        return Instantiate(templates.rightRooms[index],
                            transform.position, templates.rightRooms[index].transform.rotation, templates.transform);
            }
            return null;
      }

      private int GetRoomArrayLength(int direction)
      {
            return direction switch
            {
                  1 => templates.bottomRooms.Length,
                  2 => templates.topRooms.Length,
                  3 => templates.leftRooms.Length,
                  4 => templates.rightRooms.Length,
                  _ => 0
            };
      }


      private void OnTriggerEnter2D(Collider2D other)
      {
            // 마스터 클라이언트만 실행
            if (!PhotonNetwork.IsMasterClient) return;

            if (templates.createdRooms) return;

            if (other.CompareTag("SpawnPoint")) {
                  if (!other.GetComponent<RoomSpawner>().spawned && !spawned) {
                        //GameObject created = PhotonNetwork.Instantiate(templates.closedRoom.name + " Variant",
                        //    transform.position, templates.closedRoom.transform.rotation);
                        //SendFunctionExecution(nameof(SetRoomParentNetworked), new object[] {
                        //      created.GetComponent<PhotonView>().ViewID, templates.GetComponent<PhotonView>().ViewID });
                        Instantiate(templates.closedRoom,
                              transform.position, templates.closedRoom.transform.rotation, templates.transform);
                        Destroy(this.gameObject);
                  }

                  spawned = true;
            }
      }



      //private void SetRoomParentNetworked(int roomViewID, int parentViewID)
      //{
      //      PhotonView roomView = PhotonView.Find(roomViewID);
      //      PhotonView parentView = PhotonView.Find(parentViewID);

      //      if (roomView != null && parentView != null) {
      //            roomView.transform.parent = parentView.transform;
      //      }
      //}


      //// 이벤트 코드 정의
      //const byte ExecuteFunctionEvent = 1;
      //public void SendFunctionExecution(string functionName, object[] parameters = null)
      //{
      //      // 1. 이벤트 데이터 생성
      //      object[] eventData = { functionName, parameters };

      //      // 2. 이벤트 전송
      //      PhotonNetwork.RaiseEvent(
      //          ExecuteFunctionEvent,          // 이벤트 코드
      //          eventData,                     // 이벤트 데이터
      //          new RaiseEventOptions { Receivers = ReceiverGroup.All }, // 수신 대상
      //          SendOptions.SendReliable       // 전송 옵션
      //      );

      //      //Debug.Log($"Sent event to execute function: {functionName}");
      //}
      //public void OnEvent(EventData photonEvent)
      //{
      //      if (photonEvent.Code == ExecuteFunctionEvent) {
      //            // 1. 이벤트 데이터 수신
      //            object[] data = (object[])photonEvent.CustomData;
      //            string functionName = (string)data[0];
      //            object[] parameters = (object[])data[1];

      //            // 2. 해당 함수 실행
      //            if (functionName == nameof(SetRoomParentNetworked)) {
      //                  SetRoomParentNetworked((int)parameters[0], (int)parameters[1]);
      //            }
      //      }
      //}


      //private void OnEnable()
      //{
      //      PhotonNetwork.AddCallbackTarget(this);
      //}
      //private void Update()
      //{
      //      if (templates.RefreshedRooms) {
      //            PhotonNetwork.RemoveCallbackTarget(this);
      //      }
      //}
      //private void OnDestroy()
      //{
      //      PhotonNetwork.RemoveCallbackTarget(this);
      //}
}
