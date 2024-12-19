using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public enum RoomType { Gold, Boss }
public class RoomTemplates : MonoBehaviour
{
      public GameObject[] allRooms;

      public GameObject[] bottomRooms;
      public GameObject[] topRooms;
      public GameObject[] leftRooms;
      public GameObject[] rightRooms;

      public List<GameObject> rooms;
      public GameObject closedRoom;

      public int maxRoomCount = 10;
      public int minRoomCount = 5;

      public float waitTime = 2f;

      public bool createdRooms = false;
      //private bool createdRooms = false;
      //public bool CreatedRooms
      //{
      //      get => createdRooms;
      //      set {
      //            if (createdRooms != value) {
      //                  createdRooms = value;
      //                  photonView.RPC(nameof(RPC_SetCreatedRooms), RpcTarget.OthersBuffered, value);
      //            }
      //      }
      //}
      //[PunRPC]
      //private void RPC_SetCreatedRooms(bool value)
      //{
      //      createdRooms = value;
      //}

      private bool refreshedRooms = false;
      public bool RefreshedRooms
      {
            get => refreshedRooms;
            set {
                  if (refreshedRooms != value) {
                        refreshedRooms = value;
                        photonView.RPC(nameof(RPC_SetRefreshedRooms), RpcTarget.OthersBuffered, value);
                  }
            }
      }
      [PunRPC]
      private void RPC_SetRefreshedRooms(bool value)
      {
            refreshedRooms = value;
      }

      [Header("Doors")]
      public GameObject bossDoor;
      public GameObject goldDoor;
      public GameObject exitDoor;

      [Header("Rooms")]
      public GameObject GoldRoomSet;
      public GameObject BossRoomSet;

      [Header("Props")]
      public GameObject prop;


      private PhotonView photonView;
      private void Awake()
      {
            photonView = GetComponent<PhotonView>();
      }


      private void Update()
      {
            // 마스터 클라이언트만 실행 (RDG 진행)
            if (!PhotonNetwork.IsMasterClient) {
                  return;
            }

            // Test code
            //if (Input.GetKeyDown(KeyCode.Escape)) {
            //      SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            //}

            if (waitTime <= 0 && createdRooms == false) {
                  createdRooms = true;
                  //RefreshRooms();
                  photonView.RPC(nameof(RefreshRooms), RpcTarget.AllBuffered, rooms.Count);
            }
            else if (rooms.Count >= minRoomCount && waitTime > 0) {
                  waitTime -= Time.deltaTime;
            }
      }

