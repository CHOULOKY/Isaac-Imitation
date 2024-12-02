using UnityEngine;
using Photon.Pun;
using System.Threading;

public class ChargerFSMRPC : FSMRPCController
{
      private Transform[] shadows = new Transform[2]; // 0: XShadow, 1: YShadow

      protected override void Awake()
      {
            base.Awake();
            shadows[0] = transform.GetChild(0);
            shadows[1] = transform.GetChild(1);
      }


      [PunRPC]
      protected override void RPC_SetSpriteDirection(Vector2 inputVec, bool temp = false)
      {
            if (spriteRenderer) {
                  if (inputVec.x > 0) {
                        spriteRenderer.flipX = false;
                        shadows[0].gameObject.SetActive(true);
                        shadows[1].gameObject.SetActive(false);
                  }
                  else if (inputVec.x < 0) {
                        spriteRenderer.flipX = true;
                        shadows[0].gameObject.SetActive(true);
                        shadows[1].gameObject.SetActive(false);
                  }
                  else if (inputVec.y != 0) {
                        shadows[0].gameObject.SetActive(false);
                        shadows[1].gameObject.SetActive(true);
                  }
            }
            if (animator && photonView.IsMine) {
                  animator.SetInteger("XAxisRaw", (int)inputVec.x);
                  animator.SetInteger("YAxisRaw", (int)inputVec.y);
            }
      }
}
