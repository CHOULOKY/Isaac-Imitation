using Photon.Pun;
using System.Threading;
using UnityEngine;

public class FSMRPCController : MonoBehaviour
{
      protected PhotonView photonView;

      protected SpriteRenderer spriteRenderer;
      protected Animator animator;

      protected virtual void Awake()
      {
            photonView = GetComponent<PhotonView>();

            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
      }



      public virtual void FSMRPC_SetTrigger(string name)
      {
            photonView.RPC(nameof(RPC_SetTrigger), RpcTarget.AllBuffered, name);
      }
      [PunRPC]
      protected virtual void RPC_SetTrigger(string name)
      {
            animator.SetTrigger(name);
      }

      public virtual void FSMRPC_SetBool(string name, bool value)
      {
            photonView.RPC(nameof(RPC_SetTrigger), RpcTarget.AllBuffered, name, value);
      }
      [PunRPC]
      protected virtual void RPC_SetBool(string name, bool value)
      {
            animator.SetBool(name, value);
      }




      public virtual void FSMRPC_SetSpriteDirection(Vector2 inputVec, bool temp = false)
      {
            photonView.RPC(nameof(RPC_SetSpriteDirection), RpcTarget.All, inputVec, temp);
      }
      [PunRPC]
      protected virtual void RPC_SetSpriteDirection(Vector2 inputVec, bool temp = false)
      {
            if (spriteRenderer) {
                  if (inputVec.x > 0) {
                        spriteRenderer.flipX = false;
                  }
                  else if (inputVec.x < 0) {
                        spriteRenderer.flipX = true;
                  }
            }
            if (animator && photonView.IsMine) {
                  animator.SetInteger("XAxisRaw", (int)inputVec.x);
                  animator.SetInteger("YAxisRaw", (int)inputVec.y);
            }
      }
}
