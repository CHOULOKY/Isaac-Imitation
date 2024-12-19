using ItemSpace;
using Photon.Pun;
using System.Threading.Tasks;
using UnityEngine;

// Photon applied complete
public class IsaacBody : MonoBehaviour, IPunObservable
{
      private PhotonView photonView;


      private IsaacHead head;

      private Rigidbody2D rigid;
      private Animator animator;
      private SpriteRenderer spriteRenderer;

      private FlashEffect flashEffect;

      [HideInInspector] public Vector2 inputVec;
      public float moveForce = 20; // +5
      public float maxVelocity = 5; // moveForce/5 + 1
      [HideInInspector] public float curMoveForce;
      [HideInInspector] public float curMaxVelocity;

      // for head (photonview)
      [HideInInspector] public Vector2 bodyVelocity;

      #region Health
      [SerializeField] private int health;
      public int Health
      {
            get => health;
            set {
                  if (health > 0) {
                        if (value > health) {
                              if (value > MaxHealth) health = MaxHealth;
                              else health = value;
                        }
                        else {
                              int damage = health - value;
                              if (SoulHealth > 0) SoulHealth -= damage;
                              else health = value;

                              if (SoulHealth < 0) {
                                    health += SoulHealth;
                                    SoulHealth = 0;
                              }
                        }
                  }
                  else health = value;

                  //if (value != maxHealth) GameManager.Instance.uiManager.RefreshUI();
                  photonView.RPC(nameof(RPC_SetHealth), RpcTarget.AllBuffered, health);
            }
      }
      [PunRPC]
      private void RPC_SetHealth(int _health)
      {
            health = _health;
            GameManager.Instance.uiManager.RefreshUI();
      }

      [SerializeField] private int maxHealth = 6;
      public int MaxHealth
      {
            get => maxHealth;
            set {
                  if (value > 24) maxHealth = 24;
                  else maxHealth = value;

                  //GameManager.Instance.uiManager.RefreshUI();
                  photonView.RPC(nameof(RPC_SetMaxHealth), RpcTarget.AllBuffered, maxHealth);
            }
      }
      [PunRPC]
      private void RPC_SetMaxHealth(int _maxHealth)
      {
            maxHealth = _maxHealth;
            GameManager.Instance.uiManager.RefreshUI();
      }

      [SerializeField] private int soulHealth = 0;
      public int SoulHealth
      {
            get => soulHealth;
            set {
                  if (value > 24) soulHealth = 24;
                  else soulHealth = value;

                  //GameManager.Instance.uiManager.RefreshUI();
                  photonView.RPC(nameof(RPC_SetSoulHealth), RpcTarget.AllBuffered, soulHealth);
            }
      }
      [PunRPC]
      private void RPC_SetSoulHealth(int _soulHealth)
      {
            soulHealth = _soulHealth;
            GameManager.Instance.uiManager.RefreshUI();
      }
      #endregion

      #region Item
      [Header("Item")]
      [SerializeField] private int bombCount = 3;
      public int BombCount
      {
            get => bombCount;
            set {
                  bombCount = value;

                  //GameManager.Instance.uiManager.RefreshUI();
                  photonView.RPC(nameof(RPC_SetBombCount), RpcTarget.AllBuffered, bombCount);
            }
      }
      [PunRPC]
      private void RPC_SetBombCount(int _bombCount)
      {
            bombCount = _bombCount;
            GameManager.Instance.uiManager.RefreshUI();
      }

      public float maxBombCool = 1;
      private float curBombCool = 0;



      [HideInInspector] public bool ingPickup = false;
      #endregion


      private void Awake()
      {
            photonView = GetComponent<PhotonView>();

            head = GetComponentInChildren<IsaacHead>();

            rigid = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();

            flashEffect = GetComponent<FlashEffect>();
      }

      private void OnEnable()
      {
            // 마스터 클라이언트고 현재 오브젝트를 소유하고 있지 않으면
            if (PhotonNetwork.IsMasterClient && photonView.Owner != PhotonNetwork.LocalPlayer) {
                  photonView.RequestOwnership();
            }

            Health = MaxHealth;
            curMoveForce = moveForce;
            curMaxVelocity = maxVelocity;
      }

      private void Update()
      {
            // 현재 오브젝트에 소유권이 없으면 return
            if (!PhotonNetwork.IsMasterClient) return;

            GetInputVec();

            SetBodyDirection();

            ControlItems();

            // test code
            //if (Input.GetKeyDown(KeyCode.Alpha1)) {
            //      IsHurt = true;
            //}
      }

      private void FixedUpdate()
      {
            // 현재 오브젝트에 소유권이 없으면 return
            if (!photonView.IsMine) return;

            MoveBody();
      }

      private void GetInputVec()
      {
            inputVec.x = Input.GetAxisRaw("Horizontal WASD");
            inputVec.y = Input.GetAxisRaw("Vertical WASD");
            photonView.RPC(nameof(RPC_SetInputVec), RpcTarget.Others, inputVec);
      }
      [PunRPC]
      private void RPC_SetInputVec(Vector2 _inputVec)
      {
            inputVec = _inputVec;
      }

      private void SetBodyDirection()
      {
            if (inputVec.x > 0) {
                  photonView.RPC(nameof(RPC_SetFlipX), RpcTarget.All, false);
                  // spriteRenderer.flipX = false;
            }
            else if (inputVec.x < 0) {
                  photonView.RPC(nameof(RPC_SetFlipX), RpcTarget.All, true);
                  // spriteRenderer.flipX = true;
            }
            animator.SetInteger("XAxisRaw", (int)inputVec.x);
            animator.SetInteger("YAxisRaw", (int)inputVec.y);
      }
      [PunRPC]
      private void RPC_SetFlipX(bool flipX)
      {
            spriteRenderer.flipX = flipX;
      }

