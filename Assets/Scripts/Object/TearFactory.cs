using Photon.Pun;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static ItemSpace.ItemFactory;

// Photon applied complete
public class TearFactory : MonoBehaviour
{
      public enum Tears { Basic, Boss }

      [SerializeField] protected List<GameObject> tearList;

      [SerializeField] protected int[] poolSize;
      protected List<GameObject>[] pool;
      protected Transform[] parent;
      protected int[] poolIndex;


      protected PhotonView photonView;

      protected virtual void Awake()
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
      [PunRPC]
      protected virtual void RPC_CreatedSet(int viewID, int i, bool active)
      {
            GameObject created = PhotonView.Find(viewID).gameObject;
            created.transform.parent = parent[i].transform;
            created.SetActive(active);
            pool[i].Add(created);
      }
      

      public virtual GameObject GetTear(Tears _type, bool _setActive = true)
      {
            int index = (int)_type;
            GameObject selected;
            for (int i = 0; i < pool[index].Count; i++) {
                  if (!pool[index][poolIndex[index]].activeSelf) {
                        selected = pool[index][poolIndex[index]];
                        selected.SetActive(_setActive);
                        return selected;
                  }
                  poolIndex[index] = ++poolIndex[index] % pool[index].Count;
            }

            //selected = Instantiate(tearList[index], parent[index]);
            //pool[index].Add(selected);
            if (PhotonNetwork.IsMasterClient) { // 마스터 클라이언트만 실행
                  if (photonView.Owner != PhotonNetwork.LocalPlayer) photonView.RequestOwnership();

                  selected = PhotonNetwork.Instantiate(
                        tearList[index].name + " Variant", parent[index].transform.position, tearList[index].transform.rotation);
                  photonView.RPC(nameof(RPC_CreatedSet), RpcTarget.AllBuffered,
                        selected.GetComponent<PhotonView>().ViewID, index, _setActive); // 모두에게
            }
            selected = pool[index][^1];
            return selected;
      }
}


public interface ITearShooter
{
      public void AttackUsingTear(GameObject curTear = default);

      public void SetTearPositionAndDirection(GameObject curTear, out Rigidbody2D tearRigid);

      public void SetTearVelocity(out Vector2 tearVelocity, Rigidbody2D tearRigid);

      public void ShootSettedTear(GameObject curTear, Rigidbody2D tearRigid, Vector2 tearVelocity, Vector2 direction = default);
}
