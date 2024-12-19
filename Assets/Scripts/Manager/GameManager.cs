using ItemSpace;
using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
      private PhotonView photonView;


      public int CurrentStage = 1;
      private bool isClear = false;
      public bool IsClear
      {
            get { return isClear; }
            set {
                  if (isClear != value) {
                        //isClear = value;
                        //if (isClear) {
                        //      StageClear();
                        //      CurrentStage++;
                        //      isClear = false;
                        //}
                        photonView.RPC(nameof(RPC_SetisClear), RpcTarget.AllBuffered, value);
                  }
            }
      }
      [PunRPC]
      private void RPC_SetisClear(bool value)
      {
            isClear = value;
            if (isClear) {
                  StageClear();
                  CurrentStage++;
                  isClear = false;
            }
      }


      private static GameManager instance;
      [Header("Singletone")]
      public UIManager uiManager;
      public IsaacTearFactory isaacTearFactory;
      public MonsterTearFactory monsterTearFactory;

      public ItemFactory itemFactory;

      public RoomTemplates roomTemplates;
      public Minimap minimap;


      // All
      private void Awake()
      {
            photonView = GetComponent<PhotonView>();

            // Singletone
            instance = this;

            // Initialization
            uiManager = GetComponentInChildren<UIManager>();
            isaacTearFactory = GetComponentInChildren<IsaacTearFactory>();
            monsterTearFactory = GetComponentInChildren<MonsterTearFactory>();

            itemFactory = GetComponentInChildren<ItemFactory>();

            if(!roomTemplates) roomTemplates = FindAnyObjectByType<RoomTemplates>();
            if (!minimap) minimap = FindAnyObjectByType<Minimap>();
      }

      public static GameManager Instance
      {
            get {
                  if (instance == null) return null;
                  return instance;
            }
      }

      private void OnEnable()
      {
            if (PhotonNetwork.IsMasterClient && photonView.Owner != PhotonNetwork.LocalPlayer) {
                  photonView.RequestOwnership();
            }
      }

      private void Start()
      {
            //PhotonNetwork.AutomaticallySyncScene = true;

            Time.timeScale = 1.0f;
            StartCoroutine(GameStart());
      }

      public IEnumerator GameStart()
      {
            yield return StartCoroutine(uiManager.GameStartBefore());
            //IsaacBody player = FindObjectOfType<IsaacBody>(true);
            //player.gameObject.SetActive(true);
            photonView.RPC(nameof(ActivePlayer), RpcTarget.AllBuffered);
            
            yield return StartCoroutine(uiManager.GameStartAfter());
      }
      [PunRPC]
      private void ActivePlayer()
      {
            IsaacBody player = FindObjectOfType<IsaacBody>(true);
            player.gameObject.SetActive(true);
      }

      //[PunRPC]
      public void GameOver()
      {
            //Time.timeScale = 0;
            //uiManager.GameOver();
            // SceneManager.LoadScene(0);
            photonView.RPC(nameof(RPC_GameOver), RpcTarget.AllBuffered);
      }
      [PunRPC]
      private void RPC_GameOver()
      {
            Time.timeScale = 0;
            uiManager.GameOver();
      }

      public void StageClear()
      {
            Time.timeScale = 0;
            // Debug.Log("Stage Clear!");
            uiManager.StageClear();
      }


      public IEnumerator PlayBossCutScene()
      {
            Time.timeScale = 0;
            yield return StartCoroutine(uiManager.PlayBossCutScene());
            Time.timeScale = 1;
      }

      public IEnumerator OnPlayerLeftRoom()
      {
            Time.timeScale = 0;

            yield return StartCoroutine(uiManager.OnPlayerLeftRoom());

            SceneManager.LoadScene(0); // Loading Scene
      }


      #region For UI Button
      public void RetryGame()
      {
            SceneManager.LoadScene(1);
            //photonView.RPC(nameof(RPC_RetryGame), RpcTarget.AllBuffered);
      }
      //[PunRPC]
      //private void RPC_RetryGame()
      //{
      //      PhotonNetwork.LoadLevel("GameScene");
      //}

      public static void ExitGame()
      {
#if UNITY_EDITOR
            // 에디터 환경에서 플레이 모드 종료
            UnityEditor.EditorApplication.isPlaying = false;
#else
        // 빌드된 실행 파일에서 애플리케이션 종료
        Application.Quit();
#endif
      }
      #endregion
}
