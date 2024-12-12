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
                  // Queue�� ������ �迭�� ��ȯ�Ͽ� ����
                  Vector2[] queueArray = followQueue.ToArray();
                  stream.SendNext(queueArray.Length); // ť�� ũ�⸦ ���� ����
                  foreach (var item in queueArray) {
                        stream.SendNext(item); // ť�� �� �׸��� ����
                  }
            }
            else {
                  // ���� �����͸� ť�� ����
                  int count = (int)stream.ReceiveNext(); // ť�� ũ�� �ޱ�
                  followQueue.Clear(); // ť �ʱ�ȭ

                  for (int i = 0; i < count; i++) {
                        Vector2 item = (Vector2)stream.ReceiveNext(); // �׸� �ޱ�
                        followQueue.Enqueue(item); // ť�� �׸� �߰�
                  }
            }
      }
}
