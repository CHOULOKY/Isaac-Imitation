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

                  int index = UnityEngine.Random.Range(0, passives.Count);
                  PhotonNetwork.Instantiate(passives[index].name + " Variant", transform.position + Vector3.up * 0.5f, Quaternion.identity);
            }
      }
}
