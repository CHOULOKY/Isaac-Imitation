using System.Collections;
using System.Linq;
using UnityEngine;
using ItemSpace.HeartSpace;
using Unity.Mathematics;
using static ItemSpace.Heart;
using System;
using Photon.Pun;

// Photon applied complete
namespace ItemSpace
{
      namespace HeartSpace
      {
            [System.Serializable]
            public class HeartArray
            {
                  public Sprite[] hearts;
            }
      }

      public class Heart : Pickup
      {
            public enum HeartType { Normal, Soul }
            public HeartType heartType;
            public HeartArray[] heartArray;
            private bool isHalf = false;

            private SpriteRenderer spriteRenderer;

            private FlashEffect flashEffect;

            protected override void Awake()
            {
                  spriteRenderer = GetComponent<SpriteRenderer>();
                  base.Awake();

                  flashEffect = GetComponent<FlashEffect>();
            }

            protected override void OnEnable()
            {
                  // ������ Ŭ���̾�Ʈ(Body)�� �ƴϸ� return
                  if (!PhotonNetwork.IsMasterClient) return;

                  base.OnEnable();

                  heartType = (HeartType)UnityEngine.Random.Range(0, Enum.GetValues(typeof(HeartType)).Length);
                  heartType = UnityEngine.Random.Range(0, 3) == 0 ? HeartType.Normal : heartType; // 3���� 1 Ȯ���� �ٽ� �Ϲ� ��Ʈ

                  isHalf = UnityEngine.Random.Range(0, 2) != 0;
                  //spriteRenderer.sprite = heartArray[(int)heartType].hearts[isHalf ? 1 : 0];
                  photonView.RPC(nameof(RPC_SetSprite), RpcTarget.AllBuffered, heartType, isHalf);
            }
            [PunRPC]
            private void RPC_SetSprite(HeartType _heartType, bool _isHalf)
            {
                  heartType = _heartType;
                  isHalf = _isHalf;
                  spriteRenderer.sprite = heartArray[(int)_heartType].hearts[_isHalf ? 1 : 0];
            }

            protected override void OnCollisionEnter2D(Collision2D collision)
            {
                  // ������ Ŭ���̾�Ʈ(Body)�� �ƴϸ� return
                  if (!PhotonNetwork.IsMasterClient) return;

                  if (collision.collider.GetComponent<IsaacBody>() is IsaacBody player) {
                        switch (heartType) {
                              case HeartType.Normal:
                                    if (player.Health < player.MaxHealth) {
                                          HandlePickup();
                                          player.Health += (isHalf ? 1 : 2);
                                    }
                                    break;
                              case HeartType.Soul:
                                    if (player.SoulHealth < 12) {
                                          HandlePickup();
                                          player.SoulHealth += (isHalf ? 1 : 2);
                                    }
                                    break;
                        }
                  }
            }

            // For animation event
            public void HeartIdle()
            {
                  if (UnityEngine.Random.Range(0, 2) == 0) {
                        flashEffect.Flash(1f, 1f, 1f, 1f); // white
                  }
            }
      }
}
