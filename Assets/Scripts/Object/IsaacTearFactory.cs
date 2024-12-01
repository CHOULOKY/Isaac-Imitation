using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

// Photon applied complete
public class IsaacTearFactory : TearFactory
{
      protected override void Awake()
      {
            photonView = GetComponent<PhotonView>();

            pool = new List<GameObject>[tearList.Count];
            parent = new Transform[tearList.Count];
            poolIndex = new int[tearList.Count];
            for (int i = 0; i < tearList.Count; i++) {
                  pool[i] = new List<GameObject>();
                  GameObject _parent = new GameObject(tearList[i].name);
                  _parent.transform.position = new Vector3(1000, 1000, 0);
                  parent[i] = _parent.transform;
                  poolIndex[i] = default;
            }

            //GameObject created = default;
            //for (int i = 0; i < tearList.Count; i++) {
            //      for (int j = 0; j < poolSize[i]; j++) {
            //            created = Instantiate(tearList[i], parent[i]);
            //            created.SetActive(false);
            //            pool[i].Add(created);
            //      }
            //}
            if (PhotonNetwork.IsMasterClient) { // 마스터 클라이언트만 실행
                  if (photonView.Owner != PhotonNetwork.LocalPlayer) photonView.RequestOwnership();

                  GameObject created = default;
                  for (int i = 0; i < tearList.Count; i++) {
                        for (int j = 0; j < poolSize[i]; j++) {
                              created = PhotonNetwork.Instantiate(
                                    tearList[i].name + " Variant", parent[i].transform.position, tearList[i].transform.rotation);
                              photonView.RPC(nameof(RPC_CreatedSet), RpcTarget.AllBuffered,
                                    created.GetComponent<PhotonView>().ViewID, i, false); // 모두에게
                        }
                  }
            }
      }
}
