using Photon.Pun;
using System.Threading;
using UnityEngine;

// Photon applied complete
public class IsaacHead : MonoBehaviour, ITearShooter
{
      private PhotonView photonView;


      private IsaacBody body;

      private Animator animator;
      private SpriteRenderer spriteRenderer;

      private Vector2 inputVec;

      private TearFactory.Tears tearType = TearFactory.Tears.Basic;
      [Tooltip("= tearRange")] public float tearSpeed = 6;
      private int tearWhatEye = 1;

      public float attackSpeed = 0.25f;
      private float curAttackTime = 0.25f;

      #region Item
      //private int temp;
      #endregion


      private void Awake()
      {
            photonView = GetComponent<PhotonView>();

            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();

            body = transform.parent.GetComponent<IsaacBody>();
      }

      private void OnEnable()
      {
            // 마스터 클라이언트가 아니고 현재 오브젝트를 소유하고 있지 않으면
            if (!PhotonNetwork.IsMasterClient && photonView.Owner != PhotonNetwork.LocalPlayer) {
                  photonView.RequestOwnership();
            }
      }

      private void Update()
      {
            // 현재 오브젝트에 소유권이 없으면 return
            if (!photonView.IsMine) return;

            if (body.IsHurt) return;

            GetInputVec();

            SetHeadDirection();

            AttackUsingTear();
      }

      private void GetInputVec()
      {
            if (Input.GetKeyDown(KeyCode.LeftArrow)) {
                  inputVec.x = -1;
                  inputVec.y = 0;
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow)) {
                  inputVec.x = 1;
                  inputVec.y = 0;
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow)) {
                  inputVec.x = 0;
                  inputVec.y = 1;
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow)) {
                  inputVec.x = 0;
                  inputVec.y = -1;
            }

            photonView.RPC(nameof(RPC_SetInputVec), RpcTarget.Others, inputVec);
      }
      [PunRPC]
      private void RPC_SetInputVec(Vector2 _inputVec)
      {
            inputVec = _inputVec;
      }

      private void SetHeadDirection()
      {
            if (inputVec.x > 0) {
                  //spriteRenderer.flipX = false;
                  photonView.RPC(nameof(RPC_SetFlipX), RpcTarget.All, false);
            }
            else if (inputVec.x < 0) {
                  //spriteRenderer.flipX = true;
                  photonView.RPC(nameof(RPC_SetFlipX), RpcTarget.All, true);
            }
            animator.SetInteger("XAxisRaw", (int)inputVec.x);
            animator.SetInteger("YAxisRaw", (int)inputVec.y);
      }
      [PunRPC]
      private void RPC_SetFlipX(bool flipX)
      {
            spriteRenderer.flipX = flipX;
      }

      public void AttackUsingTear(GameObject curTear = default)
      {
            curAttackTime += Time.deltaTime;
            if (curAttackTime > attackSpeed) {
                  if (Input.GetButton("Horizontal Arrow") || Input.GetButton("Vertical Arrow")) {
                        curAttackTime = 0;

                        //curTear = GameManager.Instance.isaacTearFactory.GetTear(tearType, true);
                        //SetTearPositionAndDirection(curTear, out Rigidbody2D tearRigid);
                        //if (tearRigid == default) {
                        //      Debug.LogWarning($"{this.name}'s tears don't have Rigidbody2D!");
                        //      return;
                        //}

                        //SetTearVelocity(out Vector2 tearVelocity, tearRigid);
                        //ShootSettedTear(curTear, tearRigid, tearVelocity);
                        photonView.RPC(nameof(RPC_SetTearBefore), RpcTarget.All, tearSpeed, tearWhatEye);
                        photonView.RPC(nameof(RPC_AttackUsingTear), RpcTarget.All, tearType);
                  }
            }
      }
      [PunRPC]
      private void RPC_SetTearBefore(float _tearSpeed, int _tearWhatEye)
      {
            tearSpeed = _tearSpeed;
            tearWhatEye = _tearWhatEye;
      }
      [PunRPC]
      private void RPC_AttackUsingTear(TearFactory.Tears tearType)
      {
            animator.SetTrigger("Fire1");

            GameObject curTear = GameManager.Instance.isaacTearFactory.GetTear(tearType, true);
            SetTearPositionAndDirection(curTear, out Rigidbody2D tearRigid);
            if (tearRigid == default) {
                  Debug.LogWarning($"{this.name}'s tears don't have Rigidbody2D!");
                  return;
            }

            SetTearVelocity(out Vector2 tearVelocity, tearRigid);
            ShootSettedTear(curTear, tearRigid, tearVelocity);
      }

      public void SetTearPositionAndDirection(GameObject curTear, out Rigidbody2D tearRigid)
      {
            if (curTear.GetComponent<Tear>() is Tear tear &&
                  curTear.GetComponent<Rigidbody2D>() is Rigidbody2D curRigid) {
                  tearWhatEye *= -1;
                  Vector2 offset = default;
                  // Up: 0, Down: 1, Right: 2, Left: 3
                  if (inputVec.x == 1) {
                        offset.x = 0.3f;
                        offset.y = 0.2f * tearWhatEye;
                        tear.tearDirection = 2;
                  }
                  else if (inputVec.x == -1) {
                        offset.x = -0.3f;
                        offset.y = 0.2f * tearWhatEye;
                        tear.tearDirection = 3;
                  }
                  else if (inputVec.y == 1) {
                        offset.x = 0.2f * tearWhatEye;
                        offset.y = 0.3f;
                        tear.tearDirection = 0;
                  }
                  else {
                        // inputVec.y == -1
                        offset.x = 0.2f * tearWhatEye;
                        offset.y = -0.3f;
                        tear.tearDirection = 1;
                  }

                  tearRigid = curRigid;
                  tearRigid.position = (Vector2)transform.position + offset;
            }
            else {
                  tearRigid = default;
            }
      }

      public void SetTearVelocity(out Vector2 tearVelocity, Rigidbody2D tearRigid)
      {
            tearVelocity = Vector2.zero;

            //if (body.GetComponent<Rigidbody2D>() is Rigidbody2D bodyRigid) {
            //      tearVelocity.x = body.inputVec.x == -inputVec.x ? bodyRigid.velocity.x * 0.25f : bodyRigid.velocity.x * 0.5f;
            //      tearVelocity.y = body.inputVec.y == -inputVec.y ? bodyRigid.velocity.y * 0.25f : bodyRigid.velocity.y * 0.5f;
            //}
            tearVelocity.x = body.inputVec.x == -inputVec.x ? body.bodyVelocity.x * 0.25f : body.bodyVelocity.x * 0.5f;
            tearVelocity.y = body.inputVec.y == -inputVec.y ? body.bodyVelocity.y * 0.25f : body.bodyVelocity.y * 0.5f;

            tearRigid.velocity = Vector2.zero;
      }

      public void ShootSettedTear(GameObject curTear, Rigidbody2D tearRigid, Vector2 tearVelocity)
      {
            float adjustedSpeed = inputVec.y < 0 ? tearSpeed * 0.75f : tearSpeed;
            // tearRigid.AddForce(inputVec * tearSpeed + Vector2.up / 2 + tearVelocity, ForceMode2D.Impulse);
            tearRigid.AddForce(inputVec * adjustedSpeed + tearVelocity, ForceMode2D.Impulse);
      }
      

      private void OnDisable()
      {
            inputVec = Vector2.zero;

            spriteRenderer.flipX = false;

            animator.SetInteger("XAxisRaw", 0);
            animator.SetInteger("YAxisRaw", 0);
      }
}
