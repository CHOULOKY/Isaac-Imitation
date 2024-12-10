using MonstroStates;
using Photon.Pun;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class MonstroFSMRPC : FSMRPCController, ITearShooter, IPunObservable
{
      private Monstro monster;
      private Rigidbody2D rigid;
      private Collider2D monsterCollider;
      private Collider2D shadowCollider;

      public IsaacBody player;

      protected override void Awake()
      {
            base.Awake();

            monster = GetComponent<Monstro>();
            rigid = GetComponent<Rigidbody2D>();
            monsterCollider = GetComponent<Collider2D>();
            foreach (Transform child in monster.GetComponentsInChildren<Transform>()) {
                  if (child.name.Contains("Shadow")) {
                        shadowCollider = child.GetComponent<Collider2D>();
                        break;
                  }
            }

            player = FindObjectsOfType<IsaacBody>(true).FirstOrDefault();
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

      //private SpriteRenderer curRenderer, byRenderer;
      //public void FSMRPC_SoryBy(SpriteRenderer _curRenderer, SpriteRenderer _byRenderer, bool wantBackOriginal = false)
      //{
      //      curRenderer = _curRenderer;
      //      byRenderer = _byRenderer;
      //      photonView.RPC(nameof(RPC_SoryBy), RpcTarget.All, wantBackOriginal);
      //}
      public void FSMRPC_SoryBy()
      {
            //photonView.RPC(nameof(RPC_SoryBy), RpcTarget.All);
            if (monster.transform.position.y > player.transform.position.y) spriteRenderer.sortingOrder = -2;
            else spriteRenderer.sortingOrder = 2;
      }
      //[PunRPC]
      //private void RPC_SoryBy()
      //{
      //      monster.sortRendererBy.SortBy(curRenderer, byRenderer, wantBackOriginal);
      //}

      public void FSMRPC_AnimatorPlay(string name, float betweenTime = 0f)
      {
            //Debug.LogError(name);
            animator.Play(name, 0, betweenTime);
            photonView.RPC(nameof(RPC_AnimatorPlay), RpcTarget.OthersBuffered, name, betweenTime);
      }
      [PunRPC]
      private void RPC_AnimatorPlay(string name, float betweenTime)
      {
            animator.Play(name, 0, betweenTime);
      }




      #region Control Exclude Layers
      private Collider2D targetCollider;

      // Exclude Layers에 레이어를 추가하는 함수
      public void FSMRPC_AddExcludeLayerToCollider(bool isMonsterColl, int layer)
      {
            //targetCollider = collder;
            photonView.RPC(nameof(RPC_AddExcludeLayerToCollider), RpcTarget.OthersBuffered, isMonsterColl, layer);
      }
      [PunRPC]
      private void RPC_AddExcludeLayerToCollider(bool isMonsterColl, int layer)
      {
            if (isMonsterColl) targetCollider = monsterCollider;
            else targetCollider = shadowCollider;

            // 현재 excludeLayers에 layerToAdd를 추가
            targetCollider.excludeLayers |= (1 << layer);
      }

      // Exclude Layers에 레이어를 제거하는 함수
      public void FSMRPC_RemoveExcludeLayerFromCollider(bool isMonsterColl, int layer)
      {
            //targetCollider = collder;
            photonView.RPC(nameof(RPC_RemoveExcludeLayerFromCollider), RpcTarget.OthersBuffered, isMonsterColl, layer);
      }
      [PunRPC]
      private void RPC_RemoveExcludeLayerFromCollider(bool isMonsterColl, int layer)
      {
            if (isMonsterColl) targetCollider = monsterCollider;
            else targetCollider = shadowCollider;

            // 현재 excludeLayers에서 layerToRemove를 제거
            targetCollider.excludeLayers &= ~(1 << layer);
      }
      #endregion




      private void Update()
      {
            if (!player) player = FindAnyObjectByType<IsaacBody>();

            //elapsedAnimationTime += Time.deltaTime;
            //curJumpDownDelayTime += Time.deltaTime;
      }

      public float elapsedAnimationTime = 0;
      //public void FSMRPC_SetElapsedTime(float time)
      //{
      //      photonView.RPC(nameof(RPC_SetElapsedTime), RpcTarget.AllBuffered, time);
      //}
      //[PunRPC]
      //private void RPC_SetElapsedTime(float time)
      //{
      //      elapsedAnimationTime = time;
      //}

      public Vector2 directionVec = default;
      public void FSMRPC_SetDirectionVec(Vector2 valueVec)
      {
            photonView.RPC(nameof(RPC_SetDirectionVec), RpcTarget.AllBuffered, valueVec);
      }
      [PunRPC]
      private void RPC_SetDirectionVec(Vector2 valueVec)
      {
            directionVec = valueVec;
      }

      public void FSMRPC_SpawnBloodEffects()
      {
            photonView.RPC(nameof(RPC_SpawnBloodEffects), RpcTarget.All);
      }
      [PunRPC]
      private void RPC_SpawnBloodEffects()
      {
            monster.SpawnBloodEffects();
      }



      #region For Small Jump State
      private Vector2 shadowOffset = default;
      public Vector2 ShadowOffset
      {
            get => shadowOffset;
            set {
                  shadowOffset = value;
                  photonView.RPC(nameof(RPC_SetShadowOffset), RpcTarget.OthersBuffered, shadowOffset);
            }
      }
      [PunRPC]
      private void RPC_SetShadowOffset(Vector2 value)
      {
            shadowOffset = value;
      }


      public int maxJumpCount = 0;
      public void FSMRPC_SetMaxJumpCount(int value)
      {
            photonView.RPC(nameof(RPC_SetMaxJumpCount), RpcTarget.AllBuffered, value);
      }
      [PunRPC]
      private void RPC_SetMaxJumpCount(int value)
      {
            maxJumpCount = value;
            curJumpCount = value;
      }

      public int curJumpCount = 0;
      public void FSMRPC_SetCurJumpCount(int value)
      {
            photonView.RPC(nameof(RPC_SetCurJumpCount), RpcTarget.OthersBuffered, value);
      }
      [PunRPC]
      private void RPC_SetCurJumpCount(int value)
      {
            curJumpCount = value;
      }



      public Vector2 nextPosition = Vector2.zero;
      public void FSMRPC_SetNextPosition(Transform baseTransform, float distance)
      {
            Vector3 nextDirection = player.transform.position - baseTransform.position;
            Vector3 nextPosition = nextDirection.normalized * distance;
            photonView.RPC(nameof(RPC_SetNextPosition), RpcTarget.AllBuffered, baseTransform.position + nextPosition);
      }
      [PunRPC]
      private void RPC_SetNextPosition(Vector3 _nextPosition)
      {
            nextPosition = _nextPosition;
      }
      #endregion

      #region For Big Jump State
      public float curJumpDownDelayTime = 0;

      public Vector2 playerLatePosition = default;
      public void FSMRPC_SetPlayerLatePosition(Vector2 valueVec)
      {
            photonView.RPC(nameof(RPC_SetPlayerLatePosition), RpcTarget.AllBuffered, valueVec);
      }
      [PunRPC]
      private void RPC_SetPlayerLatePosition(Vector2 valueVec)
      {
            playerLatePosition = valueVec;
      }

      public bool isTearSparied = false;
      public void FSMRPC_SetisTearSparied(bool value)
      {
            isTearSparied = value;
            photonView.RPC(nameof(RPC_SetisTearSparied), RpcTarget.OthersBuffered, value);
      }
      [PunRPC]
      private void RPC_SetisTearSparied(bool value)
      {
            isTearSparied = value;
      }
      #endregion

      #region For Tear Spray State
      public int sprayCount = 0;
      public void FSMRPC_SetSprayCount(int value)
      {
            sprayCount = value;
            curSprayCount = value;
            photonView.RPC(nameof(RPC_SetSprayCount), RpcTarget.OthersBuffered, value);
      }
      [PunRPC]
      private void RPC_SetSprayCount(int value)
      {
            sprayCount = value;
            curSprayCount = value;
      }

      public int curSprayCount = 0;
      public void FSMRPC_SetCurSprayCount(int value)
      {
            photonView.RPC(nameof(RPC_SetCurSprayCount), RpcTarget.OthersBuffered, value);
      }
      [PunRPC]
      private void RPC_SetCurSprayCount(int value)
      {
            curSprayCount = value;
      }
      #endregion

      #region For Tear Spray
      private bool isSprayState = false;
      public void FSMRPC_GetTearAndAttack(TearFactory.Tears tearType, int tearCount, bool _isSprayState = false)
      {
            photonView.RPC(nameof(RPC_GetTearAndAttack), RpcTarget.AllBuffered, tearType, tearCount, _isSprayState);
      }
      [PunRPC]
      private void RPC_GetTearAndAttack(TearFactory.Tears tearType, int tearCount, bool _isSprayState)
      {
            isSprayState = _isSprayState;
            for (int i = 0; i < tearCount; i++) {
                  GameObject tear = GameManager.Instance.monsterTearFactory.GetTear(tearType, true);
                  if (PhotonNetwork.IsMasterClient) AttackUsingTear(tear);
            }
      }

      public void AttackUsingTear(GameObject curTear = default)
      {
            SetTearPositionAndDirection(curTear, out Rigidbody2D tearRigid);
            if (tearRigid == default) {
                  Debug.LogWarning($"{gameObject.name}'s tears don't have Rigidbody2D!");
                  return;
            }

            SetTearVelocity(out Vector2 tearVelocity, tearRigid);
            ShootSettedTear(curTear, tearRigid, tearVelocity);
      }

      public void SetTearPositionAndDirection(GameObject curTear, out Rigidbody2D tearRigid)
      {
            if (curTear.GetComponent<Tear>() is Tear tear &&
                  curTear.GetComponent<Rigidbody2D>() is Rigidbody2D curRigid) {
                  Vector2 offset = new Vector2(0, -0.3f);
                  // Up: 0, Down: 1, Right: 2, Left: 3
                  if (directionVec.x > 0) {
                        tear.tearDirection = 2;
                  }
                  else if (directionVec.x < 0) {
                        tear.tearDirection = 3;
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
            // Not used
            tearVelocity = Vector2.zero;

            tearRigid.velocity = Vector2.zero;
      }

      public void ShootSettedTear(GameObject curTear, Rigidbody2D tearRigid, Vector2 tearVelocity)
      {
            float rotateAngle;
            if (isSprayState) {
                  // TearSprayState
                  rotateAngle = UnityEngine.Random.Range(-25f, 25f);
            }
            else {
                  // BigJumpState
                  rotateAngle = UnityEngine.Random.Range(-180f, 180f);
            }

            Vector2 inputVec = RotateVector(directionVec.normalized, rotateAngle);
            float adjustedSpeed = UnityEngine.Random.Range(monster.stat.tearSpeed - 1, monster.stat.tearSpeed + 2);
            tearRigid.AddForce(inputVec * adjustedSpeed + tearVelocity, ForceMode2D.Impulse);
      }

      private Vector2 RotateVector(Vector2 v, float angle)
      {
            // 1. 각도를 라디안으로 변환
            float radian = angle * Mathf.Deg2Rad;

            // 2. 회전 행렬의 요소 계산
            float cos = Mathf.Cos(radian);
            float sin = Mathf.Sin(radian);

            // 3. 회전 행렬 적용
            return new Vector2(
                v.x * cos - v.y * sin,
                v.x * sin + v.y * cos
            );
      }
      #endregion

      #region For Dead State
      public void FSMRPC_AfterIsDeadFinish()
      {
            photonView.RPC(nameof(RPC_AfterIsDeadFinish), RpcTarget.AllBuffered);
      }
      [PunRPC]
      private void RPC_AfterIsDeadFinish()
      {
            monster.gameObject.layer = LayerMask.NameToLayer("Destroyed");
            monster.gameObject.SetActive(false);
      }

      public Animator[] deadEffectAnimators;
      public void FSMRPC_SetDeadAnimators()
      {
            photonView.RPC(nameof(RPC_SetDeadAnimators), RpcTarget.AllBuffered);
      }
      [PunRPC]
      private void RPC_SetDeadAnimators()
      {
            deadEffectAnimators = monster.GetComponentsInChildren<Animator>(true)
                        .Where(anim => anim.gameObject != monster.gameObject).ToArray();
      }

      public void FSMRPC_ControlExplosionEffect(float explosionAnimationLength)
      {
            photonView.RPC(nameof(RPC_ControlExplosionEffect), RpcTarget.AllBuffered, explosionAnimationLength);
      }
      [PunRPC]
      private void RPC_ControlExplosionEffect(float explosionAnimationLength)
      {
            deadEffectAnimators[0].SetTrigger("Finish");
            for (int i = 1; i < deadEffectAnimators.Length; i++) {
                  deadEffectAnimators[i].Play("New State", 0, 0);
            }

            DelaySetParent(deadEffectAnimators[0].transform, monster.transform, explosionAnimationLength);
      }

      private async void DelaySetParent(Transform target, Transform parent, float time = 1)
      {
            target.parent = null;

            await Task.Delay((int)(1000 * time));

            target.parent = parent;
      }




      public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
      {
            if (stream.IsWriting) {
                  stream.SendNext(elapsedAnimationTime);
                  stream.SendNext(curJumpDownDelayTime);
            }
            else {
                  elapsedAnimationTime = (float)stream.ReceiveNext();
                  curJumpDownDelayTime = (float)stream.ReceiveNext();
            }
      }
      #endregion
}
