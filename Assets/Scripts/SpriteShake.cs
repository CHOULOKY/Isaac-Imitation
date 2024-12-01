using Photon.Pun;
using System.Collections;
using UnityEngine;

// Photon applied complete
public class SpriteShake : MonoBehaviour
{
      private PhotonView photonView;
      private PhotonTransformViewClassic transformView;

      public float duration = 0.25f;   // 흔들림 지속 시간
      public float magnitude = 0.1f; // 흔들림 강도

      private Vector3 originalPosition; // 원래 위치
      private float elapsed = 0.0f;     // 경과 시간


      private void Awake()
      {
            photonView = GetComponent<PhotonView>();
            transformView = GetComponent<PhotonTransformViewClassic>();
      }

      // monstro use => animation event
      public void StartShake()
      {
            // 현재 오브젝트에 소유권이 없으면 return => 한 번만 StartShake 실행
            if (!photonView.IsMine) return;

            originalPosition = transform.localPosition;
            elapsed = 0.0f;
            StartCoroutine(Shake());
      }

      private IEnumerator Shake()
      {
            photonView.RPC(nameof(RPC_SetInterpolateDisabled), RpcTarget.Others, true);

            float offsetX, offsetY;
            while (elapsed < duration) {
                  elapsed += Time.deltaTime;

                  // 랜덤한 위치 생성
                  offsetX = UnityEngine.Random.Range(-1f, 1f) * magnitude;
                  offsetY = UnityEngine.Random.Range(-1f, 1f) * magnitude;

                  transform.localPosition = originalPosition + new Vector3(offsetX, offsetY, 0);

                  yield return null; // 다음 프레임까지 대기
            }

            // 흔들림 종료 후 원래 위치로 복구
            transform.localPosition = originalPosition;

            photonView.RPC(nameof(RPC_SetInterpolateDisabled), RpcTarget.Others, false);
      }
      [PunRPC]
      private void RPC_SetInterpolateDisabled(bool disabled)
      {
            if (disabled) {
                  transformView.m_PositionModel.InterpolateOption =
                        PhotonTransformViewPositionModel.InterpolateOptions.Disabled;
            }
            else {
                  transformView.m_PositionModel.InterpolateOption =
                        PhotonTransformViewPositionModel.InterpolateOptions.EstimatedSpeed;
            }
      }
}
