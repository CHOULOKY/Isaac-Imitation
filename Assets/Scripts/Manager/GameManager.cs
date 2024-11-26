using ItemSpace;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
      private static GameManager instance;

      public int CurrentChaper = 1;

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
            SceneManager.LoadScene(0);
      }
}
