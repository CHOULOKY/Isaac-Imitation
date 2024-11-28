using UnityEngine;
using UnityEngine.UI;
using UISpace;
using TMPro;
using System.Xml;
using Photon.Realtime;
using System;
using UnityEngine.Playables;
using UnityEditor.Rendering;

namespace UISpace
{
      [System.Serializable]
      public class UIStruct
      {
            public Image[] images;
            public TMP_Text tmpText;
      }
}
public class UIManager : MonoBehaviour
{
      #region Fade
      private FadeController fadeController;

      public float fadeDuration = 2;
      #endregion

      private IsaacBody player;

      [Header("UI")]
      public Canvas uiCanvas; // unused
      public Canvas clearCanvas;
      public Canvas deadCanvas;

      [Header("UI - Heart")]
      [SerializeField] private UIStruct heartStruct;
      public Sprite[] heartSprites; // 0: zero, 1: half, 2: full
      [SerializeField] private UIStruct soulHeartStruct;
      public Sprite[] soulHeartSprites; // 0: zero, 1: half, 2: full
      [SerializeField] private int heartMultiple = 1;
      [SerializeField] private int soulHeartMultiple = 1;

      [Header("UI - Consume")]
      [SerializeField] private UIStruct coinStruct;
      [SerializeField] private UIStruct bombStruct;
      [SerializeField] private UIStruct keyStruct;
      [SerializeField] private int coinMultiple = 0;
      [SerializeField] private int bombMultiple = 3;
      [SerializeField] private int keyMultiple = 0;

      [Header("UI - Dead Canvas")]
      public int killedPlayer = 0; // { Charger, Gaper, Pooter, Monstro, Spike, Bomb }
      public string setKilledPlayer
      {
            get => null;
            set {
                  switch (value) {
                        case "Charger":
                              killedPlayer = (int)MonsterType.Charger;
                              break;
                        case "Gaper":
                              killedPlayer = (int)MonsterType.Gaper;
                              break;
                        case "Pooter":
                              killedPlayer = (int)MonsterType.Pooter;
                              break;
                        case "Monstro":
                              killedPlayer = (int)MonsterType.Monstro;
                              break;
                        case "Spike":
                              killedPlayer = Enum.GetValues(typeof(MonsterType)).Length;
                              break;
                        case "Bomb":
                              killedPlayer = Enum.GetValues(typeof(MonsterType)).Length + 1;
                              break;
                  }
            }
      }


      private void Awake()
      {
            // Fade
            fadeController = GetComponentInChildren<FadeController>();

            player = FindAnyObjectByType<IsaacBody>();
      }




      public void GameStart(float _fadeDuration = -1)
      {
            // Fade
            _fadeDuration = _fadeDuration <= 0 ? fadeDuration : _fadeDuration;
            StartCoroutine(fadeController.FadeOutCoroutine(null, fadeDuration));

            // UI
            RefreshUI();
      }

      private void Update()
      {
            // test code
            // if (Input.GetKeyDown(KeyCode.Space)) RefreshUI();
      }

      public void RefreshUI()
      {
            if (player == null)
                  player = FindAnyObjectByType<IsaacBody>();

            // Heart
            UpdateHeartUI(
                player.Health,
                heartStruct.images,
                heartSprites,
                heartStruct.tmpText,
                ref heartMultiple
            );

            // Soul Heart
            UpdateHeartUI(
                player.SoulHealth,
                soulHeartStruct.images,
                soulHeartSprites,
                soulHeartStruct.tmpText,
                ref soulHeartMultiple
            );

            // Consumables
            bombMultiple = player.BombCount;
            coinStruct.tmpText.text = coinMultiple.ToString("D2");
            bombStruct.tmpText.text = bombMultiple.ToString("D2");
            keyStruct.tmpText.text = keyMultiple.ToString("D2");
      }

      /// <summary>
      /// Heart UI를 업데이트하는 공통 메서드
      /// </summary>
      private void UpdateHeartUI(
          int health,
          Image[] images,
          Sprite[] sprites,
          TMP_Text tmpText,
          ref int multiple
      )
      {
            multiple = (int)Mathf.Ceil(health / 8f);
            tmpText.text = $" x{multiple}";

            int fullCount = multiple == 0 ? 0 : (health - (multiple - 1) * 8) / 2;
            int halfCount = multiple == 0 ? 0 : (health - (multiple - 1) * 8) % 2;

            // Full hearts
            for (int i = 0; i < fullCount; i++) {
                  images[i].sprite = sprites[2]; // Full heart
            }

            // Half heart
            if (halfCount == 1) {
                  images[fullCount].sprite = sprites[1]; // Half heart
            }

            // Empty hearts
            for (int i = fullCount + halfCount; i < 4; i++) {
                  images[i].sprite = sprites[0]; // Zero heart
            }
      }



      public void GameOver()
      {
            deadCanvas.GetComponent<PlayableDirector>().Play();
            SetActiveMinimap(false);
      }

      public void StageClear()
      {
            clearCanvas.GetComponent<PlayableDirector>().Play();
            SetActiveMinimap(false);
      }

      private void SetActiveMinimap(bool active)
      {
            foreach (Camera camera in FindObjectsOfType<Camera>()) {
                  if (camera.name == "Minimap Camera") {
                        camera.enabled = active;
                  }
            }
      }
}
