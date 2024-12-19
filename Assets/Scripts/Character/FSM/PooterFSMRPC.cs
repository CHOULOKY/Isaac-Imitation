using ObstacleSpace;
using Photon.Pun;
using System.Threading;
using UnityEngine;
using static ItemSpace.Heart;

public class PooterFSMRPC : FSMRPCController, ITearShooter
{
      private Rigidbody2D rigid;
      private Pooter monster;

      protected override void Awake()
      {
            base.Awake();

            rigid = GetComponent<Rigidbody2D>();
            monster = GetComponent<Pooter>();
      }


      [PunRPC]
      protected override void RPC_SetSpriteDirection(Vector2 inputVec, bool temp = false)
      {
            if (spriteRenderer) {
                  if (inputVec.x > 0) {
                        spriteRenderer.flipX = false;
                  }
                  else if (inputVec.x < 0) {
                        spriteRenderer.flipX = true;
                  }
            }
      }

      public Vector2 directionVec = Vector2.zero;
      public void FSMRPC_SetDirectionVec(Vector2 valueVec)
      {
            photonView.RPC(nameof(RPC_SetDirectionVec), RpcTarget.All, valueVec);
      }
      [PunRPC]
      private void RPC_SetDirectionVec(Vector2 valueVec)
      {
            directionVec = valueVec;
      }




      #region GetTearAndAttack
      public void FSMRPC_GetTearAndAttack(TearFactory.Tears tearType)
      {
            photonView.RPC(nameof(RPC_GetTearAndAttack), RpcTarget.AllBuffered, tearType);
      }
      [PunRPC]
      private void RPC_GetTearAndAttack(TearFactory.Tears tearType)
      {
            GameObject tear = GameManager.Instance.monsterTearFactory.GetTear(tearType, true);
            if (PhotonNetwork.IsMasterClient) AttackUsingTear(tear);
      }

      public void AttackUsingTear(GameObject curTear = default)
      {
            TearIgnoreObstacle(curTear);

            SetTearPositionAndDirection(curTear, out Rigidbody2D tearRigid);
            if (tearRigid == default) {
                  Debug.LogWarning($"{gameObject.name}'s tears don't have Rigidbody2D!");
                  return;
            }

            SetTearVelocity(out Vector2 tearVelocity, tearRigid);
            ShootSettedTear(curTear, tearRigid, tearVelocity);
      }

      private void TearIgnoreObstacle(GameObject curTear)
      {
            // 현재 몬스터가 있는 방 찾기
            if (gameObject.GetComponentInParent<AddRoom>() is AddRoom room) {
                  // 현재 방에서 이름이 Obstacles인 오브젝트 찾기
                  Transform obstacles = default;
                  foreach (Transform child in room.transform) {
                        if (child.name == "Obstacles") {
                              obstacles = child;
                              break;
                        }
                  }

                  if (obstacles == null) {
                        Debug.LogWarning("No object named 'Obstacles' found in the room.");
                        return;
                  }

                  Collider2D thisCollider = curTear.GetComponent<Collider2D>();
                  foreach (Obstacle obstacle in obstacles.GetComponentsInChildren<Obstacle>(true)) {
                        Physics2D.IgnoreCollision(thisCollider, obstacle.GetComponent<Collider2D>(), true);
                  }
            }
      }

      public void SetTearPositionAndDirection(GameObject curTear, out Rigidbody2D tearRigid, float basePosition = default)
      {
            if (curTear.GetComponent<Tear>() is Tear tear &&
                  curTear.GetComponent<Rigidbody2D>() is Rigidbody2D curRigid) {
                  Vector2 offset = new Vector2(0, -0.35f);
                  // Up: 0, Down: 1, Right: 2, Left: 3
                  if (directionVec.x > 0) {
                        tear.tearDirection = 2;
                  }
                  else if (directionVec.x < 0) {
                        tear.tearDirection = 3;
                  }
                  else if (directionVec.y > 0) {
                        tear.tearDirection = 0;
                  }
                  else {
                        tear.tearDirection = 1;
                  }

                  tearRigid = curRigid;
                  tearRigid.position = rigid.position + offset;
            }
            else {
                  tearRigid = default;
            }
      }

      public void SetTearVelocity(out Vector2 tearVelocity, Rigidbody2D tearRigid)
      {
            tearVelocity.x = Mathf.Clamp(directionVec.x, -1, 1);
            tearVelocity.y = Mathf.Clamp(directionVec.y, -1, 1);

            tearRigid.velocity = Vector2.zero;
      }

      public void ShootSettedTear(GameObject curTear, Rigidbody2D tearRigid, Vector2 tearVelocity, Vector2 direction = default)
      {
            Vector2 inputVec = Mathf.Abs(directionVec.x) > Mathf.Abs(directionVec.y) ?
                  Vector2.right * Mathf.Sign(directionVec.x) : Vector2.up * Mathf.Sign(directionVec.y);
            float adjustedSpeed = inputVec.y < 0 ? monster.stat.tearSpeed * 0.75f : monster.stat.tearSpeed;
            tearRigid.AddForce(inputVec * adjustedSpeed + tearVelocity, ForceMode2D.Impulse);
      }
      #endregion
}
