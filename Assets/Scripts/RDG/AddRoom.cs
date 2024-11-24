using System.Collections;
using UnityEngine;

public class AddRoom : MonoBehaviour
{
      private RoomTemplates templates;

      [SerializeField] private bool isClear = false;
      public bool IsClear
      {
            get { return isClear; }
            set {
                  if (isClear != value) {
                        isClear = value;
                        if (isClear) {
                              OnBoolChanged(1, 1, 1, 0.5f);
                        }
                        else {
                              OnBoolChanged(1, 1, 1, 0.2f);
                        }
                  }
            }
      }

      [SerializeField] private bool currentRoom = false;
      public bool CurrentRoom
      {
            get { return currentRoom; }
            set {
                  if (currentRoom != value) {
                        currentRoom = value;
                        if (currentRoom) {
                              OnBoolChanged(1, 1, 0, 0.75f);
                              if (!IsClear) {
                                    SpawnMonsters();
                              }
                        }
                        else {
                              if (IsClear) {
                                    if (IsBossRoom) OnBoolChanged(0, 1, 0, 0.75f);
                                    else OnBoolChanged(1, 1, 1, 0.5f);
                              }
                              else {
                                    OnBoolChanged(1, 1, 1, 0.2f);
                              }
                        }
                  }
            }
      }

      [SerializeField] private bool isBossRoom = false;
      public bool IsBossRoom
      {
            get { return isBossRoom; }
            set {
                  if (isBossRoom != value) {
                        isBossRoom = value;
                        if (isBossRoom) {
                              OnBoolChanged(1, 0, 0, 0.75f);
                        }
                  }
            }
      }

      [SerializeField] private int monsterCount = 0;
      public int MonsterCount
      {
            get => monsterCount;
            set {
                  monsterCount = value;

                  // monsterCount가 0이 되었을 때 동작 수행
                  if (monsterCount == 0) {
                        IsClear = true;
                  }
            }
      }

      [HideInInspector] public int goldRoomDirection;

      private void Awake()
      {
            templates = this.transform.parent.GetComponent<RoomTemplates>();
      }

      private void Start()
      {
            // RDG
            if (templates && !templates.createdRooms) templates.rooms.Add(this.gameObject);

            // Room & Door
            if (this.gameObject == templates.rooms[0]) {
                  goldRoomDirection = UnityEngine.Random.Range(1, 5);
            }

            // Minimap & MonsterCount
            StartCoroutine(SetInitializationRoom());
      }

      private IEnumerator SetInitializationRoom()
      {
            yield return new WaitUntil(() => templates.refreshedRooms);
            yield return null;

            if (this.gameObject == templates.rooms[0]) {
                  IsClear = true;
                  CurrentRoom = true;
            }

            yield return null;

            foreach (Transform child in GetComponentsInChildren<Transform>(true)) {
                  if (child.CompareTag("Monster")) {
                        if (child.name.StartsWith("Gaper_Head")) continue;
                        else MonsterCount += 1;
                  }
            }
            AddRoom bossRoom = templates.rooms[^1].GetComponent<AddRoom>();
            bossRoom.IsBossRoom = true;
            bossRoom.MonsterCount = 1;
      }

      private void OnBoolChanged(float r, float g, float b, float a)
      {
            if (!templates.refreshedRooms) return;

            GameObject miniRoom = GameManager.Instance.minimap.miniRoomsList[templates.rooms.IndexOf(this.gameObject)];
            foreach (SpriteRenderer renderer in miniRoom.GetComponentsInChildren<SpriteRenderer>()) {
                  renderer.color = new(r, g, b, a);
            }
      }

      private void SpawnMonsters()
      {
            if (isBossRoom) {
                  // 보스 몬스터 활성화
                  foreach (Transform child in GetComponentsInChildren<Transform>(true)) {
                        if (child.name.StartsWith("BossRoomSet")) {
                              string bossType = default;
                              switch (GameManager.Instance.CurrentChaper) {
                                    case 1:
                                          bossType = MonsterType.Monstro.ToString();
                                          break;
                              }
                              foreach (Transform child2 in GetComponentsInChildren<Transform>(true)) {
                                    if (child2.name == bossType) {
                                          child2.gameObject.SetActive(true);
                                          break;
                                    }
                              }
                              break;
                        }
                  }
            }
            else {
                  // 일반 몬스터 활성화
                  foreach (Transform child in GetComponentsInChildren<Transform>(true)) {
                        if (child.CompareTag("Monster")) {
                              if (!child.parent.gameObject.activeSelf) {
                                    child.parent.gameObject.SetActive(true);
                                    break;
                              }
                        }
                  }
            }
      }
}