      private void MoveBody()
      {
            rigid.AddForce(inputVec.normalized * curMoveForce, ForceMode2D.Force);
            if (rigid.velocity.magnitude > curMaxVelocity) {
                  rigid.velocity = rigid.velocity.normalized * curMaxVelocity;
            }
      }

      private GameObject itemObject = default;
      private void ControlItems()
      {
            curBombCool += Time.deltaTime;

            if (Input.GetKey(KeyCode.E) && BombCount > 0 && curBombCool > maxBombCool) {
                  curBombCool = 0;
                  BombCount--;

                  //itemObject = GameManager.Instance.itemFactory.GetItem(ItemFactory.Items.Bomb, false);
                  //itemObject.transform.position = rigid.position;
                  //itemObject.SetActive(true);
                  photonView.RPC(nameof(RPC_BombControl), RpcTarget.AllBuffered);
            }
      }
      [PunRPC]
      private void RPC_BombControl()
      {
            itemObject = GameManager.Instance.itemFactory.GetItem(ItemFactory.Items.Bomb, false);
            itemObject.transform.position = rigid.position;
            itemObject.SetActive(true);
      }
      

      private bool isHurt = false;
      public bool IsHurt
      {
            get { return isHurt; }
            set {
                  if (isHurt == false) {
                        //isHurt = true;
                        photonView.RPC(nameof(RPC_SetisHurt), RpcTarget.AllBuffered, true);
                        if (Health <= 0) {
                              IsDeath = true;
                              return;
                        }

                        // Body만
                        if (photonView.IsMine) {
                              curMoveForce = moveForce + 10;
                              curMaxVelocity = maxVelocity + 2;
                        }

                        //아이템 Visual 잠시 꺼두기
                        photonView.RPC(nameof(RPC_SetAlphaItemVisual), RpcTarget.All, 0);
                        //this.animator.SetTrigger("Hit");
                        photonView.RPC(nameof(RPC_SetAnimTrigger), RpcTarget.All, "Hit");
                        //flashEffect.Flash(new Color(1, 1, 0, 1));
                        photonView.RPC(nameof(flashEffect.Flash), RpcTarget.AllBuffered, 1f, 1f, 0f, 1f);
                  }
                  else photonView.RPC(nameof(RPC_SetisHurt), RpcTarget.AllBuffered, value);
            }
      }
      [PunRPC]
      private void RPC_SetisHurt(bool value)
      {
            isHurt = value;
      }
      [PunRPC]
      private void RPC_SetAnimTrigger(string name)
      {
            this.animator.SetTrigger(name);
      }

      [PunRPC]
      private void RPC_SetAlphaItemVisual(float a)
      {
            foreach (ItemVisual item in GetComponentsInChildren<ItemVisual>()) {
                  item.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, a);
            }
      }


      // For animation event
      public async void ResetIsHurtAfterAnimation()
      {
            // 마스터 클라이언트(Body)가 아니면 return
            if (!PhotonNetwork.IsMasterClient) return;

            await Task.Delay(500); // 0.5 second

            IsHurt = false;
            curMoveForce = moveForce;
            curMaxVelocity = maxVelocity;

            //아이템 Visual 다시 켜기
            photonView.RPC(nameof(RPC_SetAlphaItemVisual), RpcTarget.All, 1);
      }

      // For animation event
      public void SetHeadSpriteAlpha(float _alpha)
      {
            // 마스터 클라이언트(Body)가 아니면 return
            if (!PhotonNetwork.IsMasterClient) return;

            //head.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, _alpha);
            photonView.RPC(nameof(RPC_SetHeadAlpha), RpcTarget.All, _alpha);
      }
      [PunRPC]
      private void RPC_SetHeadAlpha(float _alpha)
      {
            head.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, _alpha);
      }

      // For animation event
      private void SetIngPickup(int value)
      {
            photonView.RPC(nameof(RPC_SetIngPickup), RpcTarget.All, value);
      }
      [PunRPC]
      private void RPC_SetIngPickup(int value)
      {
            ingPickup = value != 0;
            //Debug.LogError(ingPickup);
      }


      private void OnCollisionEnter2D(Collision2D collision)
      {
            if (!PhotonNetwork.IsMasterClient) return;

            if (collision.collider.CompareTag("Passive")) {
                  photonView.RPC(nameof(RPC_SetAnimTrigger), RpcTarget.AllBuffered, "Pickup");
            }
      }




      private bool isDeath = false;
      public bool IsDeath
      {
            get { return isDeath; }
            set {
                  if (isDeath == false) {
                        //isDeath = true;
                        photonView.RPC(nameof(RPC_SetisDeath), RpcTarget.AllBuffered, true);
                        GameManager.Instance.GameOver();
                        //photonView.RPC(nameof(GameManager.Instance.GameOver), RpcTarget.AllBuffered);
                  }
            }
      }
      [PunRPC]
      private void RPC_SetisDeath(bool value)
      {
            isDeath = value;
      }

      private void OnDisable()
      {
            inputVec = Vector2.zero;
            rigid.velocity = Vector2.zero;

            animator.SetInteger("XAxisRaw", 0);
            animator.SetInteger("YAxisRaw", 0);
      }



      public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
      {
            if (stream.IsWriting) {
                  // 로컬 플레이어의 flipX 값을 전송
                  stream.SendNext(rigid.velocity);
            }
            else {
                  // 원격 플레이어의 flipX 값을 수신
                  bodyVelocity = (Vector2)stream.ReceiveNext();
            }
      }
}
