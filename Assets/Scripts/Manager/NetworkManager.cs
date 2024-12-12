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
                  Destroy(gameObject); // �ߺ��� �ν��Ͻ��� ����
                  return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject); // �̱��� ����

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
            else Debug.LogWarning("Network: �̹� ������ ���ӵưų� �����ϰ� �ֽ��ϴ�.");
      }

      public override void OnConnectedToMaster()
      {
            // ���� �� ������ �õ�
            PhotonNetwork.JoinRandomRoom(null, 2);
      }

      // ���� �� ���� ���� �� ���ο� �� ����
      public override void OnJoinRandomFailed(short returnCode, string message)
      {
            Debug.LogWarning("Network: ���� �� ���� ����. �� �� ���� ��...");
            RoomOptions roomOptions = new RoomOptions { MaxPlayers = 2 };
            PhotonNetwork.CreateRoom(null, roomOptions);
      }

      public override void OnJoinedRoom()
      {
            if(PhotonNetwork.IsMasterClient) Debug.Log("Network: �� �� ���� ����.");
            else {
                  Debug.Log("Network: ���� �� ���� ����.");
                  CheckRoomFull();
            }
            isServerAccess = true;
      }

      // �濡 �� �÷��̾ ������ �� ȣ��Ǵ� �Լ�
      public override void OnPlayerEnteredRoom(Player newPlayer)
      {
            base.OnPlayerEnteredRoom(newPlayer);

            CheckRoomFull();
      }


      private void CheckRoomFull()
      {
            // �濡 �ִ� �÷��̾� �� Ȯ��
            if (PhotonNetwork.PlayerList.Length == 2) {
                  Debug.Log("Network: �÷��̾� 2���� ��� ���Խ��ϴ�.");
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
