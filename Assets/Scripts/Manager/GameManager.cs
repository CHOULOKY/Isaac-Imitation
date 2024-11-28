using ItemSpace;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
      private static GameManager instance;

      public int CurrentStage = 1;
      private bool isClear = false;
      public bool IsClear
      {
            get { return isClear; }
            set {
                  if (isClear != value) {
                        isClear = value;
                        if (isClear) {
                              StageClear();
                              CurrentStage++;
                              isClear = false;
                        }
                  }
            }
      }

      [Header("Singletone")]
      public UIManager uiManager;
      public IsaacTearFactory isaacTearFactory;
      public MonsterTearFactory monsterTearFactory;

      public ItemFactory itemFactory;

      public Minimap minimap;


      private void Awake()
      {
            instance = this;

            uiManager = GetComponentInChildren<UIManager>();
            isaacTearFactory = GetComponentInChildren<IsaacTearFactory>();
            monsterTearFactory = GetComponentInChildren<MonsterTearFactory>();

            itemFactory = GetComponentInChildren<ItemFactory>();

            minimap = minimap != null ? minimap : FindAnyObjectByType<Minimap>();
      }

      public static GameManager Instance
      {
            get {
                  if (instance == null) return null;
                  return instance;
            }
      }

      private void Start()
      {
            GameStart();
      }

      public void GameStart()
      {
            uiManager.GameStart();
      }

      public void GameOver()
      {
            Time.timeScale = 0;
            uiManager.GameOver();
            // SceneManager.LoadScene(0);
      }

      public void StageClear()
      {
            Time.timeScale = 0;
            // Debug.Log("Stage Clear!");
            uiManager.StageClear();
      }


      #region For UI Button
      public void RetryGame()
      {
            SceneManager.LoadScene(0);
      }

      public void ExitGame()
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
