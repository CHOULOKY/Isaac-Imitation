using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Linq;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Pun;
using Photon.Realtime;
using Unity.VisualScripting;

public class LoadingController : ScriptAnimation
{
      [SerializeField] private TMP_Text watingText;

      [Header("Typing")]
      public string stringWatingGame = "게임을 불러오는 중...";
      public string stringWatingServer = "서버에 연결하는 중...";
      public string stringWatingOther = "다른 플레이어를 기다리는 중...";
      public string stringWatingReady = "게임 시작 준비 중...";
      public string stringReadyGame = "준비 완료! 게임 시작 중...";

      [Header("Animation")]
      private RectTransform rectTransform;
      public float spaceTime = 0.75f;
      [Tooltip("Z-Axis")] public float rotateAmount = -2f;

      [Header("Fade")]
      [SerializeField] private FadeController fadeController;

      [Header("Test")]
      public bool isTest = false;


      private static string nextScene = "GameScene";
      #region Unused
      public static void LoadScene(string sceneName)
      {
            nextScene = sceneName;
            SceneManager.LoadScene("LoadingScene");
      }
      #endregion

      private void Awake()
      {
            if (!watingText) watingText = GetComponentInChildren<TMP_Text>();

            // animation
            rectTransform = watingText.GetComponent<RectTransform>();

            // fade
            if(!fadeController) fadeController = FindAnyObjectByType<FadeController>();
      }

      private void OnEnable()
      {
            watingText.text = "";
      }

      private void Start()
      {
            StartCoroutine(fadeController.FadeOutCoroutine(null, 2));

            StartCoroutine(LoadSceneProcess()); // 비동기 로딩

            StartCoroutine(RotateAnimation(rectTransform, spaceTime, rotateAmount)); // 텍스트 애니메이션
      }

      private IEnumerator LoadSceneProcess()
      {
            if (isTest) {
                  yield return new WaitUntil(() => NetworkManager.Instance.isOtherAccess);
                  Hashtable testProps = new Hashtable { { "CanStartGame", true } };
                  PhotonNetwork.LocalPlayer.SetCustomProperties(testProps);
                  foreach (var player in PhotonNetwork.PlayerList) {
                        yield return new WaitUntil(()
                              => (player.CustomProperties.ContainsKey("CanStartGame") && (bool)player.CustomProperties["CanStartGame"]));
                  }
                  PhotonNetwork.LoadLevel(1);
                  yield break;
            }

            AsyncOperation asyncOp = SceneManager.LoadSceneAsync(nextScene);
            asyncOp.allowSceneActivation = false;

            // Wating Board 올라오는 애니메이션 대기
            yield return StartCoroutine
                  (AfterAnimation(watingText.transform.parent.GetComponent<Animator>(), "AM_WatingUp"));

            // 다음 씬 로딩 대기
            Debug.Log("다음 씬 로딩 중...");
            yield return StartCoroutine(TypingCycle(stringWatingGame));
            yield return new WaitUntil(() => !(asyncOp.progress < 0.9f));
            Debug.Log("다음 씬 로딩 완료");

            // 서버 연결 대기
            Debug.Log("서버 연결 중...");
            yield return StartCoroutine(TypingCycle(stringWatingServer));
            yield return new WaitUntil(() => NetworkManager.Instance.isServerAccess);
            Debug.Log("서버 연결 완료");

            // 다른 플레이어 대기
            Debug.Log("다른 플레이어 대기 중...");
            yield return StartCoroutine(TypingCycle(stringWatingOther));
            yield return new WaitUntil(() => NetworkManager.Instance.isOtherAccess);
            Debug.Log("다른 플레이어 접속 완료");

            // 게임 시작 가능
            Debug.Log("게임 시작 준비 중...");
            yield return StartCoroutine(TypingCycle(stringWatingReady));
            Hashtable props = new Hashtable { { "CanStartGame", true } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
            foreach (var player in PhotonNetwork.PlayerList) {
                  yield return new WaitUntil(()
                        => (player.CustomProperties.ContainsKey("CanStartGame") && (bool)player.CustomProperties["CanStartGame"]));
            }
            Debug.Log("게임 준비 완료. 시작 중...");

            // 게임 시작
            yield return StartCoroutine(TypingCycle(stringReadyGame));
            yield return new WaitForSecondsRealtime(1f); // 1초 대기
            yield return StartCoroutine(fadeController.FadeInCoroutine(null, 3));
            Debug.Log("--------------------게임 시작--------------------");
            yield return new WaitForSecondsRealtime(1f); // 1초 대기
            asyncOp.allowSceneActivation = true;
      }

      private IEnumerator TypingCycle(string text)
      {
            yield return new WaitForSecondsRealtime(1f); // 1초 대기

            yield return StartCoroutine(TypingAnimation(watingText, null, 0.1f, true)); // untyping
            yield return new WaitForSecondsRealtime(0.25f);
            yield return StartCoroutine(TypingAnimation(watingText, text, 0.1f, false)); // typing
      }


      private IEnumerator AfterAnimation(Animator animator, string animName)
      {
            // 애니메이션이 끝날 때까지 대기
            yield return new WaitForSeconds(animator.runtimeAnimatorController.animationClips
                        .FirstOrDefault(clip => clip.name == animName)?.length ?? 0f);
      }


      private void OnDisable()
      {
            StopAllCoroutines();
      }
}