      [PunRPC]
      private void RefreshRooms(int roomsCount)
      {
            if (!PhotonNetwork.IsMasterClient) {
                  rooms = new List<GameObject>(roomsCount);
                  for (int i = 0; i < roomsCount; i++) {
                        rooms.Add(null); // 빈 슬롯 추가
                  }
                  rooms[0] = GetComponentsInChildren<AddRoom>()[0].gameObject; // 초기화

                  // 클라이언트: rooms 리스트 초기화 완료 시 시그널
                  Hashtable initRoomsTable = new Hashtable { { "InitRooms", true } };
                  PhotonNetwork.LocalPlayer.SetCustomProperties(initRoomsTable);
            }
            StartCoroutine(RefreshRoomsRoutine());
      }
      private IEnumerator RefreshRoomsRoutine()
      {
            if (PhotonNetwork.IsMasterClient) {
                  // 마스터: 모든 클라이언트 rooms 리스트 초기화 완료 시 대기 해제
                  foreach (var player in PhotonNetwork.PlayerList) {
                        if (player.IsMasterClient == PhotonNetwork.IsMasterClient) continue;
                        yield return new WaitUntil(()
                              => (player.CustomProperties.ContainsKey("InitRooms") && (bool)player.CustomProperties["InitRooms"]));
                        break;
                  }

                  for (int i = 1; i < rooms.Count; i++) {
                        Modifyer modifyer = rooms[i].GetComponentInChildren<Modifyer>();
                        if (modifyer != null) modifyer.RefreshRoom();
                  }

                  // 마스터: RefreshedRoom 완료 시 방에 시그널
                  Hashtable refreshProps = new Hashtable { { "Refreshed", true } };
                  PhotonNetwork.CurrentRoom.SetCustomProperties(refreshProps);
            }
            // 완료 시그널 받으면 대기 해제
            yield return new WaitUntil(() =>
                  PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("Refreshed")
                  && (bool)PhotonNetwork.CurrentRoom.CustomProperties["Refreshed"]);

            foreach (Door door in rooms[^1].GetComponentsInChildren<Door>()) {
                  if (door.doorDirection == 0) continue;
                  else {
                        StartCoroutine(door.ChangeToSelectedDoorCoroutine(bossDoor));
                        door.GetComponentInParent<AddRoom>().IsSpecialRoom = true;
                        if (PhotonNetwork.IsMasterClient) {
                              door.GetComponentInParent<AddRoom>()
                                    .GetComponentInChildren<Modifyer>().SetSpecialRoom(RoomType.Boss);
                              //CreateSpecialRoom(door.GetComponentInParent<AddRoom>().gameObject, RoomType.Boss);
                        }
                        break;
                  }
            }

            for (int i = 1; i < (rooms.Count < 5 ? rooms.Count : 5); i++) {
                  // 문이 한 개인 방이면
                  if (rooms[i].GetComponentsInChildren<Door>().Length == 2) {
                        foreach (Door door in rooms[i].GetComponentsInChildren<Door>()) {
                              if (door.doorDirection != 0) {
                                    StartCoroutine(door.ChangeToSelectedDoorCoroutine(goldDoor));
                                    door.GetComponentInParent<AddRoom>().IsSpecialRoom = true;
                                    if (PhotonNetwork.IsMasterClient) {
                                          door.GetComponentInParent<AddRoom>()
                                                .GetComponentInChildren<Modifyer>().SetSpecialRoom(RoomType.Gold);
                                          //CreateSpecialRoom(door.GetComponentInParent<AddRoom>().gameObject, RoomType.Gold);
                                    }
                                    break;
                              }
                        }
                        break;
                  }
            }

            if (PhotonNetwork.IsMasterClient) {
                  RefreshedRooms = true;
            }
      }

      //public void CreateSpecialRoom(GameObject room, RoomType roomType)
      //{
      //      // 특수 방 설정
      //      string specialName = default;
      //      switch (roomType) {
      //            case RoomType.Gold:
      //                  room.GetComponent<AddRoom>().IsClear = true;
      //                  specialName = GoldRoomSet.name;
      //                  break;
      //            case RoomType.Boss:
      //                  specialName = BossRoomSet.name;
      //                  break;
      //      }

      //      GameObject specialRoom = PhotonNetwork.Instantiate(specialName + " Variant", room.transform.position, Quaternion.identity);
      //      specialRoom.transform.parent = room.transform;
      //      photonView.RPC(nameof(RPC_SetParent), RpcTarget.OthersBuffered,
      //            specialRoom.GetComponent<PhotonView>().ViewID, room.GetComponent<PhotonView>().ViewID);

      //      // 몬스터 및 장애물 제거
      //      foreach (Transform child in room.transform) {
      //            if (child.name == "Monsters" || child.name == "Obstacles") {
      //                  PhotonView[] childViews = child.GetComponentsInChildren<PhotonView>(true);
      //                  foreach (PhotonView view in childViews) {
      //                        if (view != null) PhotonNetwork.Destroy(view.gameObject);
      //                  }
      //                  Destroy(child.gameObject);
      //            }
      //      }
      //}
      //[PunRPC]
      //private void RPC_SetParent(int viewID, int parentViewID)
      //{
      //      PhotonView target = PhotonView.Find(viewID);
      //      PhotonView parent = PhotonView.Find(parentViewID);
      //      if (target != null && parent != null) {
      //            target.transform.parent = parent.transform;
      //      }
      //}


      private void OnDisable()
      {
            if (!PhotonNetwork.IsMasterClient) return;

            RefreshedRooms = false;
      }
}
