using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

namespace ItemSpace
{
      public class ItemProp : MonoBehaviour
      {
            public List<GameObject> passives;

            private void OnEnable()
            {
                  if (!PhotonNetwork.IsMasterClient) return;

                  // 현재 활성화된 아이템은 리스트에서 삭제
                  foreach (var passive in FindAnyObjectByType<IsaacBody>(FindObjectsInactive.Include).GetComponentsInChildren<ItemVisual>()) {
                        foreach (GameObject obj in passives) {
                              if (obj.name.Contains((passive.passiveType.ToString()))) {
                                    passives.Remove(obj);
                                    break;
                              }
                        }
                  }

                  if (passives.Count == 0) {
                        Debug.LogWarning("No more obtainable items exist!");
                        return;
                  }

                  int index = UnityEngine.Random.Range(0, passives.Count);
                  PhotonNetwork.Instantiate(passives[index].name + " Variant", transform.position + Vector3.up * 0.5f, Quaternion.identity);
            }
      }
}
