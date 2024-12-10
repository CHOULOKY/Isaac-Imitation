using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class GaperFSMRPC : FSMRPCController, IPunObservable
{
      private Gaper monster;
      private GameObject monsterHead;

      protected override void Awake()
      {
            monster = GetComponent<Gaper>();
            foreach(FlashEffect component in monster.GetComponentsInChildren<FlashEffect>(true)) {
                  if (component == monster.GetComponent<FlashEffect>()) continue;
                  monsterHead = component.gameObject;
                  break;
            }

            base.Awake();
      }


      public Queue<Vector2> followQueue = new();

      public bool alreadyJustBody = false;
      public void FSMRPC_OnceIsJustBody()
      {
            photonView.RPC(nameof(RPC_OnceIsJustBody), RpcTarget.AllBuffered);
      }
      [PunRPC]
      private void RPC_OnceIsJustBody()
      {
            alreadyJustBody = true;
            monsterHead.SetActive(false);
            monster.SpawnBloodEffects();
      }

      [PunRPC]
      protected override void RPC_SetSpriteDirection(Vector2 inputVec, bool xGreaterThanY)
      {
            if (spriteRenderer) {
                  if (monster.inputVec.x > 0) {
                        spriteRenderer.flipX = false;
                  }
                  else if (monster.inputVec.x < 0) {
                        spriteRenderer.flipX = true;
                  }
            }

            if (animator && photonView.IsMine) {
                  if (xGreaterThanY) {
                        animator.SetInteger("XAxisRaw", (int)monster.inputVec.x);
                        animator.SetInteger("YAxisRaw", 0);
                  }
                  else {
                        animator.SetInteger("XAxisRaw", 0);
                        animator.SetInteger("YAxisRaw", (int)monster.inputVec.y);
                  }
            }
      }



      public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
      {
            if (stream.IsWriting) {
                  // Queue의 내용을 배열로 변환하여 전송
                  Vector2[] queueArray = followQueue.ToArray();
                  stream.SendNext(queueArray.Length); // 큐의 크기를 먼저 전송
                  foreach (var item in queueArray) {
                        stream.SendNext(item); // 큐의 각 항목을 전송
                  }
            }
            else {
                  // 받은 데이터를 큐에 복원
                  int count = (int)stream.ReceiveNext(); // 큐의 크기 받기
                  followQueue.Clear(); // 큐 초기화

                  for (int i = 0; i < count; i++) {
                        Vector2 item = (Vector2)stream.ReceiveNext(); // 항목 받기
                        followQueue.Enqueue(item); // 큐에 항목 추가
                  }
            }
      }
}
