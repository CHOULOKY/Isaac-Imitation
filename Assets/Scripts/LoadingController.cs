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
      public string stringWatingGame = "������ �ҷ����� ��...";
      public string stringWatingServer = "������ �����ϴ� ��...";
      public string stringWatingOther = "�ٸ� �÷��̾ ��ٸ��� ��...";
      public string stringWatingReady = "���� ���� �غ� ��...";
      public string stringReadyGame = "�غ� �Ϸ�! ���� ���� ��...";

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

            StartCoroutine(LoadSceneProcess()); // �񵿱� �ε�

            StartCoroutine(RotateAnimation(rectTransform, spaceTime, rotateAmount)); // �ؽ�Ʈ �ִϸ��̼�
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

            // Wating Board �ö���� �ִϸ��̼� ���
            yield return StartCoroutine
                  (AfterAnimation(watingText.transform.parent.GetComponent<Animator>(), "AM_WatingUp"));

            // ���� �� �ε� ���
            Debug.Log("���� �� �ε� ��...");
            yield return StartCoroutine(TypingCycle(stringWatingGame));
            yield return new WaitUntil(() => !(asyncOp.progress < 0.9f));
            Debug.Log("���� �� �ε� �Ϸ�");

            // ���� ���� ���
            Debug.Log("���� ���� ��...");
            yield return StartCoroutine(TypingCycle(stringWatingServer));
            yield return new WaitUntil(() => NetworkManager.Instance.isServerAccess);
            Debug.Log("���� ���� �Ϸ�");

            // �ٸ� �÷��̾� ���
            Debug.Log("�ٸ� �÷��̾� ��� ��...");
            yield return StartCoroutine(TypingCycle(stringWatingOther));
            yield return new WaitUntil(() => NetworkManager.Instance.isOtherAccess);
            Debug.Log("�ٸ� �÷��̾� ���� �Ϸ�");

            // ���� ���� ����
            Debug.Log("���� ���� �غ� ��...");
            yield return StartCoroutine(TypingCycle(stringWatingReady));
            Hashtable props = new Hashtable { { "CanStartGame", true } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
            foreach (var player in PhotonNetwork.PlayerList) {
                  yield return new WaitUntil(()
                        => (player.CustomProperties.ContainsKey("CanStartGame") && (bool)player.CustomProperties["CanStartGame"]));
            }
            Debug.Log("���� �غ� �Ϸ�. ���� ��...");

            // ���� ����
            yield return StartCoroutine(TypingCycle(stringReadyGame));
            yield return new WaitForSecondsRealtime(1f); // 1�� ���
            yield return StartCoroutine(fadeController.FadeInCoroutine(null, 3));
            Debug.Log("--------------------���� ����--------------------");
            yield return new WaitForSecondsRealtime(1f); // 1�� ���
            asyncOp.allowSceneActivation = true;
      }

      private IEnumerator TypingCycle(string text)
      {
            yield return new WaitForSecondsRealtime(1f); // 1�� ���

            yield return StartCoroutine(TypingAnimation(watingText, null, 0.1f, true)); // untyping
            yield return new WaitForSecondsRealtime(0.25f);
            yield return StartCoroutine(TypingAnimation(watingText, text, 0.1f, false)); // typing
      }


      private IEnumerator AfterAnimation(Animator animator, string animName)
      {
            // �ִϸ��̼��� ���� ������ ���
            yield return new WaitForSeconds(animator.runtimeAnimatorController.animationClips
                        .FirstOrDefault(clip => clip.name == animName)?.length ?? 0f);
      }


      private void OnDisable()
      {
            StopAllCoroutines();
      }
}
