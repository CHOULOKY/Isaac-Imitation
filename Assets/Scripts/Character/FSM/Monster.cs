using System.Collections;
using UnityEngine;

internal enum MonsterType { Charger, Gaper, Pooter, Monstro }
public class Monster<T> : MonoBehaviour where T : class
{
      [SerializeField] private MonsterType monsterType;

      protected Rigidbody2D rigid;

      protected FSM<T> fsm;

      public MonsterStat stat;
      [HideInInspector] public Vector2 inputVec;

      public GameObject spawnEffect;
      public GameObject liquidEffect;
      protected FlashEffect flashEffect;

      protected bool isSpawned = false;

      public Transform bloodParent;
      public GameObject[] deathBloods;

      protected virtual void Awake()
      {
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
      }

      protected virtual void OnEnable()
      {
            this.gameObject.layer = LayerMask.NameToLayer("Monster");

            IsDeath = false;
            stat.health = stat.maxHealth;

            StartCoroutine(ParticleSystemCoroutine(spawnEffect.GetComponent<ParticleSystem>()));
      }

      protected virtual void OnDisable()
      {
            rigid.velocity = Vector2.zero;
      }

      protected virtual IEnumerator ParticleSystemCoroutine(ParticleSystem _effect)
      {
            ParticleSystem effect = Instantiate(_effect,
                rigid.position + Vector2.down * 0.25f, Quaternion.identity, this.transform);
            effect.transform.localScale = _effect.transform.localScale * 1.5f;
            yield return new WaitUntil(() => !effect.isPlaying);

            isSpawned = true;
      }


      protected bool isHurt = false;
      public bool IsHurt
      {
            get { return isHurt; }
            set {
                  if (isHurt == false) {
                        isHurt = true;
                        if (stat.health <= 0) {
                              IsDeath = true;
                              return;
                        }

                        // hurt effect
                        flashEffect.Flash(new Color(1, 0, 0, 1));
                        foreach (FlashEffect effect in GetComponentsInChildren<FlashEffect>()) {
                              effect.Flash(new Color(1, 0, 0, 1));
                        }
                        isHurt = false;
                  }
            }
      }

      protected bool isDeath = false;
      public bool IsDeath
      {
            get { return isDeath; }
            set {
                  if (isDeath != value) {
                        isDeath = value;
                        if (isDeath == true) {
                              SetAfterDeath();
                        }
                  }
            }
      }

      protected bool OnDead()
      {
            return stat.health <= 0;
      }

      protected void SetAfterDeath()
      {
            this.gameObject.layer = LayerMask.NameToLayer("Destroyed");

            SpawnBloodEffects();

            this.gameObject.SetActive(false);
      }

      public void SpawnBloodEffects()
      {
            // spawn blood puddle & blood splash
            Instantiate(deathBloods[UnityEngine.Random.Range(0, 2)],
                        this.transform.position, Quaternion.identity, bloodParent);
            Instantiate(liquidEffect, this.transform.position, Quaternion.identity);
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
