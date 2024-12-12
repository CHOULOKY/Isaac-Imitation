using Photon.Pun;
using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

public class IsaacTear : Tear
{
      protected override void OnEnable()
      {
            // Head�� ����
            if (!PhotonNetwork.IsMasterClient && photonView.Owner != PhotonNetwork.LocalPlayer)
                  photonView.RequestOwnership();

            base.OnEnable();
      }

      private void OnTriggerEnter2D(Collider2D collision)
      {
            // Body�� return
            if (PhotonNetwork.IsMasterClient) return;

            if (collision.CompareTag("Wall") || collision.CompareTag("Obstacle")) {
                  DisableTear();
            }
            else if (collision.CompareTag("Monster")) {
                  DisableTear();
                  //if (collision.GetComponent<PhotonView>() is PhotonView pv) {
                  //      if (!pv.IsMine) pv.RequestOwnership();
                  //}

                  if (TryGetMonsterFields(collision, out MonoBehaviour script, out PropertyInfo healthProperty,
                      out PropertyInfo isHurtProperty, out FieldInfo monsterTypeField)) {
                        if (monsterTypeField.GetValue(script) is MonsterType monsterType) {
                              //monsterStat.health -= tearDamage;
                              if (healthProperty.GetValue(script) is float currentHealth) { // float Ÿ������ ����
                                    float newHealth = currentHealth - tearDamage;
                                    healthProperty.SetValue(script, newHealth);
                              }
                              //Debug.LogError(monsterStat.health + " + " + tearDamage);
                              isHurtProperty.SetValue(script, true);

                              ApplyKnockToMonster(monsterType, script.GetComponent<Rigidbody2D>());
                        }
                  }
                  else {
                        Debug.LogWarning("stat �ʵ� �Ǵ� IsHurt ������Ƽ�� ã�� �� �����ϴ�.");
                  }
            }
      }

      private bool TryGetMonsterFields(Collider2D collision, out MonoBehaviour script, out PropertyInfo healthProperty,
          out PropertyInfo isHurtProperty, out FieldInfo monsterTypeField)
      {
            // �ʱ�ȭ
            script = null;
            healthProperty = null;
            isHurtProperty = null;
            monsterTypeField = null;

            MonoBehaviour[] scripts = collision.GetComponents<MonoBehaviour>();
            if (scripts.Length <= 1) scripts = collision.GetComponentsInParent<MonoBehaviour>();
            foreach (MonoBehaviour s in scripts) {
                  Type baseType = s.GetType()?.BaseType; // �θ�: Monster
                  if (baseType != null && baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(Monster<>)) {
                        // stat �ʵ�� IsHurt ������Ƽ ã��
                        healthProperty = baseType.GetProperty("Health", BindingFlags.Instance | BindingFlags.Public);
                        isHurtProperty = baseType.GetProperty("IsHurt", BindingFlags.Instance | BindingFlags.Public);
                        // MonsterType ������ �ʵ� ã��
                        monsterTypeField = baseType.GetField("monsterType", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

                        if (healthProperty != null && isHurtProperty != null && monsterTypeField != null) {
                              script = s;
                              return true;
                        }
                        break;
                  }
            }
            return false;
      }

      private void ApplyKnockToMonster(MonsterType monsterType, Rigidbody2D monsterRigid)
      {
            float adjustedKnockPower = knockPower;
            switch (monsterType) {
                  case MonsterType.Gaper:
                        break;
                  case MonsterType.Pooter:
                        adjustedKnockPower *= 2;
                        break;
                  default:
                        // Knockback not applied
                        return;
            }

            if (monsterRigid.GetComponent<PhotonView>() is PhotonView view) {
                  if (!view.IsMine) view.RequestOwnership();
            }
            monsterRigid.velocity = Vector2.zero;
            monsterRigid.AddForce((monsterRigid.position - rigid.position).normalized * adjustedKnockPower, ForceMode2D.Impulse);
      }
}
