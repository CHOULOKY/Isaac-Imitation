using Photon.Pun;
using System.Collections;
using System.Linq;
using UnityEngine;

public class ExitDoor : MonoBehaviour
{
      private Animator animator;
      private Collider2D thisCollider;

      private void Awake()
      {
            animator = GetComponent<Animator>();
            thisCollider = GetComponent<Collider2D>();
      }

      private void OnEnable()
      {
            thisCollider.enabled = false;
            StartCoroutine(SetActiveAfterAnimation(animator, "AM_SpawnItem", true));
      }

      private IEnumerator SetActiveAfterAnimation(Animator animator, string animName, bool active)
      {
            // �ִϸ��̼��� ���� ������ ���
            yield return new WaitForSeconds(animator.runtimeAnimatorController.animationClips
                        .FirstOrDefault(clip => clip.name == animName)?.length ?? 0f);

            thisCollider.enabled = active;
      }

      private void OnCollisionEnter2D(Collision2D collision)
      {
            // ������ Ŭ���̾�Ʈ�� ����
            if (!PhotonNetwork.IsMasterClient) return;

            if (collision.transform.CompareTag("Player")) {
                  GameManager.Instance.IsClear = true;
            }
      }
}
