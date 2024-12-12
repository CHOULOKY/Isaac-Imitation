using Photon.Pun;
using UnityEngine;

public class MainCamera : SetResolution
{
      public IsaacBody Isaac;
      public float lerpSpeed;

      private Vector3 offset;
      private Vector3 targetPos = Vector3.zero;
      public Vector2 maxBoundary;
      public Vector2 minBoundary;

      private void Awake()
      {
            if(!Isaac) Isaac = FindAnyObjectByType<IsaacBody>();
      }

      protected override void Start()
      {
            base.Start();

            offset = Vector3.back * 10;
            this.transform.position = Vector3.zero + offset;
      }

      private float _lerpSpeed, _distance;
      private void LateUpdate()
      {
            // 마스터 클라이언트만
            if (PhotonNetwork.IsMasterClient) {
                  if (!Isaac) Isaac = FindAnyObjectByType<IsaacBody>();
                  //Debug.Log(Isaac.transform.position);

                  _distance = (transform.position - targetPos).magnitude;
                  _lerpSpeed = _distance >= 10 ? 1 : lerpSpeed;

                  targetPos = Isaac.transform.position + offset;
                  targetPos.x = Mathf.Clamp(targetPos.x, minBoundary.x, maxBoundary.x);
                  targetPos.y = Mathf.Clamp(targetPos.y, minBoundary.y, maxBoundary.y);
                  transform.position = Vector3.Lerp(transform.position, targetPos, _lerpSpeed);
                  //Debug.Log(targetPos);
            }
      }
}
