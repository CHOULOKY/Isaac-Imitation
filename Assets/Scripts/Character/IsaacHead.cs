using ItemSpace;
using Photon.Pun;
using System.Linq;
using System.Threading;
using UnityEngine;
using VectorUtilities;

// Photon applied complete
public class IsaacHead : MonoBehaviour, ITearShooter
{
      private PhotonView photonView;


      private IsaacBody body;

      private Animator animator;
      private SpriteRenderer spriteRenderer;

      [HideInInspector] public Vector2 inputVec;

      private TearFactory.Tears tearType = TearFactory.Tears.Basic;
      [Tooltip("= tearRange")] public float tearSpeed = 6;
      private int tearWhatEye = 1;


      [SerializeField] private float attackSpeed = 0.5f;
      public float AttackSpeed
      {
            get => attackSpeed;
            set {
                  if (attackSpeed != value) {
                        attackSpeed = value;
                        photonView.RPC(nameof(RPC_SetAttackSpeed), RpcTarget.OthersBuffered, value);
                  }
            }
      }
      [PunRPC]
      private void RPC_SetAttackSpeed(float value)
      {
            attackSpeed = value;
      }

      [SerializeField] private int attackCount = 1;
      public int AttackCount
      {
            get => attackCount;
            set {
                  if (attackCount != value) {
                        attackCount = value;
                        photonView.RPC(nameof(RPC_SetAttackCount), RpcTarget.OthersBuffered, value);
                  }
            }
      }
      [PunRPC]
      private void RPC_SetAttackCount(int value)
      {
            attackCount = value;
      }

      private float curAttackTime = 0.25f;

      #region Item
      [HideInInspector] public ItemVisual onionVisual;
      [HideInInspector] public ItemVisual innerEyeVisual;
      #endregion


      private void Awake()
      {
            photonView = GetComponent<PhotonView>();

            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();

            body = transform.parent.GetComponent<IsaacBody>();

            foreach (ItemVisual target in GetComponentsInChildren<ItemVisual>(true)) {
                  switch (target.passiveType) {
                        case PassiveType.Onion:
                              onionVisual = target;
                              break;
                        case PassiveType.InnerEye:
                              innerEyeVisual = target;
                              break;
                  }
            }
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

            if (body.IsHurt || body.ingPickup) return;

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
            ControlItemAnimator(false);
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


                        // 중심 방향 설정 (현재 입력 방향을 기본 방향으로 사용)
                        Vector2 baseDirection = inputVec.normalized;
                        if (baseDirection == Vector2.zero) {
                              Debug.LogWarning("Attack direction is zero!");
                              return;
                        }

                        // AttackCount에 따라 발사 방향 분배
                        float PositionStep = (AttackCount > 1) ? 0.4f / (AttackCount - 1) : 0; // 눈물 발사 시작점
                        float basePosition = (AttackCount > 1) ? -0.2f : 0f;
                        float angleStep = (AttackCount > 1) ? 35f / (AttackCount - 1) : 0; // 35도를 AttackCount-1로 나눔
                        float startAngle = (AttackCount > 1) ? -22.5f : 0f; // 중심 기준 좌우 대칭으로 발사 시작 각도

                        for (int i = 0; i < AttackCount; i++) {
                              float currentPosition = basePosition + (PositionStep * i);
                              float currentAngle = startAngle + (angleStep * i);
                              Vector2 rotatedDirection = baseDirection.Rotate(currentAngle); // 각도만큼 방향 회전
                              photonView.RPC(nameof(RPC_SetTearBefore), RpcTarget.All, tearSpeed, tearWhatEye);
                              photonView.RPC(nameof(RPC_AttackUsingTear), RpcTarget.All, tearType, currentPosition, rotatedDirection);
                        }
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
      private void RPC_AttackUsingTear(TearFactory.Tears tearType, float basePosition, Vector2 direction)
      {
            animator.SetTrigger("Fire1");
            ControlItemAnimator(true);

            GameObject curTear = GameManager.Instance.isaacTearFactory.GetTear(tearType, true);
            if (!PhotonNetwork.IsMasterClient) {
                  SetTearPositionAndDirection(curTear, out Rigidbody2D tearRigid, basePosition);
                  if (tearRigid == default) {
                        Debug.LogWarning($"{this.name}'s tears don't have Rigidbody2D!");
                        return;
                  }

                  SetTearVelocity(out Vector2 tearVelocity, tearRigid);
                  ShootSettedTear(curTear, tearRigid, tearVelocity, direction);
            }
      }

      public void SetTearPositionAndDirection(GameObject curTear, out Rigidbody2D tearRigid, float basePosition)
      {
            if (curTear.GetComponent<Tear>() is Tear tear &&
                  curTear.GetComponent<Rigidbody2D>() is Rigidbody2D curRigid) {
                  float positionByEye = (AttackCount == 1) ? 0.2f * (tearWhatEye *= -1) : basePosition;
                  Vector2 offset = default;
                  // Up: 0, Down: 1, Right: 2, Left: 3
                  if (inputVec.x == 1) {
                        offset = new Vector2(0.3f, positionByEye);
                        tear.tearDirection = 2;
                  }
                  else if (inputVec.x == -1) {
                        offset = new Vector2(-0.3f, -positionByEye);
                        tear.tearDirection = 3;
                  }
                  else if (inputVec.y == 1) {
                        offset = new Vector2(-positionByEye, 0.3f);
                        tear.tearDirection = 0;
                  }
                  else if (inputVec.y == -1) {
                        offset = new Vector2(positionByEye, -0.3f);
                        tear.tearDirection = 1;
                  }

                  tearRigid = curRigid;
                  if (AttackCount != 1) tearRigid.position = (Vector2)transform.position;
                  else tearRigid.position = (Vector2)transform.position + offset;
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

      public void ShootSettedTear(GameObject curTear, Rigidbody2D tearRigid, Vector2 tearVelocity, Vector2 direction = default)
      {
            float adjustedSpeed = inputVec.y < 0 ? tearSpeed * 0.75f : tearSpeed;
            tearRigid.AddForce(direction * adjustedSpeed + tearVelocity, ForceMode2D.Impulse);
      }




      private void ControlItemAnimator(bool setTrigger = false)
      {
            if (PhotonNetwork.IsMasterClient) return;

            if (onionVisual.gameObject.activeSelf) {
                  onionVisual.ControlAnimator(setTrigger);
            }
            if (innerEyeVisual.gameObject.activeSelf) {
                  innerEyeVisual.ControlAnimator(setTrigger);
            }
      }
      

      private void OnDisable()
      {
            inputVec = Vector2.zero;

            spriteRenderer.flipX = false;

            animator.SetInteger("XAxisRaw", 0);
            animator.SetInteger("YAxisRaw", 0);
      }
}
