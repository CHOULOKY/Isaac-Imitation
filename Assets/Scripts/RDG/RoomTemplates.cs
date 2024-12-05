using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
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

      private bool createdRooms = false;
      public bool CreatedRooms
      {
            get => createdRooms;
            set {
                  if (createdRooms != value) {
                        createdRooms = value;
                        photonView.RPC(nameof(RPC_SetCreatedRooms), RpcTarget.OthersBuffered, value);
                  }
            }
      }
      [PunRPC]
      private void RPC_SetCreatedRooms(bool value)
      {
            createdRooms = value;
      }

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
            if (!PhotonNetwork.IsMasterClient) return;

            // Test code
            if (Input.GetKeyDown(KeyCode.Escape)) {
                  SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }

            if (waitTime <= 0 && createdRooms == false) {
                  createdRooms = true;
                  //RefreshRooms();
                  photonView.RPC(nameof(RefreshRooms), RpcTarget.AllBuffered);
            }
            else if (rooms.Count >= minRoomCount && waitTime > 0) {
                  waitTime -= Time.deltaTime;
            }
      }

      [PunRPC]
      private void RefreshRooms()
      {
            StartCoroutine(RefreshRoomsRoutine());
      }
      private IEnumerator RefreshRoomsRoutine()
      {
            if (PhotonNetwork.IsMasterClient) {
                  for (int i = 1; i < rooms.Count; i++) {
                        if (rooms[i].GetComponentInChildren<Modifyer>()) {
                              rooms[i].GetComponentInChildren<Modifyer>().RefreshRoom(i);
                              yield return null;
                        }
                  }
            }

            foreach (Door door in rooms[^1].GetComponentsInChildren<Door>()) {
                  if (door.doorDirection == 0) continue;
                  else {
                        StartCoroutine(door.ChangeToSelectedDoorCoroutine(bossDoor));
                        if (PhotonNetwork.IsMasterClient) {
                              door.transform.parent.parent.GetComponentInChildren<Modifyer>()
                                    .SetSpecialRoom(RoomType.Boss);
                        }
                        break;
                  }
            }

            for (int i = 1; i < (rooms.Count < 5 ? rooms.Count : 5); i++) {
                  if (rooms[i].GetComponentsInChildren<Door>().Length == 2) {
                        foreach (Door door in rooms[i].GetComponentsInChildren<Door>()) {
                              if (door.doorDirection != 0) {
                                    StartCoroutine(door.ChangeToSelectedDoorCoroutine(goldDoor));
                                    if (PhotonNetwork.IsMasterClient) {
                                          door.transform.parent.parent.GetComponentInChildren<Modifyer>()
                                                .SetSpecialRoom(RoomType.Gold);
                                    }
                              }
                        }
                        break;
                  }
            }

            Hashtable props = new Hashtable { { "RefreshedRooms", true } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
            foreach (var player in PhotonNetwork.PlayerList) {
                  yield return new WaitUntil(()
                        => (player.CustomProperties.ContainsKey("RefreshedRooms") && (bool)player.CustomProperties["RefreshedRooms"]));
            }

            if (PhotonNetwork.IsMasterClient) {
                  refreshedRooms = true;
            }
      }


      private void OnDisable()
      {
            if (!PhotonNetwork.IsMasterClient) return;

            refreshedRooms = false;
      }
}
