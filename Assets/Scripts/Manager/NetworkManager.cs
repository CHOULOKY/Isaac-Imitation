using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviourPunCallbacks
{
      private static NetworkManager instance;
      public static NetworkManager Instance
      {
            get {
                  if (instance == null) return null;
                  return instance;
            }
      }


      public bool isServerAccess = false;
      public bool isOtherAccess = false;

      private void Awake()
      {
            // Singletone
            if (instance != null && instance != this) {
                  Destroy(gameObject); // 중복된 인스턴스는 삭제
                  return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject); // 싱글톤 유지

            // Photon
            if (!PhotonNetwork.IsConnected) InitializePhotonNetwork();
      }

      private void InitializePhotonNetwork()
      {
            PhotonNetwork.SendRate = 60;
            PhotonNetwork.SerializationRate = 30;
      }

      private void Start() => Connect();


      public void Connect()
      {
            if (!PhotonNetwork.IsConnected) PhotonNetwork.ConnectUsingSettings();
            else Debug.LogWarning("Network: 이미 서버에 접속됐거나 접속하고 있습니다.");
      }

      public override void OnConnectedToMaster()
      {
            // 랜덤 방 참가를 시도
            PhotonNetwork.JoinRandomRoom(null, 2);
      }

      // 랜덤 방 참가 실패 시 새로운 방 생성
      public override void OnJoinRandomFailed(short returnCode, string message)
      {
            Debug.LogWarning("Network: 랜덤 방 참가 실패. 새 방 생성 중...");
            RoomOptions roomOptions = new RoomOptions { MaxPlayers = 2 };
            PhotonNetwork.CreateRoom(null, roomOptions);
      }

      public override void OnJoinedRoom()
      {
            if(PhotonNetwork.IsMasterClient) Debug.Log("Network: 새 방 생성 성공.");
            else {
                  Debug.Log("Network: 랜덤 방 참가 성공.");
                  CheckRoomFull();
            }
            isServerAccess = true;
      }

      // 방에 새 플레이어가 들어왔을 때 호출되는 함수
      public override void OnPlayerEnteredRoom(Player newPlayer)
      {
            base.OnPlayerEnteredRoom(newPlayer);

            CheckRoomFull();
      }


      private void CheckRoomFull()
      {
            // 방에 있는 플레이어 수 확인
            if (PhotonNetwork.PlayerList.Length == 2) {
                  Debug.Log("Network: 플레이어 2명이 모두 들어왔습니다.");
                  isOtherAccess = true;
            }
      }


      public override void OnPlayerLeftRoom(Player otherPlayer)
      {
            base.OnPlayerLeftRoom(otherPlayer);

            PhotonNetwork.Disconnect();
            StartCoroutine(GameManager.Instance.OnPlayerLeftRoom());
      }

      public override void OnDisconnected(DisconnectCause cause)
      {
            //GameManager.ExitGame();
      }
}
