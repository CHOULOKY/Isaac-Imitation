using UnityEngine;
using UnityEngine.UI;
using UISpace;
using TMPro;
using System.Xml;
using Photon.Realtime;
using System;
using UnityEngine.Playables;
using UnityEditor.Rendering;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Linq;
using Photon.Pun;
using ItemSpace;

namespace UISpace
{
      [System.Serializable]
      public class UIStruct
      {
            public Image[] images;
            public TMP_Text tmpText;
      }
}
public class UIManager : ScriptAnimation
{
      #region Fade
      private FadeController fadeController;

      public float fadeDuration = 2;
      #endregion

      private IsaacBody player;

      [Header("UI")]
      public Canvas uiCanvas;
      public Canvas clearCanvas;
      public Canvas deadCanvas;
      public Canvas leftCanvas;
      public Canvas startCanvas;

      [Header("UI - Heart")]
      [SerializeField] private UIStruct heartStruct;
      public Sprite[] heartSprites; // 0: zero, 1: half, 2: full
      [SerializeField] private UIStruct soulHeartStruct;
      public Sprite[] soulHeartSprites; // 0: zero, 1: half, 2: full
      [SerializeField] private int heartMultiple = 1;
      [SerializeField] private int soulHeartMultiple = 1;

      [Header("UI - Passive")]
      public Sprite[] passiveSprites;

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

                  photonView.RPC(nameof(RPC_SetKilledPlayer), RpcTarget.AllBuffered, killedPlayer);
            }
      }
      [PunRPC]
      private void RPC_SetKilledPlayer(int _killedPlayer)
      {
            killedPlayer = _killedPlayer;
      }
      

      private PhotonView photonView;
      private void Awake()
      {
            photonView = GetComponent<PhotonView>();

            // Fade
            fadeController = GetComponentInChildren<FadeController>();

            player = FindAnyObjectByType<IsaacBody>();
      }

      private void OnEnable()
      {
            if (PhotonNetwork.IsMasterClient && photonView.Owner != PhotonNetwork.LocalPlayer) {
                  photonView.RequestOwnership();
            }
      }




      public IEnumerator GameStartBefore(float _fadeDuration = -1)
      {
            // Fade
            _fadeDuration = _fadeDuration <= 0 ? fadeDuration : _fadeDuration;
            yield return StartCoroutine(fadeController.FadeOutCoroutine(null, fadeDuration));

            // Canvas Animation
            RectTransform[] children = startCanvas.GetComponentsInChildren<RectTransform>(true);
            // ������ 3�� �ڽ� RectTransform�� RotateAnimation ����
            StartCoroutine(RotateAnimation(children[^3], 0.75f, -10));
            StartCoroutine(RotateAnimation(children[^2], 0.75f, 5));
            StartCoroutine(RotateAnimation(children[^1], 0.75f, -5));

            yield return new WaitForSecondsRealtime(3f); // 3�� ���

            Debug.Log("���� �� ���� �˰��� ���� ��...");
            yield return new WaitUntil(() => GameManager.Instance.roomTemplates.RefreshedRooms);
            Debug.Log("���� �� ���� �˰��� ���� �Ϸ�.");
      }
      public IEnumerator GameStartAfter()
      {
            // InGame UI
            RefreshUI();

            // �ִϸ��̼��� ���� ������ ���
            Animator canvasAnimator = startCanvas.transform.GetChild(0).GetComponent<Animator>();
            canvasAnimator.SetTrigger("Start");
            yield return new WaitForSecondsRealtime(canvasAnimator.runtimeAnimatorController.animationClips
                        .FirstOrDefault(clip => clip.name == "AM_Start")?.length ?? 0f);

            canvasAnimator.gameObject.SetActive(false);
      }

      private void Update()
      {
            // test code
            // if (Input.GetKeyDown(KeyCode.Space)) RefreshUI();
      }

      public void RefreshUI()
      {
            if (player == null)
                  player = FindObjectOfType<IsaacBody>(true);

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
            coinStruct.tmpText.text = coinMultiple.ToString("D2"); // not used
            bombStruct.tmpText.text = bombMultiple.ToString("D2");
            keyStruct.tmpText.text = keyMultiple.ToString("D2"); // not used
      }

      /// <summary>
      /// Heart UI�� ������Ʈ�ϴ� ���� �޼���
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


      // �ٸ� �÷��̾ ������ ��
      public IEnumerator OnPlayerLeftRoom()
      {
            SetActiveMinimap(false);

            Animator animator = leftCanvas.GetComponentInChildren<Animator>(true);
            animator.gameObject.SetActive(true); // LeftBoard ������Ʈ Ȱ��ȭ

            // �ִϸ��̼��� ���� ������ ���
            yield return new WaitForSecondsRealtime(animator.runtimeAnimatorController.animationClips
                        .FirstOrDefault(clip => clip.name == "AM_LeftAppear")?.length ?? 0f);

            TMP_Text exitText = animator.GetComponentInChildren<TMP_Text>();
            StartCoroutine(RotateAnimation(exitText.GetComponent<RectTransform>(), 0.75f, 2)); // �ؽ�Ʈ �ִϸ��̼�

            yield return TypingCycle(animator.GetComponentInChildren<TMP_Text>(), 
                  "<br>�ٸ� �÷��̾ ���ӿ��� �������ϴ�!<br>��� ȭ������ ���ư��ϴ�.");

            animator.SetTrigger("Disappear");
            yield return StartCoroutine(fadeController.FadeInCoroutine(null, 3));
      }

      private IEnumerator TypingCycle(TMP_Text watingText, string text)
      {
            yield return StartCoroutine(TypingAnimation(watingText, null, 0.1f, true)); // untyping
            yield return new WaitForSecondsRealtime(0.25f);
            yield return StartCoroutine(TypingAnimation(watingText, text, 0.1f, false)); // typing
      }





      public void SetActiveBossSlider(bool active)
      {
            foreach (Slider slider in uiCanvas.GetComponentsInChildren<Slider>(true)) {
                  if (slider && slider.name.Contains("Boss")) {
                        slider.gameObject.SetActive(active);
                        break;
                  }
            }
      }

      private int passiveIndex = -1;
      private Image[] passiveImages = null;
      public void SetActivePassiveItem(PassiveType type)
      {
            if (passiveImages == null || passiveImages.Length == 0) {
                  foreach (RectTransform target in uiCanvas.GetComponentsInChildren<RectTransform>(true)) {
                        if (target.name.Contains("Passive")) {
                              passiveImages = target.GetComponentsInChildren<Image>(true);
                              break;
                        }
                  }
                  //Debug.LogError($"Initialized {passiveImages?.Length ?? 0} passive images");
            }

            switch (type) {
                  case PassiveType.Onion:
                        if (passiveIndex + 1 < passiveImages.Length) {
                              passiveIndex++;
                              passiveImages[passiveIndex].gameObject.SetActive(true);
                              passiveImages[passiveIndex].sprite = passiveSprites[(int)type];
                        }
                        break;
            }
      }


      private void OnDisable()
      {
            StopAllCoroutines();
      }
}
