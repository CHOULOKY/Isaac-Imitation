using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

// Photon applied complete
namespace ObstacleSpace
{
      public class Obstacle : MonoBehaviour
      {
            [Tooltip("The current collider is the target's script name to ignore the collision (2D)")]
            [SerializeField] protected List<string> targetNames; // 충돌을 무시할 대상 이름


            protected virtual void Start()
            {
                  // 현재 오브젝트가 있는 방 찾기
                  if (transform.parent.parent.TryGetComponent<AddRoom>(out var room)) {
                        // 현재 방에서 이름이 Monsters인 오브젝트 찾기
                        Transform monsters = null;
                        foreach (Transform child in room.transform) {
                              if (child.name == "Monsters") {
                                    monsters = child;
                                    break;
                              }
                        }

                        if (monsters == null) {
                              Debug.LogWarning("No object named 'Monsters' found in the room.");
                              return;
                        }

                        // Monsters 오브젝트의 비활성 포함 모든 자식 중
                        Collider2D thisCollider = GetComponent<Collider2D>();
                        foreach (Transform monster in monsters.GetComponentsInChildren<Transform>(true)) {
                              // targetName과 일치하는 스크립트를 가진 오브젝트 찾기
                              if (monster.GetComponent<MonoBehaviour>() is MonoBehaviour targetScript) {
                                    if (targetNames.Contains(targetScript.GetType().Name)) {
                                          // targetNames에 일치하는 이름이 있으면 현재 오브젝트와 충돌 무시 처리
                                          Physics2D.IgnoreCollision(thisCollider, targetScript.GetComponent<Collider2D>(), true);
                                    }
                              }
                        }
                  }
            }
      }
}
