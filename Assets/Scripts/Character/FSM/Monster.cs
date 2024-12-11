using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum MonsterType { Charger, Gaper, Pooter, Monstro }
public class Monster<T> : MonoBehaviour, IPunObservable where T : class
{
      protected PhotonView photonView;


      [SerializeField] protected MonsterType monsterType;

      protected Rigidbody2D rigid;

      protected FSM<T> fsm;

      public MonsterStat stat;
      public float Health
      {
            get => stat.health;
            set {
                  if (stat.health != value) {
                        //stat.health = value;
                        photonView.RPC(nameof(RPC_SetStatHealth), RpcTarget.AllBuffered, value);
                  }
            }
      }
      [PunRPC]
      protected virtual void RPC_SetStatHealth(float value)
      {
            stat.health = value;

            // 보스 몬스터면 UI 관리
            switch (monsterType) {
                  case MonsterType.Monstro:
                        GameManager.Instance.uiManager.uiCanvas
                              .GetComponentInChildren<BossSlider>().BossHealth = stat.health;
                        break;
            }
      }

      [HideInInspector] public Vector2 inputVec;

      public GameObject spawnEffect;
      public GameObject liquidEffect;
      protected FlashEffect flashEffect;

      protected bool isSpawned = false;

      public Transform bloodParent;
      public GameObject[] deathBloods;

      // ETC
      [HideInInspector] public SortRendererBy sortRendererBy;

      protected virtual void Awake()
      {
            photonView = GetComponent<PhotonView>();

            rigid = GetComponent<Rigidbody2D>();

            if (spawnEffect == null) {
                  spawnEffect = Resources.Load<GameObject>("FX_MonsterSpawn Variant");
            }
            if (liquidEffect == null) {
                  liquidEffect = Resources.Load<GameObject>("FX_BloodLiquid Variant");
            }
            flashEffect = GetComponent<FlashEffect>();

            if (bloodParent == null) {
                  bloodParent = GameObject.Find("Blood Parent").transform;
            }
            if (deathBloods == null || deathBloods.Length == 0) {
                  deathBloods = new GameObject[3];
                  for (int i = 0; i < deathBloods.Length; i++) {
                        deathBloods[i] = Resources.Load<GameObject>($"Blood{i} Variant");
                  }
            }

            sortRendererBy = new SortRendererBy();
      }

      protected virtual void OnEnable()
      {
            // 처음에는 마스터 클라이언트가 소유하도록
            if (PhotonNetwork.IsMasterClient && photonView.Owner != PhotonNetwork.LocalPlayer) {
                  photonView.RequestOwnership();
            }

            this.gameObject.layer = LayerMask.NameToLayer("Monster");

            IsDeath = false;
            stat.health = stat.maxHealth;

            StartCoroutine(ParticleSystemCoroutine(spawnEffect.GetComponent<ParticleSystem>()));
      }

      protected virtual void OnDisable()
      {
            if (photonView.IsMine) {
                  rigid.velocity = Vector2.zero;
            }
      }

      protected virtual IEnumerator ParticleSystemCoroutine(ParticleSystem _effect)
      {
            ParticleSystem effect = Instantiate(_effect,
                rigid.position + Vector2.down * 0.25f, Quaternion.identity, this.transform);
            effect.transform.localScale = _effect.transform.localScale * 1.5f;
            yield return new WaitUntil(() => effect == null || !effect.isPlaying);

            //isSpawned = true;
            if (photonView.IsMine) photonView.RPC(nameof(RPC_SetisSpawned), RpcTarget.AllBuffered, true);
      }
      [PunRPC]
      protected void RPC_SetisSpawned(bool value)
      {
            isSpawned = value;
      }


      protected bool isHurt = false;
      public bool IsHurt
      {
            get { return isHurt; }
            set {
                  if (isHurt == false) {
                        isHurt = true;
                        photonView.RPC(nameof(RPC_SetisHurt), RpcTarget.Others, true);
                        if (stat.health <= 0) {
                              IsDeath = true;
                              return;
                        }

                        // hurt effect
                        //flashEffect.Flash(1f, 0f, 0f, 1f);
                        foreach (FlashEffect effect in GetComponentsInChildren<FlashEffect>()) {
                              //effect.Flash(1f, 0f, 0f, 1f);
                              photonView.RPC(nameof(effect.Flash), RpcTarget.All, 1f, 0f, 0f, 1f);
                        }

                        isHurt = false;
                        photonView.RPC(nameof(RPC_SetisHurt), RpcTarget.Others, false);
                  }
            }
      }
      [PunRPC]
      protected void RPC_SetisHurt(bool value)
      {
            isHurt = value;
      }

      protected bool isDeath = false;
      public bool IsDeath
      {
            get { return isDeath; }
            set {
                  if (isDeath != value) {
                        isDeath = value;
                        photonView.RPC(nameof(RPC_SetisDeath), RpcTarget.OthersBuffered, value);
                        if (isDeath == true) {
                              //SetAfterDeath();
                              photonView.RPC(nameof(SetAfterDeath), RpcTarget.AllBuffered);
                              //GetComponentInParent<AddRoom>().MonsterCount -= 1;
                              photonView.RPC(nameof(RPC_SetCountAfter), RpcTarget.MasterClient);
                        }
                  }
            }
      }
      [PunRPC]
      protected void RPC_SetisDeath(bool value)
      {
            isDeath = value;
      }
      [PunRPC]
      protected void RPC_SetCountAfter()
      {
            GetComponentInParent<AddRoom>().MonsterCount -= 1;
      }

      protected bool OnDead()
      {
            return stat.health <= 0;
      }

      [PunRPC]
      protected virtual void SetAfterDeath()
      {
            this.gameObject.layer = LayerMask.NameToLayer("Destroyed");

            SpawnBloodEffects();

            this.gameObject.SetActive(false);
      }

      public virtual void SpawnBloodEffects()
      {
            // spawn blood puddle & blood splash
            Instantiate(deathBloods[UnityEngine.Random.Range(0, 3)],
                        this.transform.position, Quaternion.identity, bloodParent);
            Instantiate(liquidEffect, this.transform.position, Quaternion.identity);
      }

      public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
      {
            if (stream.IsWriting) {
                  //stream.SendNext(stat.health);
                  stream.SendNext(inputVec);
            }
            else {
                  //stat.health = (float)stream.ReceiveNext();
                  inputVec = (Vector2)stream.ReceiveNext();
            }
      }


      protected virtual void OnDestroy()
      {
            if (PhotonNetwork.IsMasterClient) {
                  if (GetComponent<PhotonView>() is PhotonView pv) {
                        if (pv.ViewID <= 0) return;
                        if (!pv.IsMine) pv.RequestOwnership();
                        PhotonNetwork.Destroy(gameObject);
                  }
            }
      }
}

[System.Serializable]
public class MonsterStat
{
      public float health;
      public float maxHealth = 5;

      public float moveForce = 10;
      public float maxVelocity = 3; // moveForce/5 + 1

      public int attackDamage;
      public float attackSpeed;

      [Tooltip("= tearRange")]
      public float tearSpeed = 5;
}
