using Photon.Pun;
using System.Collections;
using UnityEngine;

// Photon applied complete
public class SpriteShake : MonoBehaviour
{
      private PhotonView photonView;
      private PhotonTransformViewClassic transformView;

      public float duration = 0.25f;   // ��鸲 ���� �ð�
      public float magnitude = 0.1f; // ��鸲 ����

      private Vector3 originalPosition; // ���� ��ġ
      private float elapsed = 0.0f;     // ��� �ð�


      private void Awake()
      {
            photonView = GetComponent<PhotonView>();
            transformView = GetComponent<PhotonTransformViewClassic>();
      }

      // monstro use => animation event
      public void StartShake()
      {
            // ���� ������Ʈ�� �������� ������ return => �� ���� StartShake ����
            //if (!photonView.IsMine) return;

            originalPosition = transform.localPosition;
            elapsed = 0.0f;
            StartCoroutine(Shake());
      }

      private IEnumerator Shake()
      {
            //photonView.RPC(nameof(RPC_SetInterpolateDisabled), RpcTarget.Others, true);

            float offsetX, offsetY;
            while (elapsed < duration) {
                  elapsed += Time.deltaTime;

                  // ������ ��ġ ����
                  offsetX = UnityEngine.Random.Range(-1f, 1f) * magnitude;
                  offsetY = UnityEngine.Random.Range(-1f, 1f) * magnitude;

                  transform.localPosition = originalPosition + new Vector3(offsetX, offsetY, 0);

                  yield return null; // ���� �����ӱ��� ���
            }

            // ��鸲 ���� �� ���� ��ġ�� ����
            transform.localPosition = originalPosition;

            //photonView.RPC(nameof(RPC_SetInterpolateDisabled), RpcTarget.Others, false);
      }
      //[PunRPC]
      //private void RPC_SetInterpolateDisabled(bool disabled)
      //{
      //      if (disabled) {
      //            transformView.m_PositionModel.InterpolateOption =
      //                  PhotonTransformViewPositionModel.InterpolateOptions.Disabled;
      //      }
      //      else {
      //            transformView.m_PositionModel.InterpolateOption =
      //                  PhotonTransformViewPositionModel.InterpolateOptions.EstimatedSpeed;
      //      }
      //}
}
