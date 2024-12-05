using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;

public class Destroyer : MonoBehaviour
{
	private void OnTriggerEnter2D(Collider2D other)
	{
            // 마스터 클라이언트만 실행 (RDG)
            if (!PhotonNetwork.IsMasterClient) return;

		if (other.CompareTag("Room") && other.transform != this.transform.parent) {
                  //other.transform.parent.GetComponent<RoomTemplates>().rooms.Remove(other.gameObject);
                  //PhotonNetwork.Destroy(other.gameObject);
                  GetComponent<PhotonView>().RPC(
                        nameof(RPC_RemoveRoomInList), RpcTarget.AllBuffered, other.GetComponent<PhotonView>().ViewID);
            }
	}


      [PunRPC]
      private void RPC_RemoveRoomInList(int roomViewID)
      {
            PhotonView roomView = PhotonView.Find(roomViewID);

            if(roomView == null) return;

            //transform.parent.GetComponent<RoomTemplates>().rooms.Remove(roomView.gameObject);
            FindAnyObjectByType<RoomTemplates>().rooms.Remove(roomView.gameObject);

            if (PhotonNetwork.IsMasterClient) {
                  // 먼저 자식 포톤뷰 오브젝트를 제거한 다음, 부모(본인) 제거
                  foreach (PhotonView PV in roomView.GetComponentsInChildren<PhotonView>()
                        .Where(obj => obj != roomView.gameObject)) {
                        PhotonNetwork.Destroy(PV.gameObject);
                  }
                  PhotonNetwork.Destroy(roomView.gameObject);
            }
      }
}
